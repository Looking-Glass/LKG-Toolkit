using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolkitGUI.Media;

namespace ToolkitGUI.Utils
{
    public static class FileUtils
    {
        static string[] videoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".wmv" };
        static string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };

        public static MediaType FindMediaType(string path, bool isRGBD)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();

            bool isVideo = videoExtensions.Contains(extension);
            bool isImage = imageExtensions.Contains(extension);

            if (!isVideo && !isImage)
            {
                return MediaType.Unknown;
            }

            if (isRGBD)
            {
                return isVideo ? MediaType.RGBDVideo : MediaType.RGBDImage;
            }
            else
            {
                return isVideo ? MediaType.QuiltVideo : MediaType.QuiltImage;
            }
        }
    }
}
