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
            var server = new RendererServer();

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

            // Transform.
            var newMeshPoints = Transformation.Transform(MeshPoints, new List<Transformation>()
            {
                new Transformation()
                {
                    TransformName = "RotateZ",
                    theta = DegreesToRadians(25)
                }
            });

            //CubeMesh.Positions = new Point3DCollection(NewMeshPoints);

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

            var modelData = new ModelData()
            {
                Points = MeshPoints,
                Triangles = triangleIndices
            };
            var newModelData = new ModelData()
            {
                Points = newMeshPoints,
                Triangles = triangleIndices
            };

            server.Render(modelData);

            await Task.Delay(5000);

            server.Render(newModelData);

            Console.ReadKey();

            server.Close();
        }
    }
}
