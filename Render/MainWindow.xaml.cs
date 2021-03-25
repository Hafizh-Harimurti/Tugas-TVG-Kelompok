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
        private GeometryModel3D _currentModel = new GeometryModel3D();
        private Color _modelColor = Colors.DodgerBlue;
        private PerspectiveCamera _camera = new PerspectiveCamera()
        {
            FarPlaneDistance = 20,
            NearPlaneDistance = 1,
            FieldOfView = 45,
            UpDirection = new Vector3D(0, 1, 0)
        };
        private AmbientLight _ambientLight = new AmbientLight();
        private DirectionalLight _cameraLight = new DirectionalLight();

        public MainWindow()
        {
            InitializeComponent();
            
            Title = "3D Renderer - Control from the console window.";
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Width = 900;
            Height = 600;

            var modelGroup = new Model3DGroup();
            modelGroup.Children.Add(_ambientLight);
            modelGroup.Children.Add(_cameraLight);

            var modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = modelGroup;

            var viewport3D = new Viewport3D();
            viewport3D.Camera = _camera;
            viewport3D.Children.Add(modelVisual3D);

            Content = viewport3D;
            
            RendererClient.Setup(Environment.GetCommandLineArgs()[1]);
            var client = RendererClient.Client;

            client.OnModelReceived += (modelData) =>
            {
                modelGroup.Dispatcher.Invoke(() =>
                {
                    var model = new GeometryModel3D
                    {
                        // Setup mesh.
                        Geometry = new MeshGeometry3D
                        {
                            Positions = new Point3DCollection(modelData.Points),
                            TriangleIndices = new Int32Collection(modelData.Triangles)
                        },

                        // Setup material.
                        Material = new MaterialGroup
                        {
                            Children = new MaterialCollection(new List<Material>()
                            {
                                new DiffuseMaterial(new SolidColorBrush(_modelColor))
                            })
                        }
                    };

                    if (_currentModel != null)
                    {
                        modelGroup.Children.Remove(_currentModel);
                    }
                    modelGroup.Children.Add(model);
                    _currentModel = model;
                });
            };

            client.OnSceneDataReceived += (sceneData) =>
            {
                var lookDirection = new Vector3D(
                    sceneData.LookPosition.X * -1,
                    sceneData.LookPosition.Y * -1,
                    sceneData.LookPosition.Z * -1);

                // Setup look position.
                _camera.Dispatcher.Invoke(() =>
                {
                    _camera.Position = sceneData.LookPosition;
                    _camera.LookDirection = lookDirection;
                });

                // Setup ambient light intensity.
                _ambientLight.Dispatcher.Invoke(() =>
                {
                    _ambientLight.Color = new Color()
                    {
                        A = 255,
                        R = Convert.ToByte(255 * sceneData.AmbientLightIntensity),
                        G = Convert.ToByte(255 * sceneData.AmbientLightIntensity),
                        B = Convert.ToByte(255 * sceneData.AmbientLightIntensity)
                    };
                });

                // Setup camera light intensity.
                _cameraLight.Dispatcher.Invoke(() =>
                {
                    _cameraLight.Color = new Color()
                    {
                        A = 255,
                        R = Convert.ToByte(255 * sceneData.CameraLightIntensity),
                        G = Convert.ToByte(255 * sceneData.CameraLightIntensity),
                        B = Convert.ToByte(255 * sceneData.CameraLightIntensity)
                    };
                    _cameraLight.Direction = lookDirection;
                });

                // Setup model color.
                _currentModel.Dispatcher.Invoke(() =>
                {
                    _modelColor = sceneData.ModelColorARGB;
                });
            };
        }
    }
}
