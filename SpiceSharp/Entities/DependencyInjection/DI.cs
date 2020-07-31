using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// General helper methods for dependency injection containers that are used by
    /// entities to create behaviors.
    /// </summary>
    public static partial class DI
    {
        private class Comparer : IComparer<BehaviorDescription>
        {
            public int Compare(BehaviorDescription x, BehaviorDescription y)
            {
                // Note: we are ordering from high priority to low
                if (x.Priority > y.Priority)
                    return -1;
                if (x.Priority < y.Priority)
                    return 1;
                return 0;
            }
        }
        private class BehaviorDescription
        {
            public int Priority;
            public Type Implementation;
            public Type Target;
        }
        private readonly static object _lock = new object();
        private readonly static Dictionary<Type, List<BehaviorDescription>> _behaviorDescriptions = new Dictionary<Type, List<BehaviorDescription>>();
        private readonly static ConcurrentDictionary<Type, IBehaviorResolver> _behaviorResolvers = new ConcurrentDictionary<Type, IBehaviorResolver>();

        /// <summary>
        /// If <c>true</c>, then all loaded assemblies are searched for behaviors the first time that behaviors are
        /// created.
        /// </summary>
        public static bool ScanFirstTime { get; set; } = true;

        /// <summary>
        /// Scans all assemblies (thread-safe) if <see cref="ScanFirstTime"/> is <c>true</c>.
        /// This method will then set <see cref="ScanFirstTime"/> to <c>false</c>.
        /// </summary>
        private static void Scan()
        {
            // If we still need to scan the assemblies, do it now
            if (ScanFirstTime)
            {
                lock (_lock)
                {
                    if (ScanFirstTime)
                    {
                        ScanFirstTime = false;
                        _behaviorDescriptions.Clear();
                        ScanAssemblies();
                    }
                }
            }
        }

        /// <summary>
        /// Imports all behaviors of all assemblies that are loaded. This action is only executed once.
        /// </summary>
        public static void ScanAssemblies()
        {
            _behaviorDescriptions.Clear();
#if !NETSTANDARD1_5
            // Import the behaviors from all assemblies.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetCustomAttributes<BehavioralAttribute>().Any())
                    ScanAssembly(assembly);
            }
#else
            // Import the entry assembly, and let the method recursively find other assemblies
            var todo = new Stack<Assembly>();
            var done = new HashSet<string>();
            var assembly = Assembly.GetEntryAssembly() ?? typeof(DI).GetTypeInfo().Assembly;
            done.Add(assembly.FullName);
            todo.Push(assembly);
            while (todo.Count > 0)
            {
                assembly = todo.Pop();

                // Only search if the assembly is flagged as one
                if (assembly.GetCustomAttributes<BehavioralAttribute>().Any())
                    ScanAssembly(assembly);

                // Load referenced assemblies
                foreach (var assemblyName in assembly.GetReferencedAssemblies())
                {
                    // Don't search assemblies that have been scheduled/searched already
                    if (!done.Contains(assemblyName.FullName))
                    {
                        assembly = Assembly.Load(assemblyName);
                        if (assembly != null)
                            todo.Push(assembly);
                    }
                    done.Add(assemblyName.FullName);
                }
            }
#endif
        }

        /// <summary>
        /// Finds the behaviors in the specified assembly that are eligible.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public static void ScanAssembly(Assembly assembly)
        {
            // The check for discovering behaviors
            static bool CheckType(Type type)
            {
                var info = type.GetTypeInfo();
                if (info.IsAbstract || info.IsInterface)
                    return false;
                if (!info.GetCustomAttributes(typeof(BehaviorForAttribute)).Any())
                    return false;
                return true;
            };

            // Search for behaviors in the namespace
            foreach (var type in assembly.GetExportedTypes().Where(t => CheckType(t)))
            {
                foreach (var attribute in type.GetTypeInfo().GetCustomAttributes<BehaviorForAttribute>(false))
                {
                    if (!_behaviorDescriptions.TryGetValue(attribute.EntityType, out var types))
                    {
                        types = new List<BehaviorDescription>();
                        _behaviorDescriptions.Add(attribute.EntityType, types);
                    }
                    types.Add(new BehaviorDescription
                    {
                        Priority = attribute.Priority,
                        Target = attribute.Target,
                        Implementation = type
                    });
                }
            }
        }

        /// <summary>
        /// Creates the factories for the specified entity type.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        private static IBehaviorResolver CreateFactoriesFor(Type entityType)
        {
            // First find out which context is being used by the entity
            var etype = entityType.GetTypeInfo().GetInterfaces().First(t =>
            {
                var info = t.GetTypeInfo();
                if (!info.IsGenericType)
                    return false;
                if (info.GetGenericTypeDefinition() != typeof(IEntity<>))
                    return false;
                return true;
            });

            // Get the DI container and method for registering behaviors
            var di = typeof(Cache<>).MakeGenericType(etype.GetTypeInfo().GenericTypeArguments[0]);
            var resolver = (IBehaviorResolver)Activator.CreateInstance(di);

            // See if we can use use our discovered behavior descriptions
            if (_behaviorDescriptions.TryGetValue(entityType, out var behaviors))
            {
                behaviors.Sort(new Comparer());
                foreach (var behaviorType in behaviors)
                    resolver.RegisterAfter(behaviorType.Target, behaviorType.Implementation);
                return resolver;
            }
            return resolver;
        }

        /// <summary>
        /// Resolves behaviors for the specified simulation and entity.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="behaviors">The behaviors.</param>
        /// <param name="linkParameters">if set to <c>true</c> if parameters should be linked instead of cloned.</param>
        public static void Resolve(ISimulation simulation, IEntity entity, IBehaviorContainer behaviors, bool linkParameters)
        {
            Scan();
            var resolver = _behaviorResolvers.GetOrAdd(entity.GetType(), CreateFactoriesFor);
            resolver.Resolve(simulation, entity, behaviors, linkParameters);
        }

        /// <summary>
        /// Gets the behavior resolver for the specified entity and binding context types.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <returns>The <see cref="IBehaviorResolver{TContext}"/>.</returns>
        public static IBehaviorResolver<TContext> Get<TEntity, TContext>()
            where TEntity : IEntity<TContext>
            where TContext : IBindingContext
        {
            Scan();
            return (IBehaviorResolver<TContext>)_behaviorResolvers.GetOrAdd(typeof(TEntity), CreateFactoriesFor);
        }
    }
}
