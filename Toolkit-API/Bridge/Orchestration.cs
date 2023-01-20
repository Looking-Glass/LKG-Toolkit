using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

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
                JsonNode json = JsonNode.Parse(jsonString);
                string name = json["orchestration"]?["value"]?.ToString();
                string token = json["payload"]?["value"]?.ToString();
                orch = new Orchestration(name, token);
                return true;
            }
            catch (Exception ex)
            {
                orch = null;
                return false;
            }

        }

    }
}
