using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LKG_Toolkit.LKG
{
    internal static class ServiceConnection
    {
        static Action<string> ResponseCallback;
        static Action<string> AlertCallback;

        public static void setResponseCallback(Action<string> callback)
        {
            ResponseCallback = callback;
        }

        public static void setAlertCallback(Action<string> callback)
        {
            AlertCallback = callback;
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

                ResponseCallback?.Invoke(toReturn);
                return toReturn;
            }
            catch(HttpRequestException ex)
            {
                Console.WriteLine(ex.ToString());
                AlertCallback?.Invoke(ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
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
    }
}
