using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Toolkit_API;

namespace ToolkitGUI.Media
{
    [Serializable]
    public class Playlist
    {
        [JsonInclude]
        public string name;

        [JsonInclude]
        public string filePath;

        [JsonInclude]
        public List<PlaylistItem> items;

        private string dataPath = "";

        public Playlist() 
        {
            name = "";
            filePath = "";
            dataPath = "";
            items = new List<PlaylistItem>();
        }

        public Playlist(string name, string filePath)
        {
            this.name = name;
            this.filePath = filePath;
            dataPath = Path.Combine(Path.GetDirectoryName(this.filePath), name);
            Directory.CreateDirectory(dataPath);
            items = new List<PlaylistItem>();
        }

        public void AddFile(string filePath, bool isRGBD)
        {
            if (dataPath == "")
            {
                dataPath = Path.Combine(Path.GetDirectoryName(this.filePath), name);
                Directory.CreateDirectory(dataPath);
            }

            string fileName = Path.GetFileName(filePath);
            string destinationPath = Path.Combine(dataPath, fileName);

            // If the file already exists in the dataPath, delete it
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            // Copy the file to the dataPath
            File.Copy(filePath, destinationPath);

            // Add a new PlaylistItem to the items list based on the copied file's path

            MediaType type = Utils.FileUtils.FindMediaType(filePath, isRGBD);

            PlaylistItem newItem = new PlaylistItem(destinationPath, ResourceType.File, type);
            items.Add(newItem);
        }


        public static string SerializePlaylist(Playlist playlist)
        {
            // Add JsonSerializer options with indented formatting
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            return JsonSerializer.Serialize(playlist, options);
        }


        public static Playlist DeserializePlaylist(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Playlist>(json);
        }

        public static bool TryParse(string json, out Playlist playlist)
        {
            try
            {
                playlist = DeserializePlaylist(json);
                return true;
            }
            catch (JsonException e)
            {
                Trace.WriteLine(e);
                playlist = default;
                return false;
            }
        }

        public Toolkit_API.Bridge.Playlist GetBridgePlaylist()
        {
            var bridgePlaylist = new Toolkit_API.Bridge.Playlist(name, true);

            for(int i = 0; i < items.Count; i++)
            {
                PlaylistItem item = items[i];
                if(item.isRGBD == 1)
                {
                    bridgePlaylist.AddRGBDItem(Path.GetFullPath(item.path), item.rows, item.cols, item.aspect, 
                        item.depthiness, item.depth_cutoff, item.focus, item.depth_loc, 0, 0, 
                        item.zoom, new System.Numerics.Vector2(item.crop_pos_x, item.crop_pos_y), 
                        item.depth_inversion == 1, item.chroma_depth == 1);
                }
                else
                {
                    bridgePlaylist.AddQuiltItem(Path.GetFullPath(item.path), item.rows, item.cols, item.aspect, item.viewCount);
                }
            }

            return bridgePlaylist;
        }

        internal void UpdateItem(PlaylistItem item)
        {
            PlaylistManager.Instance.SavePlaylist(this);
        }
    }
}
