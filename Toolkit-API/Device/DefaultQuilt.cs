using System;
using Newtonsoft.Json.Linq;

namespace Toolkit_API.Device
{
    public struct DefaultQuilt
    {
        public float quiltAspect;
        public int quiltY;
        public int quiltX;
        public int tileX;
        public int tileY;

        private DefaultQuilt(JObject obj)
        {
            //REVIEW: Why didn't this just take a JObject?
            //if(node.AsArray().Count > 0)
            //{
                quiltAspect = float.Parse(obj["quiltAspect"]!.ToString());
                quiltY = int.Parse(obj["quiltY"]!.ToString());
                quiltX = int.Parse(obj["quiltX"]!.ToString());
                tileX = int.Parse(obj["tileX"]!.ToString());
                tileY = int.Parse(obj["tileY"]!.ToString());
            //}
        }

        public static bool TryParse(string json, out DefaultQuilt value) =>
            JsonHelpers.TryParse(json, j => new DefaultQuilt(j), out value);
    }
}
