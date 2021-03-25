using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Globalization;

namespace Render
{
    public class SceneData
    {
        public Point3D LookPosition { get; set; }
        public double CameraLightIntensity { get; set; }
        public double AmbientLightIntensity { get; set; }
        public Color ModelColorARGB { get; set; }

        public static SceneData Deserialize(string json)
        {
            var sceneData = JObject.Parse(json);

            // Parse look position
            var lookPosition = Point3D.Parse(sceneData["LookPosition"].ToString());

            // Parse camera light intensity
            var cameraLightIntensity = double.Parse(sceneData["CameraLightIntensity"].ToString());

            // Parse ambient light intensity
            var ambientLightIntensity = double.Parse(sceneData["AmbientLightIntensity"].ToString());

            // Parse model color
            var rawColor = sceneData["ModelColorARGB"].ToString();
            rawColor = rawColor.TrimStart('#');
            var modelColorARGB = Color.FromArgb(
                byte.Parse(rawColor.Substring(0, 2), NumberStyles.HexNumber),
                byte.Parse(rawColor.Substring(2, 2), NumberStyles.HexNumber),
                byte.Parse(rawColor.Substring(4, 2), NumberStyles.HexNumber),
                byte.Parse(rawColor.Substring(6, 2), NumberStyles.HexNumber));

            return new SceneData()
            {
                LookPosition = lookPosition,
                CameraLightIntensity = cameraLightIntensity,
                AmbientLightIntensity = ambientLightIntensity,
                ModelColorARGB = modelColorARGB
            };
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
