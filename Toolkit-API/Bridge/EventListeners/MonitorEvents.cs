#if HAS_NEWTONSOFT_JSON
using Newtonsoft.Json.Linq;
#endif

namespace LookingGlass.Toolkit.Bridge
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
#if HAS_NEWTONSOFT_JSON
            JObject root = JObject.Parse(payload);

            if (root != null)
            {
                int head_index = int.Parse(root["head_index"]?["value"]?.ToString());

                if (bridge.ConnectedDisplays.ContainsKey(head_index))
                {
                    bridge.ConnectedDisplays.Remove(head_index);
                }
            }
#endif
        }

        public void Connect(string payload)
        {
#if HAS_NEWTONSOFT_JSON
            JObject root = JObject.Parse(payload);

            if(root != null)
            {
                int head_index = int.Parse(root["head_index"]?["value"]?.ToString());

                if (!bridge.ConnectedDisplays.ContainsKey(head_index))
                {
                    bridge.TryUpdateConnectedDevices();
                }
            }
#endif
        }

    }
}
