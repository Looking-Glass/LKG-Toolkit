using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolkitGUI.Media
{
    public class PlaylistManager
    {
        public static PlaylistManager Instance { get; private set; }
        public const string PlaylistsPath = "./playlists/";

        public List<string> unloadedPlaylists;
        public List<Playlist> loadedPlaylists;

        public PlaylistManager() 
        {
            Instance = this;
            unloadedPlaylists = new List<string>(); 
            loadedPlaylists = new List<Playlist>();
        }

        public void SearchForPlaylists()
        {
            // Create the PlaylistsPath directory if it doesn't exist
            if (!Directory.Exists(PlaylistsPath))
            {
                Directory.CreateDirectory(PlaylistsPath);
            }

            // Search for JSON files in the PlaylistsPath
            string[] jsonFiles = Directory.GetFiles(PlaylistsPath, "*.json");

            // Add the full paths of the JSON files to the unloadedPlaylists list
            foreach (string filePath in jsonFiles)
            {
                if(Playlist.TryParse(filePath, out Playlist playlist))
                {
                    loadedPlaylists.Add(playlist);
                    // save parsed playlist to ensure we get a valid playlist
                    SavePlaylist(playlist);
                }    
                else
                {
                    unloadedPlaylists.Add(filePath);
                }
            }
        }

        public void AddPlaylist(string name)
        {
            string filePath = Path.Combine(PlaylistsPath, $"{name}.json");
            Playlist newPlaylist = new Playlist(name, filePath);
            SavePlaylist(newPlaylist);
            loadedPlaylists.Add(newPlaylist);
        }

        public void SavePlaylist(Playlist playlist)
        {
            string json = Playlist.SerializePlaylist(playlist);
            File.WriteAllText(playlist.filePath, json);
        }

    }
}
