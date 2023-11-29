using System;

#if HAS_NEWTONSOFT_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

namespace ToolkitAPI.Device
{
    /// <summary>
    /// Contains data that is intrinsic to a specific LKG device. This data is used in rendering properly to the LKG display.
    /// </summary>
    [Serializable]
    public struct Calibration
    {
        /// <summary>
        /// The JSON text contained within the LKG device's visual.json (Calibration) file.
        /// </summary>
        public string rawJson;

        public string configVersion;

        /// <summary>
        /// The unique serial identifier for the particular LKG device.
        /// </summary>
        public string serial;

        public float pitch;
        public float slope;
        public float center;
        public int fringe;

        public int viewCone;
        public int invView;
        public float verticalAngle;

        /// <summary>
        /// The LKG display's dots per inch (DPI).
        /// </summary>
        public int DPI;

        /// <summary>
        /// The native screen width of the LKG display, in pixels.
        /// </summary>
        public int screenW;

        /// <summary>
        /// The native screen height of the LKG display, in pixels.
        /// </summary>
        public int screenH;

        /// <summary>
        /// Determines whether or not to flip the screen horizontally. A value of 1 causes the screen to flip horizontally. The default value is 0.
        /// </summary>
        public float flipImageX;

        /// <summary>
        /// Determines whether or not to flip the screen vertically. A value of 1 causes the screen to flip vertically. The default value is 0.
        /// </summary>
        public float flipImageY;

        public float flipSubp;

#if HAS_NEWTONSOFT_JSON
        private static Calibration Parse(JObject obj)
        {
            Calibration cal = new();
            cal.rawJson = obj.ToString(Formatting.Indented);

            cal.configVersion = obj["configVersion"]!.ToString();
            cal.serial = obj["serial"]!.ToString();

            cal.pitch = float.Parse(obj["pitch"]!["value"]!.ToString());
            cal.slope = float.Parse(obj["slope"]!["value"]!.ToString());
            cal.center = float.Parse(obj["center"]!["value"]!.ToString());
            try
            {
                cal.fringe = (int)float.Parse(obj["fringe"]!["value"]!.ToString());
            }
            catch
            {
                cal.fringe = 0;
            }

            cal.viewCone = (int)float.Parse(obj["viewCone"]!["value"]!.ToString());
            cal.invView = (int)float.Parse(obj["invView"]!["value"]!.ToString());
            cal.verticalAngle = float.Parse(obj["verticalAngle"]!["value"]!.ToString());
            cal.DPI = (int)float.Parse(obj["DPI"]!["value"]!.ToString());

            cal.screenW = (int)float.Parse(obj["screenW"]!["value"]!.ToString());
            cal.screenH = (int)float.Parse(obj["screenH"]!["value"]!.ToString());

            cal.flipImageX = float.Parse(obj["flipImageX"]!["value"]!.ToString());
            cal.flipImageY = float.Parse(obj["flipImageY"]!["value"]!.ToString());
            cal.flipSubp = float.Parse(obj["flipSubp"]!["value"]!.ToString());
            return cal;
        }
#endif

        public static bool TryParse(string json, out Calibration value) {
#if !HAS_NEWTONSOFT_JSON
            value = default;
            return false;
#else
            return Utils.TryParse(json, j => Parse(j), out value);
#endif
        }

        public bool SeemsGood() {
            if (screenW != 0 && screenH != 0)
                return true;
            return false;
        }
    }
}
