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
        private static ModelData _model = null;
        private static ModelData _preTransformedModel = null;
        private static ModelData _originalModel = null;
        private static SceneData _scene = null;
        private static RendererServer _renderer;
        private static CancellationTokenSource _inspectionCancellation = null;
        private static List<Transformation> _transformations = null;

        async static Task Main(string[] args)
        {
            try
            {
                ConsoleHelper.Initialize();

                // Print identity.
                Console.WriteLine("Tugas Kelompok Teknik Visualisasi Grafis - Transformasi 3D");
                Console.WriteLine("Identitas Anggota Kelompok :");
                Console.WriteLine("Daffa Bil Nadzary                19/439811/TK/48541");
                Console.WriteLine("Firdaus Bisma Suryakusuma        19/444051/TK/49247");
                Console.WriteLine("Hafizh Aradhana Harimurti        19/444053/TK/49249");
                Console.WriteLine("Please wait a while.");
                Console.WriteLine();
                await Task.Delay(3000);

                // Start up renderer
                _renderer = new RendererServer();

                Console.WriteLine();

                // Load default model
                Console.WriteLine("Loading default model(cube)...");
                var modelFileContent = File.ReadAllText($"./Models/default-cube.txt");
                _model = ModelData.Deserialize(modelFileContent);
                _originalModel = _model.Clone();
                await _renderer.RenderAwaitableAsync(_originalModel);

                // Load default scene
                Console.WriteLine("Loading default scene...");
                var sceneFileContent = File.ReadAllText($"./Scenes/default-scene.txt");
                _scene = SceneData.Deserialize(sceneFileContent);
                await _renderer.SetSceneAwaitableAsync(_scene);

                Console.WriteLine();

                // Prompt action
                while (true)
                {
                    Console.WriteLine("Select an action :");
                    Console.WriteLine("[0] Load model");
                    Console.WriteLine("[1] Load scene");
                    Console.WriteLine("[2] Save model");
                    Console.WriteLine("[3] Save scene");
                    Console.WriteLine("[4] Perform a single transformation");
                    Console.WriteLine("[5] Chain multiple transformations");
                    Console.WriteLine("[6] Inspect model");
                    Console.WriteLine("[7] Stop inspecting model");
                    Console.WriteLine("[8] Reset model");
                    Console.WriteLine("[9] Exit");
                    Console.Write("Select : ");
                    var ans = Console.ReadLine();
                    Console.WriteLine();

                    if (ans == "9") break;

                    switch (ans)
                    {
                        case "0":
                            if (_inspectionCancellation != null) _inspectionCancellation.Cancel();
                            await LoadModel();
                            break;
                        case "1":
                            if (_inspectionCancellation != null) _inspectionCancellation.Cancel();
                            await LoadScene();
                            break;
                        case "2":
                            _model.Serialize();
                            Console.WriteLine("Input desired model name without .txt extension:");
                            var _modelName = Console.ReadLine();
                            SaveModel($"./Models/{_modelName}.txt", _model);
                            if (_inspectionCancellation != null) _inspectionCancellation.Cancel();
                            break;
                        case "3":
                            _scene.Serialize();
                            Console.WriteLine("Input desired scene name without .txt extension:");
                            var _sceneName = Console.ReadLine();
                            SaveScene($"./Models/{_sceneName}.txt", _scene);
                            if (_inspectionCancellation != null) _inspectionCancellation.Cancel();
                            break;
                        case "4":
                            _preTransformedModel = _model.Clone();
                            BeginTransformation(false);
                            await _renderer.RenderAwaitableAsync(_model);
                            if (_inspectionCancellation != null) _inspectionCancellation.Cancel();
                            break;
                        case "5":
                            _preTransformedModel = _model.Clone();
                            BeginTransformation(true);
                            await _renderer.RenderAwaitableAsync(_model);
                            if (_inspectionCancellation != null) _inspectionCancellation.Cancel();
                            break;
                        case "6":
                            if (_inspectionCancellation != null) _inspectionCancellation.Cancel();
                            _inspectionCancellation = new CancellationTokenSource();
                            InspectModel(_inspectionCancellation.Token);
                            break;
                        case "7":
                            _inspectionCancellation.Cancel();
                            _inspectionCancellation = null;
                            break;
                        case "8":
                            _model = _originalModel;
                            await _renderer.RenderAwaitableAsync(_model);
                            if (_inspectionCancellation != null) _inspectionCancellation.Cancel();
                            break;
                        default:
                            if (_inspectionCancellation != null) _inspectionCancellation.Cancel();
                            WriteError("Action not recognized.");
                            break;
                    }

                    Console.WriteLine();
                }

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();

                _renderer.Close();
            }
            catch (Exception e)
            {
                WriteError(e.Message);
            }
        }

        private static async Task LoadModel()
        {
            try
            {
                Console.WriteLine("Select a file (located in ./Models/) :");

                // List files.
                foreach (var file in Directory.GetFiles("./Models/"))
                {
                    Console.WriteLine(Path.GetFileName(file));
                }
                Console.WriteLine();
                Console.Write("File (e.g. foobar.txt) : ");
                var filename = Console.ReadLine();

                var fileContent = File.ReadAllText($"./Models/{filename}");

                _model = ModelData.Deserialize(fileContent);
                _originalModel = _model.Clone();
                await _renderer.RenderAwaitableAsync(_originalModel);

                WriteSuccess("Model loaded.");
            }
            catch (Exception e)
            {
                WriteError($"Error loading model. Error : {e.Message}");
            }
        }

        private static void SaveModel(string path, ModelData model)
        {
            File.WriteAllText(path, model.Serialize());
        }

        private static async Task LoadScene()
        {
            try
            {
                Console.WriteLine("Select a file (located in ./Scenes/) :");

                // List files.
                foreach (var file in Directory.GetFiles("./Scenes/"))
                {
                    Console.WriteLine(Path.GetFileName(file));
                }
                Console.WriteLine();
                Console.Write("File (e.g. foobar.txt) : ");
                var filename = Console.ReadLine();

                var fileContent = File.ReadAllText($"./Scenes/{filename}");

                _scene = SceneData.Deserialize(fileContent);
                await _renderer.SetSceneAwaitableAsync(_scene);

                WriteSuccess("Scene loaded.");
            }
            catch (Exception e)
            {
                WriteError($"Error loading scene. Error : {e.Message}");
            }
        }

        private static void SaveScene(string path, SceneData scene)
        {
            File.WriteAllText(path, scene.Serialize());
        }

        private static void InspectModel(CancellationToken cancellationToken)
        {
            Console.WriteLine("Select inspection mode :");
            Console.WriteLine("[0] Rotate along the X axis");
            Console.WriteLine("[1] Rotate along the Y axis");
            Console.WriteLine("[2] Rotate along the Z axis");
            Console.WriteLine();
            Console.Write("Select : ");
            var answer = Console.ReadLine();

            switch (answer)
            {
                case "0":
                    StartInspectionAnimation(InspectionAnimations.RotateAlongXAxis, cancellationToken);
                    break;
                case "1":
                    StartInspectionAnimation(InspectionAnimations.RotateAlongYAxis, cancellationToken);
                    break;
                case "2":
                    StartInspectionAnimation(InspectionAnimations.RotateAlongZAxis, cancellationToken);
                    break;
                default:
                    WriteError("Action not recognized");
                    return;
            }

            WriteSuccess("Inspection animation started");
        }

        private static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleHelper.DefaultConsoleForegroundColor;
        }

        private static void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleHelper.DefaultConsoleForegroundColor;
        }

        private static async void StartInspectionAnimation(InspectionAnimations inspectionAnimation, CancellationToken cancellationToken)
        {
            // Save model state
            var savedModel = _model.Clone();

            _renderer.LoggingEnabled = false;
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _model = savedModel;
                        await _renderer.RenderAwaitableAsync(_model);
                        _renderer.LoggingEnabled = true;
                        break;
                    }

                    double DegreesToRadians(double degrees) => degrees * (Math.PI / 180);

                    Transformation transformation = null;
                    switch (inspectionAnimation)
                    {
                        case InspectionAnimations.RotateAlongXAxis:
                            transformation = new Transformation()
                            {
                                TransformName = "RotateX",
                                theta = DegreesToRadians(0.5)
                            };
                            break;
                        case InspectionAnimations.RotateAlongYAxis:
                            transformation = new Transformation()
                            {
                                TransformName = "RotateY",
                                theta = DegreesToRadians(0.5)
                            };
                            break;
                        case InspectionAnimations.RotateAlongZAxis:
                            transformation = new Transformation()
                            {
                                TransformName = "RotateZ",
                                theta = DegreesToRadians(0.5)
                            };
                            break;
                    }

                    _model.Points = Transformation.Transform((List<Point3D>)_model.Points, new List<Transformation>()
                    {
                        transformation
                    });
                    
                    _renderer.Render(_model);

                    await Task.Delay(5);
                }
            });
        }

        private static void BeginTransformation(bool _isMultipleTransform)
        {
            _transformations = new List<Transformation>();
            Transformation _newTransformation;
            bool _needXValue = false;
            bool _needYValue = false;
            bool _needZValue = false;
            bool _needXYZPivot = false;
            bool _needXYZPivot2 = false;
            bool _needTheta = false;
            try
            {
                do
                {
                    _newTransformation = new Transformation();
                    _needXValue = false;
                    _needYValue = false;
                    _needZValue = false;
                    _needXYZPivot = false;
                    _needXYZPivot2 = false;
                    _needTheta = false;
                    Console.WriteLine("Choose the transformation:");
                    Console.WriteLine("[0] Translation");
                    Console.WriteLine("[1] Scale");
                    Console.WriteLine("[2] Rotation");
                    Console.WriteLine("[3] XY Shear");
                    Console.WriteLine("[4] YZ Shear");
                    Console.WriteLine("[5] XZ Shear");
                    Console.WriteLine("[6] Stop Adding Transformation");
                    Console.WriteLine();
                    Console.Write("Select : ");
                    var _transformationChoice = Console.ReadLine();
                    switch (_transformationChoice)
                    {
                        case "0":
                            {
                                _newTransformation.TransformName = "Translate";
                                _needXValue = true;
                                _needYValue = true;
                                _needZValue = true;
                                break;
                            }
                        case "1":
                            {
                                _newTransformation.TransformName = "Scale";
                                _needXValue = true;
                                _needYValue = true;
                                _needZValue = true;
                                _needXYZPivot = true;
                                break;
                            }
                        case "2":
                            {
                                _newTransformation.TransformName = "Rotate";
                                _needXYZPivot = true;
                                _needXYZPivot2 = true;
                                _needTheta = true;
                                break;
                            }
                        case "3":
                            {
                                _newTransformation.TransformName = "ShearXY";
                                _needXValue = true;
                                _needYValue = true;
                                break;
                            }
                        case "4":
                            {
                                _newTransformation.TransformName = "ShearYZ";
                                _needYValue = true;
                                _needZValue = true;
                                break;
                            }
                        case "5":
                            {
                                _newTransformation.TransformName = "ShearXZ";
                                _needXValue = true;
                                _needZValue = true;
                                break;
                            }
                        case "6":
                            {
                                _isMultipleTransform = false;
                                break;
                            }
                    }
                    if (_needXValue)
                    {
                        Console.WriteLine();
                        Console.Write("Input X value : ");
                        _newTransformation.amountX = Convert.ToDouble(Console.ReadLine());
                    }
                    if (_needYValue)
                    {
                        Console.WriteLine();
                        Console.Write("Input Y value : ");
                        _newTransformation.amountY = Convert.ToDouble(Console.ReadLine());
                    }
                    if (_needZValue)
                    {
                        Console.WriteLine();
                        Console.Write("Input Z value : ");
                        _newTransformation.amountZ = Convert.ToDouble(Console.ReadLine());
                    }
                    if (_needXYZPivot)
                    {
                        Console.WriteLine();
                        Console.Write("Input X for pivot point:");
                        _newTransformation.pivotX1 = Convert.ToDouble(Console.ReadLine());
                        Console.WriteLine();
                        Console.Write("Input Y for pivot point:");
                        _newTransformation.pivotY1 = Convert.ToDouble(Console.ReadLine());
                        Console.WriteLine();
                        Console.Write("Input Z for pivot point:");
                        _newTransformation.pivotZ1 = Convert.ToDouble(Console.ReadLine());
                    }
                    if (_needXYZPivot2)
                    {
                        Console.WriteLine();
                        Console.Write("Input X for another pivot point to create pivot axis:");
                        _newTransformation.pivotX2 = Convert.ToDouble(Console.ReadLine());
                        Console.WriteLine();
                        Console.Write("Input Y for another pivot point to create pivot axis:");
                        _newTransformation.pivotY2 = Convert.ToDouble(Console.ReadLine());
                        Console.WriteLine();
                        Console.Write("Input Z for another pivot point to create pivot axis:");
                        _newTransformation.pivotZ2 = Convert.ToDouble(Console.ReadLine());
                    }
                    if (_needTheta)
                    {
                        Console.WriteLine();
                        Console.Write("Input rotation amount (in degree):");
                        _newTransformation.theta = Convert.ToDouble(Console.ReadLine()) / 180 * Math.PI;
                    }
                    _transformations.Add(_newTransformation);
                    Console.WriteLine();
                } while (_isMultipleTransform);
                _model.Points = Transformation.Transform(_model.Points.ToList(), _transformations);
                Console.WriteLine("List of each vertex before and after transformation:");
                string _beforeTransform, _afterTransform;
                for (int i = 0; i < _model.Points.Count; i++)
                {
                    _beforeTransform = '(' + _preTransformedModel.Points[i].X.ToString() + ',' + _preTransformedModel.Points[i].Y.ToString() + ',' + _preTransformedModel.Points[i].Z.ToString() + ')';
                    _afterTransform = '(' + _model.Points[i].X.ToString() + ',' + _model.Points[i].Y.ToString() + ',' + _model.Points[i].Z.ToString() + ')';
                    Console.WriteLine($"{_beforeTransform} -> {_afterTransform}");
                }
                Console.WriteLine();
            }
            catch
            {
                Console.WriteLine();
                WriteError("Invalid input detected");
                Console.WriteLine();
            }
        }
    }
}
