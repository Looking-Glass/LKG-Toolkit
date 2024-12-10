using System;
using System.Collections.Generic;

#if HAS_NEWTONSOFT_JSON
using Newtonsoft.Json; // For JsonReaderException
using Newtonsoft.Json.Linq;
#endif

namespace LookingGlass.Toolkit
{
    [Serializable]
    public struct LKGDeviceInfo
    {
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
        public static List<LKGDeviceInfo> ParseAll(string message)
        {
            List<LKGDeviceInfo> hardwareInfoList = new();

            JObject messageObj;
            try
            {
                messageObj = JObject.Parse(message);
            }
            catch (JsonReaderException ex)
            {
                // Handle invalid JSON
                Console.WriteLine("Invalid JSON string: " + ex.Message);
                return hardwareInfoList;
            }

            // Navigate to the "payload" -> "value"
            if (messageObj.TryGetValue("payload", out JToken payloadToken) &&
                payloadToken is JObject payloadObj &&
                payloadObj.TryGetValue("value", out JToken payloadValueToken) &&
                payloadValueToken is JObject payloadValueObj)
            {

                // Iterate over each hardware item
                foreach (JProperty item in payloadValueObj.Properties())
                {
                    if (item.Value is JObject itemObj &&
                        itemObj.TryGetValue("value", out JToken itemValueToken) &&
                        itemValueToken is JObject itemValueObj)
                    {

                        LKGDeviceInfo hardwareInfo = new();

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
                        hardwareInfo.defaultQuilt = QuiltSettings.Parse(JObject.Parse(GetValueFromProperty<string>(itemValueObj, "defaultQuilt")));

                        string calibrationString = GetValueFromProperty<string>(itemValueObj, "calibration");
                        if (calibrationString != "")
                        {
                            hardwareInfo.calibration = Calibration.Parse(JObject.Parse(calibrationString));
                        }

                        hardwareInfoList.Add(hardwareInfo);
                    }
                }
            }

            return hardwareInfoList;
        }

        // Helper method inside the struct
        private static T GetValueFromProperty<T>(JObject obj, string propertyName)
        {
            if (obj.TryGetValue(propertyName, out JToken token) &&
                token is JObject valueObj &&
                valueObj.TryGetValue("value", out JToken valueToken))
            {

                return valueToken.ToObject<T>();
            }
            return default;
        }
#endif
    }
}
