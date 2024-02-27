using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using ToolkitAPI.Bridge.EventListeners;
using ToolkitAPI.Bridge.Params;
using ToolkitAPI.Device;
using WebSocketSharp;

namespace ToolkitAPI.Bridge
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

        public Dictionary<int, TKDisplay> AllDisplays { get; private set; }
        public Dictionary<int, TKDisplay> LKGDisplays { get; private set; }

        private Dictionary<string, List<Action<string>>> eventListeners;
        private DisplayEvents monitorEvents;

        private HashSet<Action<bool>> connectionStateListeners;

        public BridgeConnectionHTTP(string url = "localhost", int port = 33334, int webSocketPort = 9724)
        {
            this.url = url;
            this.port = port;
            this.webSocketPort = webSocketPort;

            AllDisplays = new Dictionary<int, TKDisplay>();
            LKGDisplays = new Dictionary<int, TKDisplay>();

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
            JToken? json = JObject.Parse(message)["payload"]?["value"];

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

        public List<TKDisplay> GetAllDisplays()
        {
            List<TKDisplay> displays = new List<TKDisplay>();

            foreach(var kvp in AllDisplays)
            {
                displays.Add(kvp.Value);
            }

            return displays;
        }

        public List<TKDisplay> GetLKGDisplays()
        {
            List<TKDisplay> displays = new List<TKDisplay>();

            foreach (var kvp in LKGDisplays)
            {
                displays.Add(kvp.Value);
            }

            return displays;
        }

        public string TrySendMessage(string endpoint, string content)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"http://{url}:{port}/{endpoint}");
                request.Content = new StringContent(content);

                HttpResponseMessage resp = client.SendAsync(request).Result; //TODO: Async support?
                string result = resp.Content.ReadAsStringAsync().Result;

                UpdateConnectionState(true);

                return result;
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
                $@"
                {{
                    ""name"": ""{name}""
                }}
                ";

            string resp = TrySendMessage("enter_orchestration", message);
            if (resp != null)
            {
                if (Orchestration.TryParse(resp, out Orchestration newSession))
                {
                    session = newSession;
                    return true;
                }
            }
            return false;
        }

        public bool TryExitOrchestration()
        {
            if (session == null)
                return false;

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}""
                }}
                ";

            string resp = TrySendMessage("exit_orchestration", message);

            if (resp != null)
            {
                session = default;
                return true;
            }
            return false;
        }

        public bool TryTransportControlsPlay()
        {
            if (session == null)
                return false;

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}""
                }}
                ";

            string? resp = TrySendMessage("transport_control_play", message);
            return resp != null;
        }

        public bool TryTransportControlsPause()
        {
            if (session == null)
                return false;

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}""
                }}
                ";

            string resp = TrySendMessage("transport_control_pause", message);
            return resp != null;
        }

        public bool TryTransportControlsNext()
        {
            if (session == null)
                return false;

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}""
                }}
                ";

            string resp = TrySendMessage("transport_control_next", message);
            return resp != null;
        }

        public bool TryTransportControlsPrevious()
        {
            if (session == null)
                return false;

            string message =
            $@"
            {{
                ""orchestration"": ""{session.Token}""
            }}
            ";

            string resp = TrySendMessage("transport_control_previous", message);
            return resp != null;
        }

        public bool TryShowWindow(bool showWindow, int head = -1)
        {
            if (session == null)
                return false;

            string message =
            $@"
            {{
                ""orchestration"": ""{session.Token}"",
                ""show_window"": ""{showWindow}"",
                ""head_index"": {head}
            }}
            ";

            string resp = TrySendMessage("show_window", message);
            return resp != null;
        }

        public bool TrySubscribeToEvents()
        {
            if (session == null)
                return false;

            if (!webSocket.Connected())
                return false;

            string message =
                $@"
                {{
                    ""subscribe_orchestration_events"": ""{session.Token}""
                }}
                ";

            return webSocket.TrySendMessage(message);
        }


        public bool TryUpdatingParameter(string playlistName, int playlistItem, Parameters param, float newValue)
        {
            if (session == null)
                return false;

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}"",
                    ""name"": ""{playlistName}"",
                    ""index"": ""{playlistItem}"",
                    ""{ParameterUtils.GetParamName(param)}"": ""{(ParameterUtils.IsFloatParam(param) ? newValue : (int)newValue)}"",
                }}
                ";

            string resp = TrySendMessage("update_playlist_entry", message);
            return resp != null;
        }


        public bool TryUpdatingParameter(string playlistName, Parameters param, float newValue)
        {
            if (session == null)
            {
                return false;
            }

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}"",
                    ""name"": ""{playlistName}"",
                    ""{ParameterUtils.GetParamName(param)}"": ""{(ParameterUtils.IsFloatParam(param) ? newValue : (int) newValue)}"",
                }}
                ";

            string resp = TrySendMessage("update_current_entry", message);
            return resp != null;
        }

        public bool TryUpdateDevices()
        {
            if (session == null)
                return false;

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}""
                }}
                ";

            string resp = TrySendMessage("available_output_devices", message);

            if (resp != null)
            {
                JObject payloadJson = JObject.Parse(resp)?["payload"]?["value"]?.Value<JObject>();

                lock(this)
                {
                    if (payloadJson != null)
                    {
                        Dictionary<int, TKDisplay> allDisplays = new Dictionary<int, TKDisplay>();
                        Dictionary<int, TKDisplay> lkgDisplays = new Dictionary<int, TKDisplay>();

                        for (int i = 0; i < payloadJson.Count; i++)
                        {
                            JObject displayJson = payloadJson[i.ToString()]!["value"]!.Value<JObject>();
                            if (TKDisplay.TryParse(i, displayJson, out TKDisplay display))
                            {
                                if (!allDisplays.ContainsKey(display.hardwareInfo.index))
                                    allDisplays.Add(display.hardwareInfo.index, display);

                                if (display.IsLKG && !lkgDisplays.ContainsKey(display.hardwareInfo.index))
                                    lkgDisplays.Add(display.hardwareInfo.index, display);
                            }
                        }

                        AllDisplays = allDisplays;
                        LKGDisplays = lkgDisplays;
                    }
                }

                return true;
            }

            return false;
        }

        private string currentPlaylistName = "";
        public bool TryDeletePlaylist(Playlist p)
        {
            if (session == null)
                return false;

            if (currentPlaylistName == p.name)
                currentPlaylistName = "";

            string deleteMessage = p.GetInstanceJson(session);
            string response = TrySendMessage("delete_playlist", deleteMessage);

            return response != null;
        }

        public bool TrySyncPlaylist(int head = -1)
        {
            if (session == null)
                return false;

            if (currentPlaylistName != "")
            {
                string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}"",
                    ""name"": ""{currentPlaylistName}"",
                    ""head_index"": {head},
                    ""crf"": 20,
                    ""pixel_format"": ""yuv420p"",
                    ""encoder"": ""h265""
                }}
                ";

                string response = TrySendMessage("sync_overwrite_playlist", message);
                return response != null;
            }
            return false;
        }

        /// <summary>
        /// Attempts to show media (image or video) on a LKG display.
        /// </summary>
        /// <param name="p">The playlist to play. This may contain one or more images or videos to playback on the LKG display.</param>
        /// <param name="head">
        /// <para>
        /// Determines which LKG display to target. This is the LKG display index from <see cref="TKDisplayInfo.index"/>.
        /// </para>
        /// <remarks>
        /// Note that using a display index of -1 will use the first available LKG display.
        /// </remarks>
        /// </param>
        /// <returns></returns>
        public bool TryPlayPlaylist(Playlist p, int head = -1)
        {
            if (session ==  null)
                return false;

            if (currentPlaylistName == p.name)
            {
                string delete_message = p.GetInstanceJson(session);
                string delete_resp = TrySendMessage("delete_playlist", delete_message);
            }

            TryShowWindow(true, head);

            string message = p.GetInstanceJson(session);
            string resp = TrySendMessage("instance_playlist", message);
            
            string[] playlistItems = p.GetPlaylistItemsAsJson(session);

            for (int i = 0; i < playlistItems.Length; i++)
            {
                string pMessage = playlistItems[i];
                string pResp = TrySendMessage("insert_playlist_entry", pMessage);
            }

            currentPlaylistName = p.name;

            string playMessage = p.GetPlayPlaylistJson(session, head);
            string playResp = TrySendMessage("play_playlist", playMessage);

            return true;
        }

        public bool TrySaveout(string source, string filename)
        {
            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}"",
                    ""head_index"": ""-1"",
                    ""source"": ""{source}"",
                    ""filename"": ""{filename.Replace("\\", "\\\\")}""
                }}
                ";

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
                $@"
                {{
                    ""orchestration"": ""{session.Token}"",
                    ""head_index"": ""-1"",
                    ""source"": ""{source}"",
                }}
                ";

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
