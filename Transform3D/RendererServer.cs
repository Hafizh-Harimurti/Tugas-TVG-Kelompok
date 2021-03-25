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
            SendPayloads();
            Log("Renderer started.");
        }

        public void Render(ModelData modelData)
        {
            try
            {
                _sendQueue.Enqueue($"[ModelData]{modelData.Serialize()}");

                Log("Model data sent to the renderer.");
            }
            catch (IOException e)
            {
                Log($"Error: {e.Message}");
            }
        }

        public void SetScene(SceneData sceneData)
        {
            try
            {
                _sendQueue.Enqueue($"[SceneData]{sceneData.Serialize()}");

                Log("Scene data sent to the renderer.");
            }
            catch (IOException e)
            {
                Log($"Error: {e.Message}");
            }
        }

        public void Close()
        {
            _pipeClient.Kill();
        }

        private async void SendPayloads()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if (_sendQueue.Count > 0)
                    {
                        _streamWriter.WriteLine(_sendQueue.Dequeue());

                        _pipeServerStream.WaitForPipeDrain();
                    }
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
                var color = Console.ForegroundColor;
                Console.ForegroundColor = _consoleColor;
                Console.WriteLine($"[Renderer] {message}");
                Console.ForegroundColor = color;
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
