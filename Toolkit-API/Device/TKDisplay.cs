﻿using System;
using Newtonsoft.Json.Linq;

namespace ToolkitAPI.Device
{
    /// <summary>
    /// This represents a connected display, which may or may not be a LKG display or a regular 2D monitor.
    /// </summary>
    [Serializable]
    public class TKDisplay
    {
        public int id;

        /// <summary>
        /// <para>
        /// The LKG display-specific calibration values, required for accurate holographic rendering to the LKG display.<br />
        /// This will only be set properly if this display is a LKG display.
        /// </para>
        /// See also: <seealso cref="IsLKG"/>
        /// </summary>
        public Calibration calibration;

        /// <summary>
        /// <para>
        /// The default, recommended quilt settings for this LKG display.<br />
        /// This will only be set properly if this display is a LKG display.
        /// </para>
        /// See also: <seealso cref="IsLKG"/>
        /// </summary>
        public DefaultQuilt defaultQuilt;

        public TKDisplayInfo hardwareInfo;

        public bool IsLKG {
            get {
                return hardwareInfo.hwid != null && hardwareInfo.hwid.Contains("LKG")
                    && calibration.SeemsGood();
            }
        }

        public TKDisplay() 
        {
            id = -1;
            calibration = new Calibration();
            defaultQuilt = new DefaultQuilt();
            hardwareInfo = new TKDisplayInfo();
        }

        private TKDisplay(int id)
        {
            this.id = id;
        }

        private TKDisplay(int id, Calibration calibration, DefaultQuilt defautQuilt, TKDisplayInfo hardwareInfo)
        {
            this.id = id;
            this.calibration = calibration;
            this.defaultQuilt = defautQuilt;
            this.hardwareInfo = hardwareInfo;
        }

        public static bool TryParse(int id, JObject obj, out TKDisplay display)
        {
            try
            {
                display = new(id);

                if (Calibration.TryParse(obj["calibration"]?["value"].ToString(), out Calibration cal))
                    display.calibration = cal;

                if (DefaultQuilt.TryParse(obj["defaultQuilt"]?["value"].ToString(), out DefaultQuilt defaultQuilt))
                    display.defaultQuilt = defaultQuilt;

                if(TKDisplayInfo.TryParse(obj, out TKDisplayInfo info))
                    display.hardwareInfo = info;

                return true;
            }
            catch (Exception e) 
            {
                Console.WriteLine("Error parsing display json:\n" + e.ToString());
                display = null;
                return false;
            }
        }

        public string GetInfoString()
        {
            return
                "Display Type: " + hardwareInfo.hardwareVersion + "\n" +
                "Display Serial: " + hardwareInfo.hwid + "\n" +
                "Display Loc: [" + hardwareInfo.windowCoords[0] + ", " + hardwareInfo.windowCoords[1] + "]\n" +
                "Calibration Version: " + calibration.configVersion + "\n";
        }
    }
}
