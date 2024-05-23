using System;

namespace ToolkitAPI.Device {
    /// <summary>
    /// Describes the layout of subpixel cells on a LKG display.
    /// </summary>
    [Serializable]
    public struct SubpixelCell {
        /// <summary>
        /// The size of one <see cref="SubpixelCell"/> struct, in bytes.
        /// </summary>
        public static int Stride => 3 * sizeof(float) * 2;

        public float ROffsetX;
        public float ROffsetY;
        public float GOffsetX;
        public float GOffsetY;
        public float BOffsetX;
        public float BOffsetY;
    }
}
