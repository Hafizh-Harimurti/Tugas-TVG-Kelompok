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
        private static ModelData _originalModel = null;
        private static SceneData _scene = null;
        private static RendererServer _renderer;
        private static CancellationTokenSource _inspectionCancellation = null;

        async static Task Main(string[] args)
        {
            try
            {
                ConsoleHelper.Initialize();

                // Print identity.
                Console.WriteLine("Tugas Kelompok Teknik Visualisasi Grafis - Transformasi 3D");
                Console.WriteLine("Identitas Anggota Kelompok :");
                Console.WriteLine("Firdaus Bisma Suryakusuma        19/444051/TK/49247");
                Console.WriteLine("Lorem Ipsum                      XX/XXXXXX/XX/XXXXX");
                Console.WriteLine("Lorem Ipsum                      XX/XXXXXX/XX/XXXXX");
                Console.WriteLine("Please wait a while.");
                Console.WriteLine();
                await Task.Delay(3000);

                // Start up renderer
                _renderer = new RendererServer();

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
                            if (_inspectionCancellation != null) _inspectionCancellation.Cancel();
                            break;
                        case "3":
                            if (_inspectionCancellation != null) _inspectionCancellation.Cancel();
                            break;
                        case "4":
                            if (_inspectionCancellation != null) _inspectionCancellation.Cancel();
                            break;
                        case "5":
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
                            _model = _originalModel.Clone();
                            WriteSuccess("Model reset.");
                            break;
                        default:
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
            Console.WriteLine("The model files are saved in \'\'")
            Console.Write()
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
    }
}
