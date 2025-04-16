namespace LookingGlass.Toolkit {
    /// <summary>
    /// Implement this in a custom pure C# class to modify the initialization of the LKG Toolkit <see cref="ServiceLocator"/>.
    /// </summary>
    public interface ILKGToolkitBootstrapper {
        /// <summary>
        /// <para>
        /// The order in which the bootstrappers will be called.
        /// The built-in core systems are added at Order = 100, since they depend on user-configurable systems (<see cref="ILogger"/>, <see cref="IHttpSender"/>).
        /// </para>
        /// 
        /// <para>If you wish to modify the <see cref="ServiceLocator"/> before built-in bootstrapping occurs, return a value of 99 or less.</para>
        /// <para>If you wish to modify the <see cref="ServiceLocator"/> after built-in bootstrapping occurs, return a value of 101 or greater.</para>
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// The callback during initialization where you may make modifications during startup.
        /// </summary>
        /// <param name="locator">The collection of systems that is currently mid-way through being initialized.</param>
        public void Bootstrap(ServiceLocator locator);
    }
}
