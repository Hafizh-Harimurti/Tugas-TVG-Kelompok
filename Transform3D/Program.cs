using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Transform3D;

namespace Render
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var renderer = new RendererServer();

            double DegreesToRadians(double degrees) => degrees * (Math.PI / 180);

            var MeshPoints = new List<Point3D>() {
                new Point3D(0.5, 0.5, 0.5),
                new Point3D(-0.5, 0.5, 0.5),
                new Point3D(-0.5, -0.5, 0.5),
                new Point3D(0.5, -0.5, 0.5),
                new Point3D(0.5, 0.5, -0.5),
                new Point3D(-0.5, 0.5, -0.5),
                new Point3D(-0.5, -0.5, -0.5),
                new Point3D(0.5, -0.5, -0.5)
            };

            var triangleIndices = new int[] {
                // Front
                0,1,2,
                0,2,3,
                // Back
                4,7,6,
                4,6,5,
                // Right
                4,0,3,
                4,3,7,
                // Left
                1,5,6,
                1,6,2,
                // Top
                1,0,4,
                1,4,5,
                // Bottom
                2,6,7,
                2,7,3
            };

            var newMeshPoints = MeshPoints;
            var rotateModel = Task.Run(async () =>
            {
                while (true)
                {
                    newMeshPoints = Transformation.Transform(newMeshPoints, new List<Transformation>()
                    {
                        new Transformation()
                        {
                            TransformName = "RotateZ",
                            theta = DegreesToRadians(0.5)
                        }
                    });

                    var modelData = new ModelData()
                    {
                        Points = newMeshPoints,
                        Triangles = triangleIndices
                    };

                    renderer.Render(modelData);

                    await Task.Delay(5);
                }
            });

            var changeScene = Task.Run(async () =>
            {
                while (true)
                {
                    var scenes = new List<SceneData>() 
                    {
                        new SceneData()
                        {
                            LookPosition = new Point3D(3, 3, 4),
                            AmbientLightIntensity = 0.2,
                            CameraLightIntensity = 0.2,
                            ModelColorARGB = Colors.GreenYellow
                        },
                        new SceneData()
                        {
                            LookPosition = new Point3D(5, -6, 7),
                            AmbientLightIntensity = 0.6,
                            CameraLightIntensity = 0.6,
                            ModelColorARGB = Colors.Coral
                        },
                        new SceneData()
                        {
                            LookPosition = new Point3D(-3, -3, -3),
                            AmbientLightIntensity = 0.8,
                            CameraLightIntensity = 0.8,
                            ModelColorARGB = Colors.PaleVioletRed
                        }
                    };

                    foreach (var scene in scenes)
                    {
                        renderer.SetScene(scene);

                        await Task.Delay(2500);
                    }
                }
            });

            await Task.WhenAll(rotateModel, changeScene);

            Console.ReadKey();

            renderer.Close();
        }
    }
}
