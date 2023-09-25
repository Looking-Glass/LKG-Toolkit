using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ToolkitAPI {
    internal static class JsonHelpers {
        public static bool TryParse<T>(string json, Func<JObject, T> callback, out T value)
        {
            value = default;
            if (string.IsNullOrEmpty(json))
                return false;

            JObject j = null;
            try {
                j = JObject.Parse(json);
            } catch (JsonReaderException) {
                return false;
            }
            if (j == null)
                return false;

            try
            {
                value = callback(j);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
