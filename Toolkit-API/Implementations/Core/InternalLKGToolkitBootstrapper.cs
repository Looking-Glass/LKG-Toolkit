using LookingGlass.Toolkit.Bridge;

namespace LookingGlass.Toolkit {
    /// <summary>
    /// Initializes the <see cref="ServiceLocator"/> with built-in LKG Toolkit systems.
    /// </summary>
    public class InternalLKGToolkitBootstrapper : ILKGToolkitBootstrapper {
        public InternalLKGToolkitBootstrapper() { }

        public int Order => 100;

        public void Bootstrap(ServiceLocator locator) {
            AddDefaultIfNeeded<ILogger, ConsoleLogger>(locator);
            AddDefaultIfNeeded<IHttpSender, DefaultHttpSender>(locator);
            AddDefaultIfNeeded<ILKGDeviceTemplateSystem, BridgeTemplateSystem>(locator);

            locator.AddSystem<BridgeConnectionHTTP>(new BridgeConnectionHTTP());
        }

        /// <summary>
        /// Checks if the locator already has a system of type <typeparamref name="InterfaceType"/>. If not, a default system of type <typeparamref name="DefaultType"/> is added for it.
        /// </summary>
        /// <typeparam name="InterfaceType">The interface type that you want to ensure exists on the service locator.</typeparam>
        /// <typeparam name="DefaultType">This is the concrete type the service locator will default to using if no system of type <typeparamref name="InterfaceType"/> is found.</typeparam>
        private void AddDefaultIfNeeded<InterfaceType, DefaultType>(ServiceLocator locator)
            where InterfaceType : class
            where DefaultType : class, InterfaceType, new() {

            InterfaceType system = locator.GetSystem<InterfaceType>();
            if (system == null)
                locator.AddSystem<DefaultType>(new DefaultType());
        }

    }
}
