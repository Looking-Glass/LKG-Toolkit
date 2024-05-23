using System;

#if HAS_NEWTONSOFT_JSON
using Newtonsoft.Json.Linq;
#endif

namespace ToolkitAPI.Device {
    /// <summary>
    /// The recommended default quilt settings for a given LKG display.
    /// </summary>
    [Serializable]
    public struct QuiltSettings {
        public const int MinSize = 256;
        public const int MaxSize = 8192 * 2;
        public const int MinRowColumnCount = 1;
        public const int MaxRowColumnCount = 32;

        //REVIEW: [CRT-4039]
        /// <summary>
        /// Provides default quilt settings of 3360x3360 at 8x6 tiling, with a 0.75 aspect ratio (which conveniently matches the <see cref="TileAspect"/> of these settings).
        /// </summary>
        /// <remarks>
        /// Use this only if all else fails to load, to avoid having zeroed out <see cref="QuiltSettings"/>.
        /// </remarks>
        public static QuiltSettings Fallback => new QuiltSettings(3360, 3360, 8, 6, 0.75f);

        /// <summary>
        /// Provides settings for a blank 256x256 quilt at 1x1 tiling and 1 aspect ratio, to avoid issues with 0 quiltAspect and camera projetion matrices resulting with Infinity.
        /// </summary>
        public static QuiltSettings Blank => new QuiltSettings(MinSize, MinSize, 1, 1, 1);

        /// <summary>
        /// <para>
        /// The aspect ratio of the camera or source (2D/RGBD) image, when this quilt was originally rendered.<br />
        /// If you are rendering new quilts from a 3D scene, full-screen to a LKG display, this should be set to the aspect ratio of your LKG display's native screen resolution (<see cref="Calibration.screenW"/> / <see cref="Calibration.screenH"/>).
        /// </para>
        /// <para>This aspect ratio is NOT necessarily equal to the aspect ratio of each tile's (width / height).</para>
        /// </summary>
        public float quiltAspect;

        /// <summary>
        /// The total width of the quilt texture, in pixels.
        /// </summary>
        public int quiltX;

        /// <summary>
        /// The total height of the quilt texture, in pixels.
        /// </summary>
        public int quiltY;

        /// <summary>
        /// The number of quilt tiles counted along the x-axis (the number of columns).
        /// </summary>
        public int tileX;

        /// <summary>
        /// The number of quilt tiles counted along the y-axis (the number of rows).
        /// </summary>
        public int tileY;

        public bool IsDefaultOrBlank => Equals(default) || Equals(Blank);

        /// <summary>
        /// The total number of tiles in the quilt, which is given as <c>tileX * tileY</c>.
        /// </summary>
        public int TileCount => tileX * tileY;

        public int TileWidth {
            get {
                if (tileX <= 0)
                    return quiltX;
                return quiltX / tileX;
            }
        }

        public int TileHeight {
            get {
                if (tileY <= 0)
                    return quiltY;
                return quiltY / tileY;
            }
        }
        //NOTE: THIS IS DIFFERENT from what we use pretty much everywhere else.
        //This does NOT necessarily match the quiltAspect (of the native display resolution, if rendering fullscreen)
        public float TileAspect => (quiltX / tileX) / (quiltY / tileY);

        public int PaddingHorizontal => quiltX - tileX * TileWidth;
        public int PaddingVertical => quiltY - tileY * TileHeight;
        public float ViewPortionHorizontal => ((float) tileX * TileWidth) / quiltX;
        public float ViewPortionVertical => ((float) tileY * TileHeight) / quiltY;

        /// <summary>
        /// Creates new arbitrary quilt settings. Note that the <see cref="quiltAspect"/> is auto-calculated, and kept as a convenience.
        /// </summary>
        /// <param name="quiltX"></param>
        /// <param name="quiltY"></param>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        public QuiltSettings(
            int quiltX,
            int quiltY,
            int tileX,
            int tileY,
            float quiltAspect) : this() {

            this.quiltX = Math.Clamp(quiltX, MinSize, MaxSize);
            this.quiltY = Math.Clamp(quiltY, MinSize, MaxSize);
            this.tileX = Math.Clamp(tileX, MinRowColumnCount, MaxRowColumnCount);
            this.tileY = Math.Clamp(tileY, MinRowColumnCount, MaxRowColumnCount);
            this.quiltAspect = quiltAspect;
        }

#if HAS_NEWTONSOFT_JSON
        public static QuiltSettings Parse(JObject obj) {
            QuiltSettings result = new QuiltSettings();
            obj.TryGet<float>("quiltAspect", out result.quiltAspect);
            obj.TryGet<int>("quiltX", out result.quiltX);
            obj.TryGet<int>("quiltY", out result.quiltY);
            obj.TryGet<int>("tileX", out result.tileX);
            obj.TryGet<int>("tileY", out result.tileY);
            return result;
        }
#endif

        public bool Equals(QuiltSettings other) {
            if (quiltAspect == other.quiltAspect
                && quiltX == other.quiltX
                && quiltY == other.quiltY
                && tileX == other.tileX
                && tileY == other.tileY)
                return true;
            return false;
        }

        public static QuiltSettings GetDefaultFor(LKGDeviceType deviceType) {
            ILKGDeviceTemplateSystem system = ToolkitAPI.ServiceLocator.Instance.GetSystem<ILKGDeviceTemplateSystem>();
            if (system != null) {
                LKGDeviceTemplate template = system.GetTemplate(deviceType);
                if (template != null)
                    return template.defaultQuilt;
            }
            return QuiltSettings.Blank;
        }
    }
}
