using System.Text.Json.Nodes;
using Toolkit_API.Bridge.EventListeners;
using Toolkit_API.Device;

namespace Toolkit_API.Bridge
{
    public class BridgeConnectionHTTP : IDisposable
    {
        private int port;
        private int webSocketPort;
        private string url;

        private HttpClient client;
        private BridgeWebSocketClient webSocket;
        private volatile bool LastConnectionState = false;

        private Orchestration session;

        public Dictionary<int, Display> all_displays { get; private set; }
        public Dictionary<int, Display> LKG_Displays { get; private set; }


        private Dictionary<string, List<Action<string>>> eventListeners;

        private DisplayEvents monitorEvents;

        public BridgeConnectionHTTP(string url = "localhost", int port = 33334, int webSocketPort = 9724)
        {
            this.url = url;
            this.port = port;
            this.webSocketPort = webSocketPort;

            all_displays = new Dictionary<int, Display>();
            LKG_Displays = new Dictionary<int, Display>();

            eventListeners = new Dictionary<string, List<Action<string>>>();

            monitorEvents = new DisplayEvents(this);
        }

        public bool Connect()
        {
            client = new HttpClient();
            webSocket = new BridgeWebSocketClient(UpdateListeners);
            
            LastConnectionState = webSocket.TryConnect($"ws://{url}:{webSocketPort}/event_source");
            return LastConnectionState;
        }

        public int AddListener(string name, Action<string> callback)
        {
            if (eventListeners.ContainsKey(name))
            {
                int id = eventListeners[name].Count;
                eventListeners[name].Add(callback);
                return id;
            }
            else
            {
                List<Action<string>> callbacks = new List<Action<string>>();
                callbacks.Add(callback);

                eventListeners.Add(name, callbacks);
                return 0;
            }
        }

        public void RemoveListener(string name, Action<string> callback)
        {
            if (eventListeners.ContainsKey(name))
            {
                if (eventListeners[name].Contains(callback))
                {
                    eventListeners[name].Remove(callback);
                }
            }
        }

        private void UpdateListeners(string message)
        {
            JsonNode? json = JsonNode.Parse(message)["payload"]?["value"];

            if (json != null)
            {
                string eventName = json["event"]["value"].ToString();
                string eventData = json.ToString();

                if (eventListeners.ContainsKey(eventName))
                {
                    foreach (var listener in eventListeners[eventName])
                    {
                        listener(eventData);
                    }
                }

                // special case for listeners with empty names
                if(eventListeners.ContainsKey(""))
                {
                    foreach (var listener in eventListeners[""])
                    {
                        listener(eventData);
                    }
                }
            }
        }

        public List<Display> GetAllDisplays()
        {
            List<Display> displays = new List<Display>();

            foreach(var kvp in all_displays)
            {
                displays.Add(kvp.Value);
            }

            return displays;
        }

        public List<Display> GetLKGDisplays()
        {
            List<Display> displays = new List<Display>();

            foreach (var kvp in LKG_Displays)
            {
                displays.Add(kvp.Value);
            }

            return displays;
        }

        public string? TrySendMessage(string endpoint, string content)
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"http://{url}:{port}/{endpoint}");
                request.Content = new StringContent(content);

                var resp = client.Send(request);
                string toReturn = resp.Content.ReadAsStringAsync().Result;

                LastConnectionState = true;

                return toReturn;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            LastConnectionState = false;

            return null;
        }

        public bool TryEnterOrchestration(string name = "default")
        {
            string message =
                $$"""
                {
                    "name": "{{name}}"
                }
                """;

            string? resp = TrySendMessage("enter_orchestration", message);
            
            if(resp != null)
            {
                if(Orchestration.TryParse(resp, out Orchestration newSession))
                {
                    session = newSession;
                    return true;
                }
            }

            return false;
        }

        public bool TryExitOrchestration()
        {
            string message =
                $$"""
                {
                    "orchestration": "{{session.token}}"
                }
                """;

            string? resp = TrySendMessage("exit_orchestration", message);

            if (resp != null)
            {
                session = default;
                return true;
            }

            return false;
        }

        public bool TrySubscribeToEvents()
        {
            if(session == null)
            {
                return false;
            }

            if(!webSocket.Connected())
            {
                return false;
            }

            string message =
                $$"""
                {
                    "subscribe_orchestration_events": "{{session.token}}"
                }
                """;

            return webSocket.TrySendMessage(message);
        }

        public bool TryUpdateDevices()
        {
            string message =
                $$"""
                {
                    "orchestration": "{{session.token}}"
                }
                """;

            string? resp = TrySendMessage("available_output_devices", message);

            if (resp != null)
            {
                JsonObject node = JsonNode.Parse(resp)?["payload"]?["value"]?.AsObject();

                if(node != null)
                {
                    for (int i = 0; i < node.Count; i++)
                    {
                        Display? d = Display.ParseJson(i, node[i.ToString()]!["value"]!);
                        if (d != null && !all_displays.ContainsKey(d.hardwareInfo.index))
                        {
                            all_displays.Add(d.hardwareInfo.index, d);

                            if (d.hardwareInfo.hardwareVersion != "thirdparty" && !LKG_Displays.ContainsKey(d.hardwareInfo.index))
                            {
                                LKG_Displays.Add(d.hardwareInfo.index, d);
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public bool TryPlayPlaylist(Playlist p, int head)
        {
            string message = p.GetInstanceJson(session);
            string? resp = TrySendMessage("instance_playlist", message);

            //Console.WriteLine(resp);

            string[] playlistItems = p.GetPlaylistItemsAsJson(session);

            for(int i = 0; i < playlistItems.Length; i++)
            {
                string pMessage = playlistItems[i];
                string pResp = TrySendMessage("insert_playlist_entry", pMessage);

                //Console.WriteLine(pResp);
            }

            string playMessage = p.GetPlayPlaylistJson(session, head);
            string? playResp = TrySendMessage("play_playlist", playMessage);

            //Console.WriteLine(playResp);

            return true;
        }

        public void Dispose()
        {
            if (session != default)
            {
                TryExitOrchestration();
            }

            webSocket.Dispose();
            client.Dispose();
        }
    }
}