using System;
using Newtonsoft.Json.Linq;

namespace Toolkit_API.Device
{
    public class Display
    {
        public int id;
        public Calibration calibration;
        public DefaultQuilt defautQuilt;
        public DisplayInfo hardwareInfo;

        public Display() 
        {
            id = -1;
            calibration = new Calibration();
            defautQuilt = new DefaultQuilt();
            hardwareInfo = new DisplayInfo();
        }

        private Display(int id)
        {
            this.id = id;
        }

        private Display(int id, Calibration calibration, DefaultQuilt defautQuilt, DisplayInfo hardwareInfo)
        {
            this.id = id;
            this.calibration = calibration;
            this.defautQuilt = defautQuilt;
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

        public static Display? ParseJson(int id, JObject obj)
        {
            try
            {
                Display disp = new Display();

                if (Calibration.TryParse(obj["calibration"]?["value"].ToString(), out Calibration c))
                {
                    disp.calibration = c;
                }

                if (DefaultQuilt.TryParse(obj["defaultQuilt"]?["value"].ToString(), out DefaultQuilt d))
                {
                    disp.defautQuilt = d;
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

        public string getInfoString()
        {
            return
                "Display Type: " + hardwareInfo.hardwareVersion + "\n" +
                "Display Serial: " + hardwareInfo.hwid + "\n" +
                "Display Loc: [" + hardwareInfo.windowCoords[0] + ", " + hardwareInfo.windowCoords[1] + "]\n" +
                "Calibration Version: " + calibration.configVersion + "\n";
        }
    }
}
