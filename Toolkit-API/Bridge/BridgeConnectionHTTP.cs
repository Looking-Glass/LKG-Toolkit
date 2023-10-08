﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using ToolkitAPI.Bridge.EventListeners;
using ToolkitAPI.Bridge.Params;
using ToolkitAPI.Device;

namespace ToolkitAPI.Bridge
{
    public class BridgeConnectionHTTP : IDisposable
    {
        public const string DefaultURL = "localhost";
        public const int DefaultPort = 33334;
        public const int DefaultWebSocketPort = 9724;

        private int port;
        private int webSocketPort;
        private string url;

        private ILogger logger;
        private IHttpSender httpSender;
        private BridgeWebSocketClient webSocket;
        private volatile bool lastConnectionState = false;

        private Stopwatch timer = new Stopwatch();

        private Orchestration session;
        private string currentPlaylistName = "";

        public bool LogTimes { get; set; }
        public Dictionary<int, TKDisplay> AllDisplays { get; private set; }
        public Dictionary<int, TKDisplay> LKGDisplays { get; private set; }

        private Dictionary<string, List<Action<string>>> eventListeners;
        private DisplayEvents monitorEvents;
        private HashSet<Action<bool>> connectionStateListeners;

        public BridgeConnectionHTTP(string url = DefaultURL, int port = DefaultPort, int webSocketPort = DefaultWebSocketPort) : this(null, null, url, port, webSocketPort) { }
        public BridgeConnectionHTTP(ILogger logger, IHttpSender httpSender, string url = DefaultURL, int port = DefaultPort, int webSocketPort = DefaultWebSocketPort)
        {
            this.url = url;
            this.port = port;
            this.webSocketPort = webSocketPort;

            //TODO: Use DI instead of putting defaults here:
            if (logger == null)
                logger = new ConsoleLogger();
            if (httpSender == null)
                httpSender = new DefaultHttpSender();
            this.logger = logger;
            this.httpSender = httpSender;
            this.httpSender.ExceptionHandler = HandleHttpException;

            AllDisplays = new Dictionary<int, TKDisplay>();
            LKGDisplays = new Dictionary<int, TKDisplay>();

            eventListeners = new Dictionary<string, List<Action<string>>>();
            monitorEvents = new DisplayEvents(this);
            connectionStateListeners = new HashSet<Action<bool>>();
        }

        public bool Connect(int timeoutSeconds = 300)
        {
            httpSender.TimeoutSeconds = timeoutSeconds;
            webSocket = new BridgeWebSocketClient(UpdateListeners);
            return UpdateConnectionState(webSocket.TryConnect($"ws://{url}:{webSocketPort}/event_source"));
        }


        public bool UpdateConnectionState(bool state)
        {
            lastConnectionState = state;

            foreach (Action<bool> callback in connectionStateListeners)
            {
                callback(lastConnectionState);
            }

            return lastConnectionState;
        }

        private string GetURL(string endpoint) => $"http://{url}:{port}/{endpoint}";
        private void HandleHttpException(Exception e) {
            this.logger.LogException(e);
            UpdateConnectionState(false);
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

                callback(lastConnectionState);
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
                string result = httpSender.Send(HttpSenderMethod.Put, GetURL(endpoint), content);
                UpdateConnectionState(true);
                return result;
            }
            catch (Exception e)
            {
                HandleHttpException(e);
                return null;
            }
        }

        public void TrySendMessageAsync(string endpoint, string content, Action<string> onCompletion)
        {
            try
            {
                httpSender.SendAsync(HttpSenderMethod.Put, GetURL(endpoint), content, (string response) => {
                    UpdateConnectionState(true);
                    onCompletion(response);
                });
            }
            catch (Exception e)
            {
                HandleHttpException(e);
            }
        }

        private void PrintTime(string name, TimeSpan time)
        {
            logger.Log($"Endpoint: {name} completed in : {time.TotalMilliseconds}ms");
        }

        public bool TryEnterOrchestration(string name = "default")
        {
            if (LogTimes)
                timer.Restart();

            string message =
                $@"
                {{
                    ""name"": ""{name}""
                }}
                ";

            if (LogTimes)
                PrintTime("enter_orchestration sending messsage", timer.Elapsed);

            string resp = TrySendMessage("enter_orchestration", message);

            if (LogTimes)
                PrintTime("enter_orchestration message received", timer.Elapsed);


            if (resp != null)
            {
                if (Orchestration.TryParse(resp, out Orchestration newSession))
                {
                    session = newSession;
                    if (LogTimes)
                        PrintTime("enter_orchestration message parsed", timer.Elapsed);
                    return true;
                }
            }

            if (LogTimes)
                PrintTime("enter_orchestration", timer.Elapsed);

            return false;
        }

