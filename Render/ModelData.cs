using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Render
{
    public class ModelData
    {
        public IList<Point3D> Points { get; set; }
        public int[] Triangles { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static ModelData Deserialize(string json)
        {
            var modelData = JObject.Parse(json);

            // Parse points.
            var points = new List<Point3D>();
            var rawPoints = modelData["Points"].Children().ToList();
            foreach (var rawPoint in rawPoints)
            {
                points.Add(Point3D.Parse(rawPoint.ToString()));
            }

            // Parse triangles.
            var triangles = new List<int>();
            var rawTriangles = modelData["Triangles"].Children().ToList();
            foreach (var rawTriangle in rawTriangles)
            {
                triangles.Add(int.Parse(rawTriangle.ToString()));
            }

            return new ModelData()
            {
                Points = points,
                Triangles = triangles.ToArray()
            };
        }
    }
}
