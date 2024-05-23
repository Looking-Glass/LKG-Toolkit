using System.Collections.Generic;
using ToolkitAPI.Device;

namespace ToolkitAPI {
    public interface ILKGDeviceTemplateSystem {
        public LKGDeviceTemplate GetDefaultTemplate() => GetTemplate(LKGDeviceTypeExtensions.GetDefault());
        public LKGDeviceTemplate GetTemplate(LKGDeviceType deviceType);
        public IEnumerable<LKGDeviceTemplate> GetAllTemplates();
    }
}
