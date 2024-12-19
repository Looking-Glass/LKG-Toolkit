using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LookingGlass.Toolkit {
    /// <summary>
    /// Represents a central collection of systems, for use in the entire program.
    /// </summary>
    /// <remarks>
    /// The <see cref="ServiceLocator"/> makes it easy to choose what systems to use for the duration of the program.<br />
    /// It supports abstraction through parent classes and interfaces, allowing swappable systems to support different C# environments (such as pure .NET, Unity/C#, etc.).
    /// </remarks>
    public class ServiceLocator : IDisposable {
        private static ServiceLocator instance;
        public static ServiceLocator Instance {
            get {
                if (instance == null) {
                    instance = new ServiceLocator();
                    instance.InvokeBootstrappers();
                }
                return instance;
            }
            internal set { instance = value; }
        }

        private List<object> systems = new();

        private void InvokeBootstrappers() {
            List<ILKGToolkitBootstrapper> bootstrappers = new();
            ILogger temporaryLogger = new ConsoleLogger(); //NOTE: This is just used to convey errors during initialization, since ILoggers get setup during initialization -- they aren't ready yet.

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Type type in assemblies.SelectMany(a => a.GetTypes())) {
                if (typeof(ILKGToolkitBootstrapper).IsAssignableFrom(type)) {
                    ConstructorInfo ctor = type.GetConstructor(new Type[0]);
                    if (ctor != null) {
                        ILKGToolkitBootstrapper b = (ILKGToolkitBootstrapper) ctor.Invoke(new object[0]);
                        if (b != null) {
                            bootstrappers.Add(b);
                        }
                    } else {
                        temporaryLogger.LogError("Found an " + nameof(ILKGToolkitBootstrapper) + " that doesn't have a default/empty constructor -- failed to use it during initialization! (Type: " + type.FullName + ")");
                    }
                }
            }

            bootstrappers.Sort((left, right) => left.Order - right.Order);
            foreach (ILKGToolkitBootstrapper b in bootstrappers) {
                try {
                    b.Bootstrap(this);
                } catch (Exception e) {
                    temporaryLogger.LogException(e);
                }
            }
        }

        public void Dispose() {
            foreach (object system in systems) {
                if (system is IDisposable disposable)
                    disposable.Dispose();
            }
            systems.Clear();
        }

        /// <summary>
        /// Attempts to add a system to the collection, if it's not already in the collection.
        /// </summary>
        /// <typeparam name="T">
        /// The type of system being added.<br />
        /// This generic type parameter is just used for type-safety, does not impact anything currently, and can be inferred by C#.
        /// </typeparam>
        /// <param name="system">The system that you want to add to the <see cref="ServiceLocator"/>.</param>
        /// <returns></returns>
        public bool AddSystem<T>(T system) where T : class {
            if (system == null)
                return false;
            if (systems.Contains(system))
                return false;
            systems.Add(system);
            return true;
        }

        /// <summary>
        /// Attempts to get the first system that matches the given type <typeparamref name="T"/> from the collection.
        /// </summary>
        /// <typeparam name="T">The type of system to search for (supports interface types and inheritance/parent class types).</typeparam>
        /// <returns>The first system that was found, or <c>null</c> otherwise.</returns>
        public T GetSystem<T>() where T : class {
            foreach (object system in systems) {
                if (system is T requestedType)
                    return requestedType;
            }
            return null;
        }
    }
}
