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
        private readonly static Dictionary<Type, List<BehaviorDescription>> _behaviorDescriptions = new Dictionary<Type, List<BehaviorDescription>>();
        private readonly static object _lock = new object();
        private readonly static ConcurrentDictionary<Type, IBehaviorResolver> _behaviorResolvers = new ConcurrentDictionary<Type, IBehaviorResolver>();

        /// <summary>
        /// If <c>true</c>, then all loaded assemblies are searched for behaviors the first time that behaviors are
        /// created.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the assembly is scanned for behaviors if an entity doesn't have any behaviors; otherwise, <c>false</c>.
        /// </value>
        public static bool ScanIfNotFound { get; set; } = true;

        /// <summary>
        /// Registers all behaviors in the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assembly"/> is <c>null</c>.</exception>
        public static void RegisterAllBehaviors(Assembly assembly)
        {
            assembly.ThrowIfNull(nameof(assembly));
            lock(_lock)
            {
                ScanAssembly(assembly);
            }
        }

        /// <summary>
        /// Registers a behavior for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <typeparam name="TBehavior">The type of the behavior.</typeparam>
        /// <param name="priority">The priority of the behavior.</param>
        public static void RegisterBehaviorFor<TEntity, TTarget, TBehavior>(int priority)
            where TEntity : IEntity
            where TTarget : IBehavior
            where TBehavior : TTarget
        {
            lock (_lock)
            {
                if (!_behaviorDescriptions.TryGetValue(typeof(TEntity), out var types))
                {
                    types = new List<BehaviorDescription>();
                    _behaviorDescriptions.Add(typeof(TEntity), types);
                }
                types.Add(new BehaviorDescription
                {
                    Priority = priority,
                    Target = typeof(TTarget),
                    Implementation = typeof(TBehavior)
                });
            }

            // Clear the behavior factories if we need to build them again

            if (_behaviorResolvers.TryRemove(typeof(TEntity), out var resolver))
                resolver.Clear();
        }

        /// <summary>
        /// Finds the behaviors in the specified assembly that are eligible.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <remarks>
        /// Access is not thread-safe!
        /// </remarks>
        private static void ScanAssembly(Assembly assembly)
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
                    var ctype = type;
                    if (attribute.GenericTypeArguments != null)
                        ctype = ctype.MakeGenericType(attribute.GenericTypeArguments);
                    types.Add(new BehaviorDescription
                    {
                        Priority = attribute.Priority,
                        Target = attribute.Target,
                        Implementation = ctype
                    });
                }
            }
        }

        /// <summary>
        /// Creates the factories for the specified entity type.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entityType"/> is <c>null</c>.</exception>
        private static IBehaviorResolver CreateFactoriesFor(Type entityType)
        {
            // First find out which context is being used by the entity
            var etype = entityType.ThrowIfNull(nameof(entityType))
                .GetTypeInfo()
                .GetInterfaces()
                .First(t =>
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

            List<BehaviorDescription> behaviors = null;
            lock (_lock)
            {
                // See if we can use use our discovered behavior descriptions
                if (!_behaviorDescriptions.TryGetValue(entityType, out behaviors) && ScanIfNotFound)
                {
                    // Since we're searching for behaviors anyway, let's import the whole assembly while we're at it
                    ScanAssembly(entityType.GetTypeInfo().Assembly);
                    behaviors = _behaviorDescriptions[entityType];
                }
            }
            if (behaviors != null)
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
        public static void Resolve(ISimulation simulation, IEntity entity, IBehaviorContainer behaviors)
        {
            var resolver = _behaviorResolvers.GetOrAdd(entity.GetType(), CreateFactoriesFor);
            resolver.Resolve(simulation, entity, behaviors);
        }

        /// <summary>
        /// Resolves behaviors for the specified simulation and entity.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="behaviors">The behaviors.</param>
        /// <param name="context">The context.</param>
        public static void Resolve<TContext>(ISimulation simulation, IEntity entity, IBehaviorContainer behaviors, TContext context)
            where TContext : IBindingContext
        {
            var resolver = (Cache<TContext>)_behaviorResolvers.GetOrAdd(entity.GetType(), CreateFactoriesFor);
            resolver.Resolve(simulation, behaviors, context);
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
            return (IBehaviorResolver<TContext>)_behaviorResolvers.GetOrAdd(typeof(TEntity), CreateFactoriesFor);
        }
    }
}
