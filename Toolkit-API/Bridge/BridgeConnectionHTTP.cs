using System.Text.Json.Nodes;
using Toolkit_API.Bridge.EventListeners;
using Toolkit_API.Bridge.Params;
using Toolkit_API.Device;
using WebSocketSharp;

namespace Toolkit_API.Bridge
{
    public class BridgeConnectionHTTP : IDisposable
    {
        private int port;
        private int webSocketPort;
        private string url;

        private HttpClient client = null;
        private BridgeWebSocketClient webSocket;
        private volatile bool LastConnectionState = false;

        private Orchestration session;

        public Dictionary<int, Display> all_displays { get; private set; }
        public Dictionary<int, Display> LKG_Displays { get; private set; }

        private Dictionary<string, List<Action<string>>> eventListeners;
        private DisplayEvents monitorEvents;

        private HashSet<Action<bool>> connectionStateListeners;

        public BridgeConnectionHTTP(string url = "localhost", int port = 33334, int webSocketPort = 9724)
        {
            this.url = url;
            this.port = port;
            this.webSocketPort = webSocketPort;

            all_displays = new Dictionary<int, Display>();
            LKG_Displays = new Dictionary<int, Display>();

            eventListeners = new Dictionary<string, List<Action<string>>>();

            monitorEvents = new DisplayEvents(this);

            connectionStateListeners = new HashSet<Action<bool>>();
        }

        public bool Connect(int timeoutSeconds = 300)
        {
            client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };
            webSocket = new BridgeWebSocketClient(UpdateListeners);
            return UpdateConnectionState(webSocket.TryConnect($"ws://{url}:{webSocketPort}/event_source"));
        }


        public bool UpdateConnectionState(bool state)
        {
            LastConnectionState = state;

            foreach (Action<bool> callback in connectionStateListeners)
            {
                callback(LastConnectionState);
            }

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

        public void AddConnectionStateListener(Action<bool> callback)
        {
            if (!connectionStateListeners.Contains(callback))
            {
                connectionStateListeners.Add(callback);

                callback(LastConnectionState);
            }
        }

        public void RemoveConnectionStateListener(Action<bool> callback)
        {
            if (connectionStateListeners.Contains(callback))
            {
                connectionStateListeners.Remove(callback);
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"http://{url}:{port}/{endpoint}");
                request.Content = new StringContent(content);

                var resp = client.Send(request);
                string toReturn = resp.Content.ReadAsStringAsync().Result;

                UpdateConnectionState(true);

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

            UpdateConnectionState(false);

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


        public bool TryUpdatingParameter(string playlistName, int playlistItem, Parameters param, float newValue)
        {
            string message =
                $$"""
                {
                    "orchestration": "{{session.token}}",
                    "name": "{{playlistName}}",
                    "index": "{{playlistItem}}",
                    "{{ParameterUtils.GetParamName(param)}}": "{{(ParameterUtils.IsFloatParam(param) ? newValue : (int)newValue)}}",
                }
                """;

            string? resp = TrySendMessage("update_playlist_entry", message);

            if (resp != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool TryUpdatingParameter(string playlistName, Parameters param, float newValue)
        {
            string message =
                $$"""
                {
                    "orchestration": "{{session.token}}",
                    "name": "{{playlistName}}",
                    "{{ParameterUtils.GetParamName(param)}}": "{{(ParameterUtils.IsFloatParam(param) ? newValue : (int) newValue)}}",
                }
                """;

            string? resp = TrySendMessage("update_current_entry", message);

            if (resp != null)
            {
                return true;
            }
            else
            {
                return false;
            }
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

                lock(this)
                {
                    if (node != null)
                    {
                        Dictionary<int, Display> all_displays = new Dictionary<int, Display>();
                        Dictionary<int, Display> LKG_Displays = new Dictionary<int, Display>();

                        for (int i = 0; i < node.Count; i++)
                        {
                            Display? d = Display.ParseJson(i, node[i.ToString()]!["value"]!);
                            if (d != null)
                            {
                                if (!all_displays.ContainsKey(d.hardwareInfo.index))
                                {
                                    all_displays.Add(d.hardwareInfo.index, d);
                                }

                                if (d.hardwareInfo.hwid.Contains("LKG") && !LKG_Displays.ContainsKey(d.hardwareInfo.index))
                                {
                                    if (!LKG_Displays.ContainsKey(d.hardwareInfo.index))
                                    {
                                        LKG_Displays.Add(d.hardwareInfo.index, d);
                                    }
                                }
                            }
                        }

                        this.all_displays = all_displays;
                        this.LKG_Displays = LKG_Displays;
                    }
                }

                return true;
            }

            return false;
        }


        private string current_playlist_name = string.Empty;
        public bool TryPlayPlaylist(Playlist p, int head)
        {
            if(current_playlist_name == p.name)
            {
                string delete_message = p.GetInstanceJson(session);
                string? delete_resp = TrySendMessage("delete_playlist", delete_message);
            }

            string message = p.GetInstanceJson(session);
            string? resp = TrySendMessage("instance_playlist", message);
            
            string[] playlistItems = p.GetPlaylistItemsAsJson(session);

            for(int i = 0; i < playlistItems.Length; i++)
            {
                string pMessage = playlistItems[i];
                string? pResp = TrySendMessage("insert_playlist_entry", pMessage);
            }

            current_playlist_name = p.name;

            string playMessage = p.GetPlayPlaylistJson(session, head);
            string? playResp = TrySendMessage("play_playlist", playMessage);

            return true;
        }

        public bool TrySaveout(string source, string filename)
        {
            string message =
                $$"""
                {
                    "orchestration": "{{session.token}}",
                    "head_index": "-1",
                    "source": "{{source}}",
                    "filename": "{{filename.Replace("\\", "\\\\")}}"
                }
                """;

            string? resp = TrySendMessage("source_saveout", message);

            if (!resp.IsNullOrEmpty())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryReadback(string source)
        {
            string message =
                $$"""
                {
                    "orchestration": "{{session.token}}",
                    "head_index": "-1",
                    "source": "{{source}}",
                }
                """;

            string? resp = TrySendMessage("source_readback", message);

            if (!resp.IsNullOrEmpty())
            {
                return true;
            }
            else
            {
                return false;
            }
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