using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ToolkitGUI.Media
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

    }
}
