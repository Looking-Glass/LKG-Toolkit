using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using Newtonsoft.Json.Linq;
using ToolkitAPI.Bridge.EventListeners;
using ToolkitAPI.Bridge.Params;
using ToolkitAPI.Device;

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
            EndAsync();
            BeginAsync();

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

                HttpResponseMessage resp = client.SendAsync(request).Result;
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

        volatile bool stopAsyncThread = false;
        Thread asyncSendThread;
        ConcurrentQueue<AsyncMessageRecord> toSend;

        public class AsyncMessageRecord
        {
            public string endpoint;
            public string content;
            public Action<string> onCompletion;

            public AsyncMessageRecord(string endpoint, string content, Action<string> onCompletion)
            {
                this.endpoint = endpoint;
                this.content = content;
                this.onCompletion = onCompletion;
            }
        }

        public void BeginAsync()
        {
            toSend = new ConcurrentQueue<AsyncMessageRecord>();
            stopAsyncThread = false;

            asyncSendThread = new Thread(() =>
            {
                while (!stopAsyncThread)
                {
                    if(toSend.TryDequeue(out AsyncMessageRecord message))
                    {
                        try
                        {
                            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"http://{url}:{port}/{message.endpoint}");
                            request.Content = new StringContent(message.content);

                            HttpResponseMessage resp = client.SendAsync(request).Result;
                            string result = resp.Content.ReadAsStringAsync().Result;

                            UpdateConnectionState(true);
                            message.onCompletion(result);
                            continue;
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

                        message.onCompletion("");
                    }

                    Thread.Sleep(1);
                }
            });

            // this ensures the thread closes no matter what, even if we crash
            asyncSendThread.IsBackground = true;
            asyncSendThread.Start();
        }

        public void EndAsync()
        {
            stopAsyncThread = true;
            asyncSendThread?.Join();
        }

        public void TrySendMessageAsync(string endpoint, string content, Action<string> onCompletion)
        {
            if(toSend != null)
            {
                toSend.Enqueue(new AsyncMessageRecord(endpoint, content, onCompletion));
            }
            else
            {
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"http://{url}:{port}/{endpoint}");
                    request.Content = new StringContent(content);

                    HttpResponseMessage resp = client.SendAsync(request).Result;
                    string result = resp.Content.ReadAsStringAsync().Result;

                    UpdateConnectionState(true);
                    onCompletion(result);
                    return;
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

                onCompletion("");
            }
        }


        Stopwatch timer = new Stopwatch();
        private static void PrintTime(string name, TimeSpan time)
        {
            Console.WriteLine($"Endpoint: {name} completed in : {time.TotalMilliseconds}");
        }

        public Task<bool> TryEnterOrchestrationAsync(string name = "default")
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            string message =
                $@"
                {{
                    ""name"": ""{name}""
                }}
                ";

            TrySendMessageAsync("enter_orchestration", message, (resp) =>
            {
                if (resp != null)
                {
                    if (Orchestration.TryParse(resp, out Orchestration newSession))
                    {
                        session = newSession;
                        tcs.SetResult(true);
                        return;
                    }
                }

                tcs.SetResult(false);
            });

            return tcs.Task;
        }


        public bool TryEnterOrchestration(string name = "default")
        {
            timer.Restart();

            string message =
                $@"
                {{
                    ""name"": ""{name}""
                }}
                ";

            PrintTime("enter_orchestration sending messsage", timer.Elapsed);

            string resp = TrySendMessage("enter_orchestration", message);

            PrintTime("enter_orchestration message received", timer.Elapsed);


            if (resp != null)
            {
                if (Orchestration.TryParse(resp, out Orchestration newSession))
                {
                    session = newSession;
                    PrintTime("enter_orchestration message parsed", timer.Elapsed);
                    return true;
                }
            }

            PrintTime("enter_orchestration", timer.Elapsed);

            return false;
        }

        public bool TryExitOrchestration()
        {
            timer.Restart();

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
                PrintTime("exit_orchestration", timer.Elapsed);
                return true;
            }

            PrintTime("exit_orchestration", timer.Elapsed);
            return false;
        }

        public bool TryTransportControlsPlay()
        {
            timer.Restart();

            if (session == null)
                return false;

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}""
                }}
                ";

            string? resp = TrySendMessage("transport_control_play", message);

            PrintTime("transport_control_play", timer.Elapsed);

            return resp != null;
        }

        public bool TryTransportControlsPause()
        {
            timer.Restart();

            if (session == null)
                return false;

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}""
                }}
                ";

            string resp = TrySendMessage("transport_control_pause", message);

            PrintTime("transport_control_pause", timer.Elapsed);

            return resp != null;
        }

        public bool TryTransportControlsNext()
        {
            timer.Restart();

            if (session == null)
                return false;

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}""
                }}
                ";

            string resp = TrySendMessage("transport_control_next", message);

            PrintTime("transport_control_next", timer.Elapsed);

            return resp != null;
        }

        public bool TryTransportControlsPrevious()
        {
            timer.Restart();

            if (session == null)
                return false;

            string message =
            $@"
            {{
                ""orchestration"": ""{session.Token}""
            }}
            ";

            string resp = TrySendMessage("transport_control_previous", message);

            PrintTime("transport_control_previous", timer.Elapsed);

            return resp != null;
        }

        public bool TryShowWindow(bool showWindow, int head = -1)
        {
            timer.Restart();

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

            PrintTime("show_window", timer.Elapsed);

            return resp != null;
        }

        public bool TrySubscribeToEvents()
        {
            timer.Restart();

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

            PrintTime("TrySubscribeToEvents", timer.Elapsed);

            return webSocket.TrySendMessage(message);
        }


        public bool TryUpdatingParameter(string playlistName, int playlistItem, Parameters param, float newValue)
        {
            timer.Restart();

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

            PrintTime("update_playlist_entry", timer.Elapsed);

            return resp != null;
        }


        public bool TryUpdatingParameter(string playlistName, Parameters param, float newValue)
        {
            timer.Restart();

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

            PrintTime("update_current_entry", timer.Elapsed);

            return resp != null;
        }

        public bool TryUpdateDevices()
        {
            timer.Restart();

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
            
                PrintTime("available_output_devices", timer.Elapsed);
                return true;
            }

            PrintTime("available_output_devices", timer.Elapsed);
            return false;
        }

        private string currentPlaylistName = "";
        public bool TryDeletePlaylist(Playlist p)
        {
            timer.Restart();

            if (session == null)
                return false;

            if (currentPlaylistName == p.name)
                currentPlaylistName = "";

            string deleteMessage = p.GetInstanceJson(session);
            string response = TrySendMessage("delete_playlist", deleteMessage);

            PrintTime("delete_playlist", timer.Elapsed);

            return response != null;
        }

        public bool TrySyncPlaylist(int head = -1)
        {
            timer.Restart();

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

                PrintTime("sync_overwrite_playlist", timer.Elapsed);

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

        public void Dispose()
        {
            if (session != null)
                TryExitOrchestration();

            webSocket.Dispose();
            client.Dispose();

            EndAsync();
        }
    }
}
