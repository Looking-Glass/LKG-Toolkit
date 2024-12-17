using System.Collections.Generic;

namespace LookingGlass.Toolkit.Bridge {
    /// <summary>
    /// Retrieves all possible Looking Glass templates from Bridge via HTTP.
    /// </summary>
    public class BridgeTemplateSystem : ILKGDeviceTemplateSystem {
        private List<LKGDeviceInfo> allLKGHardwareInfos;
        private Dictionary<LKGDeviceType, LKGDeviceTemplate> deviceTemplates;

        private void LoadAllTemplates() {
            BridgeConnectionHTTP connection =
                //TODO: Something like this:
                //ServiceLocator.Instance.GetSystem<IBridgeConnectionHTTP>();
                //  For now, we'll use this because the UnityPlugin (and maybe other Toolkit-dependent codebase(s)?) may depend on instantiating BridgeConnectionHTTP themselves:
                BridgeConnectionHTTP.LastInstance;

            allLKGHardwareInfos = connection.GetAllLKGDisplays();

            deviceTemplates = new Dictionary<LKGDeviceType, LKGDeviceTemplate>();
            foreach (LKGDeviceInfo info in allLKGHardwareInfos) {
                LKGDeviceTemplate template = new(info.calibration, info.defaultQuilt);
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
