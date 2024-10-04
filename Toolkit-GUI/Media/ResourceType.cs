using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LookingGlass.Toolkit.GUI.Media
{
    public enum ResourceType
    {
        Invalid = -1,
        URL = 0,
        File = 1,
    }

    public enum MediaType
    {
        Unknown = -1,
        QuiltVideo = 0,
        QuiltImage = 1,
        RGBDVideo = 2,
        RGBDImage = 3,
    }
}