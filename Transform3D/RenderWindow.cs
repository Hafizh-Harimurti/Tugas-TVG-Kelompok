using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Transform3D
{
    public class RenderWindow : Window
    {
        public RenderWindow(GeometryModel3D model)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Width = 900;
            Height = 600;

            // Create scene.
            var Light = new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1));
            
            var Camera = new PerspectiveCamera();
            Camera.FarPlaneDistance = 20;
            Camera.NearPlaneDistance = 1;
            Camera.FieldOfView = 45;
            Camera.Position = new Point3D(2, 2, 3);
            Camera.LookDirection = new Vector3D(-2, -2, -3);
            Camera.UpDirection = new Vector3D(0, 1, 0);

            var ModelGroup = new Model3DGroup();
            ModelGroup.Children.Add(model);
            ModelGroup.Children.Add(Light);

            var ModelVisual3D = new ModelVisual3D();
            ModelVisual3D.Content = ModelGroup;

            var Viewport3D = new Viewport3D();
            Viewport3D.Camera = Camera;
            Viewport3D.Children.Add(ModelVisual3D);

            Content = Viewport3D;
        }
    }
}
