using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Toolkit_API.Bridge
{
    public class Playlist
    {
        public string name;
        public bool loop;
        private List<PlaylistItem> items;

        public Playlist(string name, bool loop = false)
        {
            this.name = name;
            this.loop = loop;
            items = new List<PlaylistItem>();
        }

        public void AddItem(string URI, int rows, int cols, float aspect, int viewCount)
        {
            int id = items.Count;
            PlaylistItem p = new PlaylistItem(id, URI, rows, cols, aspect, viewCount);
            items.Add(p);
        }

        public void RemoveItem(int id)
        {
            items.RemoveAt(id);

            for(int i = 0; i < items.Count; i++)
            {
                items[i].id = i;
            }
        }

        public string GetPlayPlaylistJson(Orchestration session, int head)
        {
            string content =
                $$"""
                {
                    "orchestration": "{{session.token}}",
                    "name": "{{name}}",
                    "head_index": "{{head}}"
                }
                """;

            return content;
        }

        public string GetInstanceJson(Orchestration session)
        {
            string content =
                $$"""
                {
                    "orchestration": "{{session.token}}",
                    "name": "{{name}}",
                    "loop": "{{(loop ? "true" : "false")}}"
                }
                """;

            return content;
        }

        public string[] GetPlaylistItemsAsJson(Orchestration session)
        {
            string[] strings = new string[items.Count];

            for(int i = 0; i < items.Count; i++)
            {
                strings[i] = GetPlaylistItemJson(session, i);
            }

            return strings;
        }

        private string GetPlaylistItemJson(Orchestration session, int id)
        {
            PlaylistItem item = items[id];

            string URI = item.URI;

            if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (new Uri(URI).IsFile)
                {
                    URI = URI.Replace("\\", "\\\\");
                }
            }

            string content =
                $$"""
                {
                    "orchestration": "{{session.token}}",
                    "name": "{{name}}",
                    "index": "{{id}}",
                    "uri": "{{URI}}",
                    "rows": "{{item.rows}}",
                    "cols": "{{item.cols}}",
                    "aspect": "{{item.aspect}}",
                    "view_count": "{{item.viewCount}}"
                }
                """;

            return content;
        }
    }

    public class PlaylistItem
    {
        public int id = -1;
        public string URI = "";
        public int rows = 1;
        public int cols = 1;
        public float aspect = 1;
        public int viewCount = 1;

        public PlaylistItem(int id, string URI, int rows, int cols, float aspect, int viewCount)
        {
            this.id = id;
            this.URI = URI;
            this.rows = rows;
            this.cols = cols;
            this.aspect = aspect;
            this.viewCount = viewCount;
        }
    }
}
