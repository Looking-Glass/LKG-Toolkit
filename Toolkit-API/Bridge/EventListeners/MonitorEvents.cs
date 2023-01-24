using System.Text.Json.Nodes;

namespace Toolkit_API.Bridge.EventListeners
{
    public class DisplayEvents
    {
        public readonly string prefix = "Monitor ";

        private BridgeConnectionHTTP bridge;

        public DisplayEvents(BridgeConnectionHTTP bridge)
        {
            this.bridge = bridge;
            SetupListeners();
        }

        private void SetupListeners()
        {
            bridge.AddListener(prefix + "Disconnect", Disconnect);
            bridge.AddListener(prefix + "Connect", Connect);
        }

        public void Disconnect(string payload)
        {
            JsonNode? node = JsonNode.Parse(payload);

            if (node != null)
            {
                int head_index = int.Parse(node["head_index"]?["value"]?.ToString());

                if (bridge.all_displays.ContainsKey(head_index))
                {
                    bridge.all_displays.Remove(head_index);
                }

                if(bridge.LKG_Displays.ContainsKey(head_index))
                {
                    bridge.LKG_Displays.Remove(head_index);
                }
            }
        }

        public void Connect(string payload)
        {
            JsonNode? node = JsonNode.Parse(payload);

            if(node != null)
            {
                int head_index = int.Parse(node["head_index"]?["value"]?.ToString());

                if (!bridge.all_displays.ContainsKey(head_index))
                {
                    bridge.TryUpdateDevices();
                }
            }
        }

    }
}
