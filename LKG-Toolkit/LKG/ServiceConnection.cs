using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LKG_Toolkit.LKG
{
    internal static class ServiceConnection
    {
        static Action<string> ResponseCallback;
        static Action<string> AlertCallback;
        static Action<Display[]> PollCallback;
        static Action<bool> ConnectionStateCallback;

        static volatile bool runPollingThread;
        static volatile bool LastConnectionState;
        static Thread PollingThread;

        public static void setResponseCallback(Action<string> callback)
        {
            ResponseCallback = callback;
        }

        public static void setAlertCallback(Action<string> callback)
        {
            AlertCallback = callback;
        }

        public static void setPollCallback(Action<Display[]> callback)
        {
            PollCallback = callback;

            if(callback != null)
            {
                SetupPollingThread();
            }
        }

        public static void setConnectionStateCallback(Action<bool> callback)
        {
            ConnectionStateCallback = callback;

            if (callback != null)
            {
                SetupPollingThread();
            }
        }


        public static Display[] GetDisplays()
        {
            string resp = SendRequest("info", "");
            return Display.GetDisplaysFromJson(resp);
        }

        public static void ShowWindow(int window)
        {
            SendRequest("show",
            """
                    {
                        "targetDisplay" : 
            """
            + window +
            """
            ,
                    }
            """
            );
        }

        public static void ShowFile(int window, string path)
        {
            path = path.Replace("\\", "\\\\");

            SendRequest("show",
            """
                    {
                        "targetDisplay" : 
            """
                        + window +
            """
            ,
                        "source" : "
            """
                        + path +
            """
            ",
                    }
            """
            );
        }

        public static void HideWindow(int window)
        {
            SendRequest("hide",
            """
                    {
                        "targetDisplay" : 
            """
                        + window +
            """
            ,
                    }
            """
            );
        }

        private static bool TryInvoke(Action toInvoke)
        {
            try
            {
                Application.Current.Dispatcher.Dispatch(toInvoke);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        private static string SendRequest(string endpoint, string content)
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://localhost:33334/" + endpoint);
                request.Content = new StringContent(content);

                var resp = client.Send(request);
                string toReturn = resp.Content.ReadAsStringAsync().Result;

                TryInvoke(() => { ResponseCallback?.Invoke(toReturn); });
                LastConnectionState = true;

                return toReturn;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.ToString());
                if(LastConnectionState)
                {
                    TryInvoke(() => { AlertCallback?.Invoke("Unexpected Bridge Disconnection"); });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            TryInvoke(() => { ConnectionStateCallback?.Invoke(false); });
            LastConnectionState = false;

            return null;
        }

        private static void SetupPollingThread()
        {
            if(PollingThread != null)
            {
                runPollingThread = false;
                PollingThread.Join(100);
            }

            runPollingThread = true;

            PollingThread = new Thread(() =>
            {
                while(runPollingThread)
                {
                    string resp = SendRequest("info", "");
                    bool connected = resp != null;

                    if(connected)
                    {
                        Display[] displays = Display.GetDisplaysFromJson(resp);
                        TryInvoke(() => { PollCallback?.Invoke(displays); });
                    }

                    LastConnectionState = connected;
                    TryInvoke(() => { ConnectionStateCallback?.Invoke(connected); });

                    Thread.Sleep(2000);
                }
            });

            PollingThread.IsBackground = true;
            PollingThread.Start();
        }
    }
}
