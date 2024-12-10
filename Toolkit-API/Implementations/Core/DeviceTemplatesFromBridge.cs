using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LookingGlass.Toolkit.Bridge
{
    public partial class BridgeConnectionHTTP : ILKGDeviceTemplateSystem
    {
        public List<LKGDeviceInfo> AllLKGHardwareInfos;
        public List<LKGDeviceTemplate> DeviceTemplates;

        public IEnumerable<LKGDeviceTemplate> GetAllTemplates()
        {
            if (AllLKGHardwareInfos == null)
            {
                AllLKGHardwareInfos = GetAllLKGDisplays();
                DeviceTemplates = new List<LKGDeviceTemplate>();
                foreach (var LKGDeviceInfo in AllLKGHardwareInfos)
                {
                    DeviceTemplates.Add(new LKGDeviceTemplate(LKGDeviceInfo.calibration, LKGDeviceInfo.defaultQuilt));
                }
            }

            return DeviceTemplates;
        }

        public LKGDeviceTemplate GetTemplate(LKGDeviceType deviceType)
        {
            if(DeviceTemplates == null)
            {
                GetAllTemplates();
            }

            return GetTemplate(deviceType);
        }
    }
}
