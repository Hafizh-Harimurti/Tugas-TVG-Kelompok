using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Render
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GeometryModel3D _previousModel = null;
        public MainWindow()
        {
            InitializeComponent();
            
            Title = "3D Renderer";
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Width = 900;
            Height = 600;

            var lookPosition = new Point3D(3, 3, 4);
            var lookDirection = new Vector3D(lookPosition.X * -1, lookPosition.Y * -1, lookPosition.Z * -1);

            var lightIntensity = 0.6;

            var ambientLight = new AmbientLight(new Color() 
            {
                A = 255,
                R = Convert.ToByte(255 * lightIntensity),
                G = Convert.ToByte(255 * lightIntensity),
                B = Convert.ToByte(255 * lightIntensity)
            });
            //var light = new SpotLight(Colors.White, lookPosition, lookDirection, 171, 170);
            //var light = new PointLight(Colors.White, lookPosition);
            var light = new DirectionalLight(Colors.White, lookDirection);

            var camera = new PerspectiveCamera();
            camera.FarPlaneDistance = 20;
            camera.NearPlaneDistance = 1;
            camera.FieldOfView = 45;
            camera.Position = lookPosition;
            camera.LookDirection = lookDirection;
            camera.UpDirection = new Vector3D(0, 1, 0);

            var modelGroup = new Model3DGroup();
            modelGroup.Children.Add(ambientLight);
            modelGroup.Children.Add(light);

            var modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = modelGroup;

            var viewport3D = new Viewport3D();
            viewport3D.Camera = camera;
            viewport3D.Children.Add(modelVisual3D);

            Content = viewport3D;
            
            RendererClient.Setup(Environment.GetCommandLineArgs()[1]);
            var client = RendererClient.Client;

            client.OnModelReceived += (modelData) =>
            {
                modelGroup.Dispatcher.Invoke(() =>
                {
                    var model = new GeometryModel3D();

                    // Setup mesh.
                    model.Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection(modelData.Points),
                        TriangleIndices = new Int32Collection(modelData.Triangles)
                    };

                    // Setup material.
                    model.Material = new MaterialGroup
                    {
                        Children = new MaterialCollection(new List<Material>()
                        {
                            new DiffuseMaterial(new SolidColorBrush(Colors.DodgerBlue))
                        })
                    };

                    modelGroup.Children.Add(model);
                    if (_previousModel != null)
                    {
                        modelGroup.Children.Remove(_previousModel);
                    }
                    _previousModel = model;
                });
            };
        }
    }
}
