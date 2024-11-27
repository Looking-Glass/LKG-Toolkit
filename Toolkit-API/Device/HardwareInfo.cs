using System;
using System.Collections.Generic;

#if HAS_NEWTONSOFT_JSON
using Newtonsoft.Json; // For JsonReaderException
using Newtonsoft.Json.Linq;
#endif

namespace LookingGlass.Toolkit {
    [Serializable]
    public struct HardwareInfo {
        public int index;
        public string hardwareVersion;
        public string hardwareVersionLong;
        public bool hasEdidCalibration;
        public float hfov;
        public float vfov;
        public float viewCone;
        public int resolutionWidth;
        public int resolutionHeight;
        public string defaultQuilt;
        public string calibration;

#if HAS_NEWTONSOFT_JSON
        public static List<HardwareInfo> ParseAll(string message) {
            List<HardwareInfo> hardwareInfoList = new();

            JObject messageObj;
            try {
                messageObj = JObject.Parse(message);
            }
            catch (JsonReaderException ex) {
                // Handle invalid JSON
                Console.WriteLine("Invalid JSON string: " + ex.Message);
                return hardwareInfoList;
            }

            // Navigate to the "payload" -> "value"
            if (messageObj.TryGetValue("payload", out JToken payloadToken) &&
                payloadToken is JObject payloadObj &&
                payloadObj.TryGetValue("value", out JToken payloadValueToken) &&
                payloadValueToken is JObject payloadValueObj) {

                // Iterate over each hardware item
                foreach (var item in payloadValueObj.Properties()) {
                    if (item.Value is JObject itemObj &&
                        itemObj.TryGetValue("value", out JToken itemValueToken) &&
                        itemValueToken is JObject itemValueObj) {

                        HardwareInfo hardwareInfo = new();

                        // Parse each property without extension method
                        hardwareInfo.index = GetValueFromProperty<int>(itemValueObj, "index");
                        hardwareInfo.hardwareVersion = GetValueFromProperty<string>(itemValueObj, "hardwareVersion");
                        hardwareInfo.hardwareVersionLong = GetValueFromProperty<string>(itemValueObj, "hardwareVersionLong");
                        hardwareInfo.hasEdidCalibration = GetValueFromProperty<bool>(itemValueObj, "hasEdidCalibration");
                        hardwareInfo.hfov = GetValueFromProperty<float>(itemValueObj, "hfov");
                        hardwareInfo.vfov = GetValueFromProperty<float>(itemValueObj, "vfov");
                        hardwareInfo.viewCone = GetValueFromProperty<float>(itemValueObj, "viewCone");
                        hardwareInfo.resolutionWidth = GetValueFromProperty<int>(itemValueObj, "resolutionWidth");
                        hardwareInfo.resolutionHeight = GetValueFromProperty<int>(itemValueObj, "resolutionHeight");
                        hardwareInfo.defaultQuilt = GetValueFromProperty<string>(itemValueObj, "defaultQuilt");
                        hardwareInfo.calibration = GetValueFromProperty<string>(itemValueObj, "calibration");

                        hardwareInfoList.Add(hardwareInfo);
                    }
                }
            }

            return hardwareInfoList;
        }

        // Helper method inside the struct
        private static T GetValueFromProperty<T>(JObject obj, string propertyName) {
            if (obj.TryGetValue(propertyName, out JToken token) &&
                token is JObject valueObj &&
                valueObj.TryGetValue("value", out JToken valueToken)) {

                return valueToken.ToObject<T>();
            }
            return default;
        }
#endif

        public override int GetHashCode() => index.GetHashCode();
        public override bool Equals(object obj) {
            if (obj == null || !(obj is HardwareInfo other))
                return false;
            return index == other.index &&
                   hardwareVersion == other.hardwareVersion &&
                   hardwareVersionLong == other.hardwareVersionLong &&
                   hasEdidCalibration == other.hasEdidCalibration &&
                   hfov == other.hfov &&
                   vfov == other.vfov &&
                   viewCone == other.viewCone &&
                   resolutionWidth == other.resolutionWidth &&
                   resolutionHeight == other.resolutionHeight &&
                   defaultQuilt == other.defaultQuilt &&
                   calibration == other.calibration;
        }
    }
}
