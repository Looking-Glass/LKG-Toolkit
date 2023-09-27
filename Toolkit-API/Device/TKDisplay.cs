﻿using System;
using Newtonsoft.Json.Linq;

namespace ToolkitAPI.Device
{
    public class TKDisplay
    {
        public int id;
        public Calibration calibration;
        public DefaultQuilt defaultQuilt;
        public DisplayInfo hardwareInfo;

        public TKDisplay() 
        {
            id = -1;
            calibration = new Calibration();
            defaultQuilt = new DefaultQuilt();
            hardwareInfo = new DisplayInfo();
        }

        private TKDisplay(int id)
        {
            this.id = id;
        }

        private TKDisplay(int id, Calibration calibration, DefaultQuilt defautQuilt, DisplayInfo hardwareInfo)
        {
            this.id = id;
            this.calibration = calibration;
            this.defaultQuilt = defautQuilt;
            this.hardwareInfo = hardwareInfo;
        }

        public bool SeemsGood()
        {
            if(calibration.screenW != 0 && calibration.screenH != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static TKDisplay? ParseJson(int id, JObject obj)
        {
            try
            {
                TKDisplay disp = new TKDisplay();

                if (Calibration.TryParse(obj["calibration"]?["value"].ToString(), out Calibration c))
                {
                    disp.calibration = c;
                }

                if (DefaultQuilt.TryParse(obj["defaultQuilt"]?["value"].ToString(), out DefaultQuilt d))
                {
                    disp.defaultQuilt = d;
                }

                if(DisplayInfo.TryParse(obj, out DisplayInfo i))
                {
                    disp.hardwareInfo = i;
                }

                return disp;
            }
            catch (Exception e) 
            {
                Console.WriteLine("Error parsing display json:\n" + e.ToString());
                return null;
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
