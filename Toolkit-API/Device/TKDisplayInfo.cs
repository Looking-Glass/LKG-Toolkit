using System;
using System.Linq;

#if HAS_NEWTONSOFT_JSON
using Newtonsoft.Json.Linq;
#endif

namespace ToolkitAPI.Device
{
    [Serializable]
    public struct TKDisplayInfo
    {
        public string hardwareVersion;

        /// <summary>
        /// The LKG device's hardware id.
        /// REVIEW: ???
        /// </summary>
        /// <remarks>In some contexts, this is also equivalent to the LKG device's "LKG name". Note that this is NOT the same as the LKG device's unique serial identifier.</remarks>
        public string hwid;

        /// <summary>
        /// The index of this display. This can be used to select a certain LKG display for certain operations.
        /// </summary>
        /// <remarks>
        /// This is also known as the head index in Looking Glass Bridge.
        /// </remarks>
        public int index;

        public string state;

        /// <summary>
        /// <para>
        /// Contains the xy screen coordinates of the top-left corner of the LKG display.
        /// This corresponds to the currently-running OS's display arrangement coordinates.
        /// </para>
        /// <para>
        /// See also:
        /// <list type="bullet">
        /// <item>On Windows: <seealso href="https://learn.microsoft.com/en-us/windows/win32/gdi/the-virtual-screen"/></item>
        /// </list>
        /// </para>
        /// </summary>
        public int[] windowCoords;

#if HAS_NEWTONSOFT_JSON
        private TKDisplayInfo(JObject obj)
        {
            hardwareVersion = obj["hardwareVersion"]!["value"]!.ToString();
            hwid = obj["hwid"]!["value"]!.ToString();
            index = int.Parse(obj["index"]!["value"]!.ToString());
            state = obj["state"]!["value"]!.ToString();
            windowCoords = new int[2];
            windowCoords[0] = int.Parse(obj["windowCoords"]!["value"]!["x"]!.ToString());
            windowCoords[1] = int.Parse(obj["windowCoords"]!["value"]!["y"]!.ToString());
        }

        public static bool TryParse(JObject obj, out TKDisplayInfo value)
        {
            if (obj == null)
            {
                value = default;
                return false;
            }

            try
            {
                value = new TKDisplayInfo(obj);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }
#endif

        public override int GetHashCode() => hwid?.GetHashCode() ?? 0;
        public override bool Equals(object obj) {
            if (obj == null || !(obj is TKDisplayInfo other))
                return false;
            return hardwareVersion == other.hardwareVersion &&
                hwid == other.hwid &&
                index == other.index &&
                state == other.state &&
                (((windowCoords == null) == (other.windowCoords == null)) || (windowCoords != null && windowCoords.SequenceEqual(other.windowCoords)));
        }
    }
}

