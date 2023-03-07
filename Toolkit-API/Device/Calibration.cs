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
        public int DPI = 1;
        public float center = 1;
        public string configVersion = "";
        public float flipImageX = 1;
        public float flipImageY = 1;
        public float flipSubp = 1;
        public int fringe = 1;
        public int invView = 1 ;
        public float pitch = 1;
        public int screenH = 1;
        public int screenW = 1;
        public string serial = "";
        public float slope = 1;
        public float verticalAngle = 1;
        public int viewCone = 1;

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
