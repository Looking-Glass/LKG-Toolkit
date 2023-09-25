using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ToolkitAPI.Device
{
    public struct Calibration
    {
        public string rawJson;
        public int DPI;
        public float center;
        public string configVersion;
        public float flipImageX;
        public float flipImageY;
        public float flipSubp;
        public int fringe;
        public int invView;
        public float pitch;
        public int screenH;
        public int screenW;
        public string serial;
        public float slope;
        public float verticalAngle;
        public int viewCone;

        private Calibration(JObject obj)
        {
            rawJson = obj.ToString(Formatting.Indented);

            configVersion = obj["configVersion"]!.ToString();
            serial = obj["serial"]!.ToString();

            DPI = (int)float.Parse(obj["DPI"]!["value"]!.ToString());
            center = float.Parse(obj["center"]!["value"]!.ToString());
            flipImageX = float.Parse(obj["flipImageX"]!["value"]!.ToString());
            flipImageY = float.Parse(obj["flipImageY"]!["value"]!.ToString());
            flipSubp = float.Parse(obj["flipSubp"]!["value"]!.ToString());
            
            try
            {
                fringe = (int)float.Parse(obj["fringe"]!["value"]!.ToString());
            }
            catch
            {
                fringe = 0;
            }

            invView = (int)float.Parse(obj["invView"]!["value"]!.ToString());
            pitch = float.Parse(obj["pitch"]!["value"]!.ToString());
            screenH = (int)float.Parse(obj["screenH"]!["value"]!.ToString());
            screenW = (int)float.Parse(obj["screenW"]!["value"]!.ToString());
            slope = float.Parse(obj["slope"]!["value"]!.ToString());
            verticalAngle = float.Parse(obj["verticalAngle"]!["value"]!.ToString());
            viewCone = (int)float.Parse(obj["viewCone"]!["value"]!.ToString());
        }

        public static bool TryParse(string obj, out Calibration value) =>
            JsonHelpers.TryParse(obj, j => new Calibration(j), out value);
    }
}
