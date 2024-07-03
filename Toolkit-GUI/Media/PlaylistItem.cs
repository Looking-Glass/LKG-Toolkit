using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using System.IO;
using System.Drawing;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Controls;
using HarfBuzzSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel;
using System.Reflection;
using System.Security.AccessControl;
using Avalonia.Media;
using Image = Avalonia.Controls.Image;
using Size = Avalonia.Size;

namespace LookingGlass.Toolkit.GUI.Media
{
    [Serializable]
    public class PlaylistItem
    {
        [JsonInclude]
        public string path;
        [JsonInclude]
        public ResourceType rtype;
        [JsonInclude]
        public MediaType mtype;

        [JsonInclude]
        public int rows = 5;
        
        [JsonInclude]
        public int cols = 9;

        // this is a text float input box
        [JsonInclude]
        public float aspect = 1.77f;
        
        [JsonInclude]
        public int viewCount = 45;

        // this only affects non-video holograms
        [JsonInclude]
        public int durationMS = 20000;

        // this is a bool 
        [JsonInclude]
        public int isRGBD = 0;

        // combo box bottom = 0, top = 1, left = 2, right = 3
        [JsonInclude]
        public int depth_loc = 2;
        
        // this is a bool
        [JsonInclude]
        public int depth_inversion = 0;

        // this is also a bool
        [JsonInclude]
        public int chroma_depth = 0;

        // this ranges from -1 to 1
        [JsonInclude]
        public float crop_pos_x = 0;

        // this ranges from -1 to 1
        [JsonInclude]
        public float crop_pos_y = 0;

        // this ranges from 0 to 1
        [JsonInclude]
        public float depthiness = 0;

        // this ranges from 0 to 1
        [JsonInclude]
        public float depth_cutoff = 0;
 
        // this ranges from -1 to 1
        [JsonInclude]
        public float focus = 0;

        // this ranges from 0 to 2
        [JsonInclude]
        public float zoom = 1;

        // Preview Bitmap property
        [JsonIgnore]
        public Avalonia.Media.Imaging.Bitmap PreviewBitmap
        {
            get
            {
                if (_previewBitmap == null)
                {
                    LoadPreviewBitmap();
                }

                return _previewBitmap;
            }
        }
        private Avalonia.Media.Imaging.Bitmap _previewBitmap;

        public PlaylistItem()
        {
            path = "";
            rtype = ResourceType.Invalid;
            mtype = MediaType.Unknown;
        }

        public PlaylistItem(string path, ResourceType rtype, MediaType mtype)
        {
            this.path = path;
            this.rtype = rtype;
            this.mtype = mtype;
            if(mtype == MediaType.RGBDVideo || mtype == MediaType.RGBDImage)
            {
                isRGBD = 1;
            }
        }

        private void LoadPreviewBitmap()
        {
            // If it's a video, extract the first frame
            if (mtype == MediaType.QuiltVideo || mtype == MediaType.RGBDVideo)
            {
                // use FFMPEG to extract the first frame
                //using var video = new VideoFileReader(); // You might need a suitable video library
                //video.Open(path);
                //using var firstFrame = video.ReadVideoFrame();
                //_previewBitmap = ResizeImage(firstFrame, 256, 256);
                //video.Close();
            }
            // If it's an image, just load it
            else if (mtype == MediaType.QuiltImage || mtype == MediaType.RGBDImage)
            {
                using var image = new Avalonia.Media.Imaging.Bitmap(path);
                _previewBitmap = image;
            }
        }
    }
}
