using System;
using Newtonsoft.Json.Linq;

namespace Toolkit_API.Device
{
    public struct DisplayInfo
    {
        public string hardwareVersion;
        public string hwid;
        public int index;
        public string state;
        public int[] windowCoords;

        private DisplayInfo(JObject obj)
        {
            hardwareVersion = obj["hardwareVersion"]!["value"]!.ToString();
            hwid = obj["hwid"]!["value"]!.ToString();
            index = int.Parse(obj["index"]!["value"]!.ToString());
            state = obj["state"]!["value"]!.ToString();
            windowCoords = new int[2];
            windowCoords[0] = int.Parse(obj["windowCoords"]!["value"]!["x"]!.ToString());
            windowCoords[1] = int.Parse(obj["windowCoords"]!["value"]!["y"]!.ToString());
        }

        public static bool TryParse(JObject obj, out DisplayInfo value)
        {
            if (obj == null)
            {
                value = default;
                return false;
            }

            try
            {
                value = new DisplayInfo(obj);
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

