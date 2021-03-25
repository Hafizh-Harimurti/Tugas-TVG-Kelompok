using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Render
{
    internal class RendererClient
    {
        public static RendererClient Client { get; private set; }
        public event Action<ModelData> OnModelReceived;
        public event Action<SceneData> OnSceneDataReceived;
        public bool _sychronized = false;

        private StreamReader _streamReader;

        public static void Setup(string pipeHandle)
        {
            Client = new RendererClient(pipeHandle);
        }

        private RendererClient(string pipeHandle)
        {
            var pipeClient = new AnonymousPipeClientStream(PipeDirection.In, pipeHandle);

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
                        if (data.Contains("[ModelData]"))
                        {
                            data = data.Replace("[ModelData]", "");
                            OnModelReceived.Invoke(ModelData.Deserialize(data));
                        }
                        else if (data.Contains("[SceneData]"))
                        {
                            data = data.Replace("[SceneData]", "");
                            OnSceneDataReceived.Invoke(SceneData.Deserialize(data));
                        }
                    }
                }
            });
        }
    }
}
