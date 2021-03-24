using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Render
{
    internal class RendererClient
    {
        public static RendererClient Client { get; private set; }
        public event Action<ModelData> OnModelReceived;
        public bool _sychronized = false;

        private StreamReader _streamReader;

        public static void Setup(string pipeHandle)
        {
            Client = new RendererClient(pipeHandle);
        }

        private RendererClient(string pipeHandle)
        {
            var pipeClient = new AnonymousPipeClientStream(PipeDirection.In, pipeHandle);
            Debug.WriteLine($"[CLIENT] Current TransmissionMode: { pipeClient.TransmissionMode }.");

            _streamReader = new StreamReader(pipeClient);

            // Read from stream.
            Task.Run(async () =>
            {
                while (true)
                {
                    var data = await _streamReader.ReadLineAsync();

                    // Wait for synchronization message.
                    while (data != "[SYNC]" && !_sychronized);

                    if (!_sychronized)
                    {
                        _sychronized = true;
                        continue;
                    }

                    if (data != null)
                    {
                        OnModelReceived.Invoke(ModelData.Deserialize(data));
                    }
                }
            });
        }
    }
}
