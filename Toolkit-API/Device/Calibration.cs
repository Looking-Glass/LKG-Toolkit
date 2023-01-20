using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Toolkit_API.Device
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

        private Calibration(JsonNode node)
        {
            JsonObject obj = node.AsObject();
            
            rawJson = obj.ToJsonString();

            configVersion = obj["configVersion"]!.ToString();
            serial = obj["serial"]!.ToString();

            DPI = (int)float.Parse(obj["DPI"]!["value"]!.ToString());
            center = float.Parse(obj["center"]!["value"]!.ToString());
            flipImageX = float.Parse(obj["flipImageX"]!["value"]!.ToString());
            flipImageY = float.Parse(obj["flipImageY"]!["value"]!.ToString());
            flipSubp = float.Parse(obj["flipSubp"]!["value"]!.ToString());
            fringe = (int)float.Parse(obj["fringe"]!["value"]!.ToString());
            invView = (int)float.Parse(obj["invView"]!["value"]!.ToString());
            pitch = float.Parse(obj["pitch"]!["value"]!.ToString());
            screenH = (int)float.Parse(obj["screenH"]!["value"]!.ToString());
            screenW = (int)float.Parse(obj["screenW"]!["value"]!.ToString());
            slope = float.Parse(obj["slope"]!["value"]!.ToString());
            verticalAngle = float.Parse(obj["verticalAngle"]!["value"]!.ToString());
            viewCone = (int)float.Parse(obj["viewCone"]!["value"]!.ToString());
        }

        public static bool TryParse(string obj, out Calibration value)
        {
            if (obj == null || obj.Length == 0)
            {
                value = default;
                return false;
            }

            JsonNode? json = JsonNode.Parse(obj);
            if(json == null)
            {
                value = default;
                return false;
            }

            try
            {
                value = new Calibration(json);
                return true;
            }
            catch (Exception ex)
            {
                value = default;
                return false;
            }
        }
    }
}
