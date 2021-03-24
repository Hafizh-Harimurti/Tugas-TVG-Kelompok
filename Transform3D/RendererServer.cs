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

        public RendererServer()
        {
            Log("Starting up renderer...");
            SetupAnonymousPipes();
            ClearConsoleLine();
            Log("Renderer started.");
        }

        public void Render(ModelData modelData)
        {
            try
            {
                var payload = modelData.Serialize();
                _streamWriter.WriteLine(payload);

                _pipeServerStream.WaitForPipeDrain();

                Log("Data sent to renderer.");
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
            var color = Console.ForegroundColor;
            Console.ForegroundColor = _consoleColor;
            Console.WriteLine($"[Renderer] {message}");
            Console.ForegroundColor = color;
        }
        private void ClearConsoleLine()
        {
            int currentLine = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLine - 1);
        }
    }
}
