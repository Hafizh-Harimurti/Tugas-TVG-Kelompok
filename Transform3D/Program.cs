using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Transform3D
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var CubeMesh = new MeshGeometry3D();

            CubeMesh.Positions = new Point3DCollection(new List<Point3D>() {
                new Point3D(0.5, 0.5, 0.5),
                new Point3D(-0.5, 0.5, 0.5),
                new Point3D(-0.5, -0.5, 0.5),
                new Point3D(0.5, -0.5, 0.5),
                new Point3D(0.5, 0.5, -0.5),
                new Point3D(-0.5, 0.5, -0.5),
                new Point3D(-0.5, -0.5, -0.5),
                new Point3D(0.5, -0.5, -0.5)
            });

            CubeMesh.TriangleIndices = new Int32Collection(new int[] {
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
            });

            var CubeModel = new GeometryModel3D();
            CubeModel.Geometry = CubeMesh;
            CubeModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));

            var app = new Application();
            app.Run(new RenderWindow(CubeModel));

            Console.ReadKey();
        }
    }
}
