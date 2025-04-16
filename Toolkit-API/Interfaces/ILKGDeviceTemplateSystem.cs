using System.Collections.Generic;

namespace LookingGlass.Toolkit {
    /// <summary>
    /// Represents a system that can retrieve template data for all known/supported types of Looking Glass devices.
    /// </summary>
    public interface ILKGDeviceTemplateSystem {
        public LKGDeviceTemplate GetDefaultTemplate() => GetTemplate(LKGDeviceTypeExtensions.GetDefault());

        /// <summary>
        /// Attempts to retrieve the template for a given type of Looking Glass.
        /// </summary>
        /// <param name="deviceType">The type of device to search for.</param>
        /// <returns>The template associated with the given Looking Glass device type, or <c>null</c> when not found.</returns>
        public LKGDeviceTemplate GetTemplate(LKGDeviceType deviceType);

        /// <summary>
        /// Gets all known possible Looking Glass device templates, which contain default calibrations and quilt settings.
        /// </summary>
        public IEnumerable<LKGDeviceTemplate> GetAllTemplates();
    }
}
