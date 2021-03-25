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
        private static ModelData _transformedModel = null;
        private static SceneData _scene = null;
        private static RendererServer _renderer;

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

                _scene = SceneData.Deserialize(File.ReadAllText($"./Scenes/scene1.txt"));
                _renderer.SetScene(_scene);
                _model = ModelData.Deserialize(File.ReadAllText($"./Models/cube-model.txt"));
                _renderer.Render(_model);

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
                    Console.WriteLine("[6] Reset model");
                    Console.WriteLine("[7] Exit");
                    Console.Write("Select : ");
                    var ans = Console.ReadLine();
                    Console.WriteLine();

                    if (ans == "7") break;

                    switch (ans)
                    {
                        case "0":
                            LoadModel();
                            break;
                        case "1":
                            LoadScene();
                            break;
                        case "2":
                            break;
                        case "3":
                            break;
                        case "4":
                            break;
                        case "5":
                            break;
                        case "6":
                            break;
                        default:
                            WriteError("Action not recognized.");
                            break;
                    }

                    Console.WriteLine();
                }

                double DegreesToRadians(double degrees) => degrees * (Math.PI / 180);

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();

                _renderer.Close();
            }
            catch (Exception e)
            {
                WriteError(e.Message);
            }
        }

        private static void LoadModel()
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
                _transformedModel = _model;
                _renderer.Render(_transformedModel);

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

        private static void LoadScene()
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
                _renderer.SetScene(_scene);

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
    }
}
