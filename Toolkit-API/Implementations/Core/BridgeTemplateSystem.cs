using System.Collections.Generic;
using LookingGlass.Toolkit.Bridge;

namespace LookingGlass.Toolkit {
    /// <summary>
    /// Retrieves all possible Looking Glass templates from Bridge via HTTP.
    /// </summary>
    public class BridgeTemplateSystem : ILKGDeviceTemplateSystem {
        private List<LKGDeviceInfo> allLKGHardwareInfos;
        private Dictionary<LKGDeviceType, LKGDeviceTemplate> deviceTemplates;

        private void LoadAllTemplates() {
            BridgeConnectionHTTP connection = ServiceLocator.Instance.GetSystem<BridgeConnectionHTTP>();
            allLKGHardwareInfos = connection.GetAllSupportedLKGHardware();

            deviceTemplates = new Dictionary<LKGDeviceType, LKGDeviceTemplate>();
            foreach (LKGDeviceInfo info in allLKGHardwareInfos) {
                LKGDeviceTemplate template = new(info);
                deviceTemplates.Add(info.calibration.GetDeviceType(), template);
            }
        }

        public IEnumerable<LKGDeviceTemplate> GetAllTemplates() {
            if (deviceTemplates == null)
                LoadAllTemplates();
            return deviceTemplates.Values;
        }

        public LKGDeviceTemplate GetTemplate(LKGDeviceType deviceType) {
            if (deviceTemplates == null)
                GetAllTemplates();
            if (deviceTemplates.TryGetValue(deviceType, out LKGDeviceTemplate template))
                return template;
            return null;
        }
    }
}
