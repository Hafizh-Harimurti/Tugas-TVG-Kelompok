using Render;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transform3D
{
    internal class RendererServer
    {
        private Process _pipeClient = new Process();
        private StreamWriter _streamWriter;
        private AnonymousPipeServerStream _pipeServerStream;
        private const ConsoleColor _consoleColor = ConsoleColor.Blue;
        private Queue<string> _sendQueue = new Queue<string>();

        public bool LoggingEnabled { get; set; } = true;

        public RendererServer()
        {
            Log("Starting up renderer...");
            SetupAnonymousPipes();
            ClearConsoleLine();
            SendQueuedPayloads();
            Log("Renderer started.");
        }

        public void Render(ModelData modelData)
        {
            _sendQueue.Enqueue($"[ModelData]{modelData.Serialize()}");
        }

        public async Task RenderAwaitableAsync(ModelData modelData)
        {
            await SendPayload($"[ModelData]{modelData.Serialize()}");
        }

        public void SetScene(SceneData sceneData)
        {
            _sendQueue.Enqueue($"[SceneData]{sceneData.Serialize()}");
        }

        public async Task SetSceneAwaitableAsync(SceneData sceneData)
        {
            await SendPayload($"[SceneData]{sceneData.Serialize()}");
        }

        public void Close()
        {
            _pipeClient.Kill();
        }

        private async void SendQueuedPayloads()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (_sendQueue.Count > 0)
                    {
                        await SendPayload(_sendQueue.Dequeue());
                    }
                }
            });
        }

        private async Task SendPayload(string payload)
        {
            await Task.Run(() =>
            {
                try
                {
                    _streamWriter.WriteLine(payload);

                    _pipeServerStream.WaitForPipeDrain();

                    Log("Render data sent to the renderer.");
                }
                catch (Exception e)
                {
                    Log($"Error: {e.Message}");
                }
            });
        }

        private void SetupAnonymousPipes()
        {
            _pipeClient.StartInfo.FileName = "Render.exe";

            _pipeServerStream = new AnonymousPipeServerStream(
                PipeDirection.Out,
                HandleInheritability.Inheritable);

            // Pass the client process a handle to the server.
            _pipeClient.StartInfo.Arguments = _pipeServerStream.GetClientHandleAsString();
            _pipeClient.StartInfo.UseShellExecute = false;
            _pipeClient.Start();

            _pipeServerStream.DisposeLocalCopyOfClientHandle();

            _streamWriter = new StreamWriter(_pipeServerStream)
            {
                AutoFlush = true
            };

            _streamWriter.WriteLine("[SYNC]");
            _pipeServerStream.WaitForPipeDrain();
        }

        private void Log(string message)
        {
            if (LoggingEnabled)
            {
                Console.ForegroundColor = _consoleColor;
                Console.WriteLine($"[Renderer] {message}");
                Console.ForegroundColor = ConsoleHelper.DefaultConsoleForegroundColor;
            }
        }

        private void ClearConsoleLine()
        {
            if (LoggingEnabled)
            {
                int currentLine = Console.CursorTop;
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, currentLine - 1);
            }
        }
    }
}
