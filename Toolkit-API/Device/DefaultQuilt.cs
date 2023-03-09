using System.Text.Json.Nodes;

namespace Toolkit_API.Device
{
    public struct DefaultQuilt
    {
        public float quiltAspect;
        public int quiltY;
        public int quiltX;
        public int tileX;
        public int tileY;

        private DefaultQuilt(JsonNode node)
        {
            if(node.AsArray().Count > 0)
            {
                quiltAspect = float.Parse(node.AsObject()["quiltAspect"]!.ToString());
                quiltY = int.Parse(node.AsObject()["quiltY"]!.ToString());
                quiltX = int.Parse(node.AsObject()["quiltX"]!.ToString());
                tileX = int.Parse(node.AsObject()["tileX"]!.ToString());
                tileY = int.Parse(node.AsObject()["tileY"]!.ToString());
            }
        }

        public static bool TryParse(string obj, out DefaultQuilt value)
        {
            if (obj == null || obj.Length == 0)
            {
                value = default;
                return false;
            }

            JsonNode? json = JsonNode.Parse(obj);
            if (json == null)
            {
                value = default;
                return false;
            }

            try
            {
                value = new DefaultQuilt(json);
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
