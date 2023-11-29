using System;

#if HAS_NEWTONSOFT_JSON
using Newtonsoft.Json.Linq;
#endif

namespace ToolkitAPI.Device
{
    /// <summary>
    /// The recommended default quilt settings for a given LKG display.
    /// </summary>
    [Serializable]
    public struct DefaultQuilt
    {
        public float quiltAspect;
        public int quiltY;
        public int quiltX;
        public int tileX;
        public int tileY;

#if HAS_NEWTONSOFT_JSON
        private static DefaultQuilt ParseJson(JObject obj)
        {
            try
            {
                DefaultQuilt result = new DefaultQuilt();
                result.quiltAspect = obj["quiltAspect"].Value<float>();
                result.quiltY = obj["quiltY"].Value<int>();
                result.quiltX = obj["quiltX"].Value<int>();
                result.tileX = obj["tileX"].Value<int>();
                result.tileY = obj["tileY"].Value<int>();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing display json:\n" + e.ToString());
                return default;
            }
        }
#endif

        public static bool TryParse(string json, out DefaultQuilt value) {
#if !HAS_NEWTONSOFT_JSON
            value = default;
            return false;
#else
            return Utils.TryParse(json, j => ParseJson(j), out value);
#endif
        }
    }
}