        public bool TryExitOrchestration()
        {
            if (session == null)
                return false;

            if (LogTimes)
                timer.Restart();

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
                if (LogTimes)
                    PrintTime("exit_orchestration", timer.Elapsed);
                return true;
            }

            if (LogTimes)
                PrintTime("exit_orchestration", timer.Elapsed);
            return false;
        }

        public bool TryTransportControlsPlay()
        {
            if (session == null)
                return false;

            if (LogTimes)
                timer.Restart();

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}""
                }}
                ";

            string resp = TrySendMessage("transport_control_play", message);

            if (LogTimes)
                PrintTime("transport_control_play", timer.Elapsed);

            return resp != null;
        }

        public bool TryTransportControlsPause()
        {
            if (session == null)
                return false;

            if (LogTimes)
                timer.Restart();

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}""
                }}
                ";

            string resp = TrySendMessage("transport_control_pause", message);

            if (LogTimes)
                PrintTime("transport_control_pause", timer.Elapsed);

            return resp != null;
        }

        public bool TryTransportControlsNext()
        {
            if (session == null)
                return false;

            if (LogTimes)
                timer.Restart();

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}""
                }}
                ";

            string resp = TrySendMessage("transport_control_next", message);

            if (LogTimes)
                PrintTime("transport_control_next", timer.Elapsed);

            return resp != null;
        }

        public bool TryTransportControlsPrevious()
        {
            if (session == null)
                return false;

            if (LogTimes)
                timer.Restart();

            string message =
            $@"
            {{
                ""orchestration"": ""{session.Token}""
            }}
            ";

            string resp = TrySendMessage("transport_control_previous", message);

            if (LogTimes)
                PrintTime("transport_control_previous", timer.Elapsed);

            return resp != null;
        }

        public bool TryShowWindow(bool showWindow, int head = -1)
        {
            if (session == null)
                return false;

            if (LogTimes)
                timer.Restart();

            string message =
            $@"
            {{
                ""orchestration"": ""{session.Token}"",
                ""show_window"": ""{showWindow}"",
                ""head_index"": {head}
            }}
            ";

            string resp = TrySendMessage("show_window", message);

            if (LogTimes)
                PrintTime("show_window", timer.Elapsed);

            return resp != null;
        }

        public bool TrySubscribeToEvents()
        {
            if (session == null)
                return false;

            if (LogTimes)
                timer.Restart();

            if (!webSocket.Connected())
                return false;

            string message =
                $@"
                {{
                    ""subscribe_orchestration_events"": ""{session.Token}""
                }}
                ";

            if (LogTimes)
                PrintTime("TrySubscribeToEvents", timer.Elapsed);

            return webSocket.TrySendMessage(message);
        }


        public bool TryUpdatingParameter(string playlistName, int playlistItem, Parameters param, float newValue)
        {
            if (session == null)
                return false;

            if (LogTimes)
                timer.Restart();

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

            if (LogTimes)
                PrintTime("update_playlist_entry", timer.Elapsed);

            return resp != null;
        }


        public bool TryUpdatingParameter(string playlistName, Parameters param, float newValue)
        {
            if (session == null)
                return false;

            if (LogTimes)
                timer.Restart();

            string message =
                $@"
                {{
                    ""orchestration"": ""{session.Token}"",
                    ""name"": ""{playlistName}"",
                    ""{ParameterUtils.GetParamName(param)}"": ""{(ParameterUtils.IsFloatParam(param) ? newValue : (int) newValue)}"",
                }}
                ";

            string resp = TrySendMessage("update_current_entry", message);

            if (LogTimes)
                PrintTime("update_current_entry", timer.Elapsed);

            return resp != null;
        }

        public bool TryUpdateDevices()
        {
            if (session == null)
                return false;

            if (LogTimes)
                timer.Restart();

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
            
                if (LogTimes)
                    PrintTime("available_output_devices", timer.Elapsed);
                return true;
            }

            if (LogTimes)
                PrintTime("available_output_devices", timer.Elapsed);
            return false;
        }

        public bool TryDeletePlaylist(Playlist p)
        {
            if (session == null)
                return false;

            if (LogTimes)
                timer.Restart();

            if (currentPlaylistName == p.name)
                currentPlaylistName = "";

            string deleteMessage = p.GetInstanceJson(session);
            string response = TrySendMessage("delete_playlist", deleteMessage);

            if (LogTimes)
                PrintTime("delete_playlist", timer.Elapsed);

            return response != null;
        }

        public bool TrySyncPlaylist(int head = -1)
        {
            if (session == null)
                return false;

            if (LogTimes)
                timer.Restart();

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

                if (LogTimes)
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
            if (logger is IDisposable l)
                l.Dispose();
            if (httpSender is IDisposable h)
                h.Dispose();
        }
    }
}
