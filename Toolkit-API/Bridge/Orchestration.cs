using System;
using Newtonsoft.Json.Linq;

namespace Toolkit_API.Bridge
{
    public class Orchestration
    {
        public string name { get; private set; }
        public string token { get; private set; }

        private Orchestration(string name, string token)
        {
            this.name = name;
            this.token = token;
        }

        public static bool TryParse(string jsonString, out Orchestration orch)
        {
            try
            {
                JObject json = JObject.Parse(jsonString);
                string name = json["orchestration"]?["value"]?.ToString();
                string token = json["payload"]?["value"]?.ToString();
                orch = new Orchestration(name, token);
                return true;
            }
            catch
            {
                orch = null;
                return false;
            }

        }

    }
}
