using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace LKG_Toolkit.LKG
{
    public class Display
    {
        public CalibraionData calibraionData;
        public DefaultQuilt defautQuilt;
        public DisplayInfo hardwareInfo;

        private Display(CalibraionData calibraionData, DefaultQuilt defautQuilt, DisplayInfo hardwareInfo)
        {
            this.calibraionData = calibraionData;
            this.defautQuilt = defautQuilt;
            this.hardwareInfo = hardwareInfo;
        }

        public static Display[] GetDisplaysFromJson(string jsonString)
        {
            try
            {
                List<Display> displays = new List<Display>();
                JsonNode json = JsonNode.Parse(jsonString).AsObject()["devices"];

                foreach (JsonNode node in json.AsArray())
                {
                    CalibraionData calData = new CalibraionData(node.AsObject()["calibration"]);
                    DefaultQuilt defQ = JsonSerializer.Deserialize<DefaultQuilt>(node.AsObject()["defaultQuilt"].ToString());
                    DisplayInfo dispInfo = new DisplayInfo(node);

                    displays.Add(new Display(calData, defQ, dispInfo));
                }

                return displays.ToArray();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new Display[0];
            }

        }

        public string getInfoString()
        {
            return
                "Display Type: " + hardwareInfo.hardwareVersion + "\n" +
                "Display Serial: " + hardwareInfo.hwid + "\n" +
                "Display Loc: [" + hardwareInfo.windowCoords[0] + ", " + hardwareInfo.windowCoords[1] + "]\n" +
                "Calibration Version: " + calibraionData.configVersion + "\n";
        }
    }

    public class CalibraionData
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

        public CalibraionData(JsonNode node)
        {
            JsonObject obj = node.AsObject();
            DPI = (int)float.Parse(obj["DPI"]["value"].ToString());
            center = float.Parse(obj["center"]["value"].ToString());
            configVersion = obj["configVersion"].ToString();
            flipImageX = float.Parse(obj["flipImageX"]["value"].ToString());
            flipImageY = float.Parse(obj["flipImageY"]["value"].ToString());
            flipSubp = float.Parse(obj["flipSubp"]["value"].ToString());
            fringe = (int)float.Parse(obj["fringe"]["value"].ToString());
            invView = (int)float.Parse(obj["invView"]["value"].ToString());
            pitch = float.Parse(obj["pitch"]["value"].ToString());
            screenH = (int)float.Parse(obj["screenH"]["value"].ToString());
            screenW = (int)float.Parse(obj["screenW"]["value"].ToString());
            serial = obj["serial"].ToString();
            slope = float.Parse(obj["slope"]["value"].ToString());
            verticalAngle = float.Parse(obj["verticalAngle"]["value"].ToString());
            viewCone = (int)float.Parse(obj["viewCone"]["value"].ToString());
        }
    }

    public class DefaultQuilt
    {
        public float quiltAspect;
        public int quiltY;
        public int quiltX;
        public int tileX;
        public int tileY;
    }

    public struct DisplayInfo
    {
        public string hardwareVersion;
        public string hwid;
        public int index;
        public string state;
        public int[] windowCoords;

        public DisplayInfo(JsonNode node)
        {
            hardwareVersion = node.AsObject()["hardwareVersion"].ToString();
            hwid = node.AsObject()["hwid"].ToString();
            index = int.Parse(node.AsObject()["index"].ToString());
            state = node.AsObject()["state"].ToString();
            windowCoords = new int[2];
            windowCoords[0] = int.Parse(node.AsObject()["windowCoords"][0].ToString());
            windowCoords[1] = int.Parse(node.AsObject()["windowCoords"][1].ToString());
        }
    }
}
