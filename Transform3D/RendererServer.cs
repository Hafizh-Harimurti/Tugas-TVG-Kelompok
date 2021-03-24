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

        public RendererServer()
        {
            SetupAnonymousPipes();
        }

        public void Render(ModelData modelData)
        {
            try
            {
                var payload = modelData.Serialize();
                _streamWriter.WriteLine(payload);

                _pipeServerStream.WaitForPipeDrain();

                Console.WriteLine("Data sent.");
            }
            catch (IOException e)
            {
                Console.WriteLine("[SERVER] Error: {0}", e.Message);
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
            
            Console.WriteLine("[SERVER] Current TransmissionMode: {0}.", _pipeServerStream.TransmissionMode);

            // Pass the client process a handle to the server.
            _pipeClient.StartInfo.Arguments = _pipeServerStream.GetClientHandleAsString();
            _pipeClient.StartInfo.UseShellExecute = false;
            _pipeClient.Start();

            _pipeServerStream.DisposeLocalCopyOfClientHandle();

            _streamWriter = new StreamWriter(_pipeServerStream);
            _streamWriter.AutoFlush = true;
            _streamWriter.WriteLine("[SYNC]");
            _pipeServerStream.WaitForPipeDrain();

            _pipeClient.Exited += (sender, e) =>
            {
                _pipeClient.Close();
                Console.WriteLine("[SERVER] Client quit. Server terminating.");
            };
        }
    }
}
