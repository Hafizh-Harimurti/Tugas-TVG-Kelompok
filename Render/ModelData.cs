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
    public class ModelData : RendererDataBase, ICloneable<ModelData>
    {
        public ModelData() { }

        public ModelData(ModelData modelData)
        {
            Points = modelData.Points;
            Triangles = modelData.Triangles;
        }

        public IList<Point3D> Points { get; set; }
        public int[] Triangles { get; set; }

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

        public ModelData Clone()
        {
            return new ModelData(this);
        }
    }
}
