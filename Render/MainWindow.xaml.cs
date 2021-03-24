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
            
            Title = "3D Render";
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Width = 900;
            Height = 600;

            var light = new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1));

            var camera = new PerspectiveCamera();
            camera.FarPlaneDistance = 20;
            camera.NearPlaneDistance = 1;
            camera.FieldOfView = 45;
            camera.Position = new Point3D(2, 2, 3);
            camera.LookDirection = new Vector3D(-2, -2, -3);
            camera.UpDirection = new Vector3D(0, 1, 0);

            var modelGroup = new Model3DGroup();
            modelGroup.Children.Add(light);

            var modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = modelGroup;

            var viewport3D = new Viewport3D();
            viewport3D.Camera = camera;
            viewport3D.Children.Add(modelVisual3D);

            //StartListenForModeChange(ModelGroup);

            Content = viewport3D;
            
            RendererClient.Setup(Environment.GetCommandLineArgs()[1]);
            var client = RendererClient.Client;

            client.OnModelReceived += (modelData) =>
            {
                modelGroup.Dispatcher.Invoke(() =>
                {
                    if (_previousModel != null)
                    {
                        modelGroup.Children.Remove(_previousModel);
                    }

                    var model = new GeometryModel3D();

                    var mesh = new MeshGeometry3D();
                    mesh.Positions = new Point3DCollection(modelData.Points);
                    mesh.TriangleIndices = new Int32Collection(modelData.Triangles);

                    model.Geometry = mesh;
                    model.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Green));

                    modelGroup.Children.Add(model);
                    _previousModel = model;
                });
            };
        }
    }
}
