using System;

#if HAS_NEWTONSOFT_JSON
using Newtonsoft.Json.Linq;
#endif

namespace LookingGlass.Toolkit {
    /// <summary>
    /// Contains template hardware data about a given type of Looking Glass (LKG) device, including templates for the calibration and quilt settings, and other hardware info.
    /// </summary>
    [Serializable]
    public struct LKGDeviceInfo {
        public int index;
        public string hardwareVersion;
        public string hardwareVersionLong;
        public bool hasEdidCalibration;
        public float hfov;
        public float vfov;
        public float viewCone;
        public int resolutionWidth;
        public int resolutionHeight;
        public QuiltSettings defaultQuilt;
        public Calibration calibration;

#if HAS_NEWTONSOFT_JSON
        public static LKGDeviceInfo Parse(string json) {
            JObject j = JObject.Parse(json);
            return Parse(j);
        }

        public static LKGDeviceInfo Parse(JObject obj) {
            LKGDeviceInfo info = new();
            obj.TryGet<int>("index", "value", out info.index);
            obj.TryGet<string>("hardwareVersion", "value", out info.hardwareVersion);
            obj.TryGet<string>("hardwareVersionLong", "value", out info.hardwareVersionLong);
            obj.TryGet<bool>("hasEdidCalibration", "value", out info.hasEdidCalibration);
            obj.TryGet<float>("hfov", "value", out info.hfov);
            obj.TryGet<float>("vfov", "value", out info.vfov);
            obj.TryGet<float>("viewCone", "value", out info.viewCone);
            obj.TryGet<int>("resolutionWidth", "value", out info.resolutionWidth);
            obj.TryGet<int>("resolutionHeight", "value", out info.resolutionHeight);

            obj.TryGet<string>("defaultQuilt", "value", out string defaultQuiltString);
            if (!string.IsNullOrEmpty(defaultQuiltString))
                info.defaultQuilt = QuiltSettings.Parse(JObject.Parse(defaultQuiltString));

            obj.TryGet<string>("calibration", "value", out string calibrationString);
            if (!string.IsNullOrEmpty(calibrationString))
                info.calibration = Calibration.Parse(JObject.Parse(calibrationString));
            return info;
        }
#endif
    }
}
