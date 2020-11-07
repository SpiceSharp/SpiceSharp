using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharp.Entities.DependencyInjection
{
    /// <summary>
    /// A helper class for generating binding contexts.
    /// </summary>
    public static class ContextFactory
    {
        private static readonly ConcurrentDictionary<Type, Method> _factories = new ConcurrentDictionary<Type, Method>();

        /// <summary>
        /// A delegate that describes the possible inputs for a context.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="container">The container with the behaviors.</param>
        /// <returns>The context.</returns>
        public delegate IBindingContext Method(ISimulation simulation, IEntity entity, IBehaviorContainer container);

        /// <summary>
        /// Creates the factory for creating binding contexts.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the constructor cannot be resolved.</exception>
        private static Method CreateFactory(Type type)
        {
            // Create the constructor for the binding context
            var ctors = type.GetTypeInfo().GetConstructors();
            ConstructorInfo ctor;
            if (ctors == null)
                ctor = null;
            else if (ctors.Length > 0)
                ctor = ctors[0];
            else
                throw new ArgumentException(Properties.Resources.DI_CannotResolveConstructor.FormatString(type.FullName));

            var pSimulation = Expression.Parameter(typeof(ISimulation), "simulation");
            var pEntity = Expression.Parameter(typeof(IEntity), "entity");
            var pContainer = Expression.Parameter(typeof(IBehaviorContainer), "behaviors");
            var parameters = ctor.GetParameters();
            var arguments = new Expression[parameters.Length];
            foreach (var p in parameters)
            {
                var ptype = p.ParameterType;
                if (ptype == typeof(ISimulation))
                    arguments[p.Position] = pSimulation;
                else if (ptype == typeof(IBehaviorContainer))
                    arguments[p.Position] = pContainer;
                else if (ptype == typeof(IEntity))
                    arguments[p.Position] = pEntity;

                // Harder/slower checks
                else if (typeof(IEntity).GetTypeInfo().IsAssignableFrom(ptype))
                    arguments[p.Position] = Expression.TypeAs(pEntity, ptype);
                else if (typeof(ISimulation).GetTypeInfo().IsAssignableFrom(ptype))
                    arguments[p.Position] = Expression.TypeAs(pSimulation, ptype);
                else if (typeof(IBehaviorContainer).GetTypeInfo().IsAssignableFrom(ptype))
                    arguments[p.Position] = Expression.TypeAs(pContainer, ptype);
                else
                    throw new ArgumentException(Properties.Resources.DI_InvalidConstructorParameter.FormatString(ptype.FullName, type.FullName));
            }
            return Expression.Lambda<Method>(Expression.New(ctor, arguments), pSimulation, pEntity, pContainer).Compile();
        }

        /// <summary>
        /// Gets a binding context of the specified type.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="container">The container.</param>
        /// <returns>The binding context.</returns>
        public static TContext Get<TContext>(ISimulation simulation, IEntity entity, IBehaviorContainer container) where TContext : IBindingContext
        {
            var factory = _factories.GetOrAdd(typeof(TContext), CreateFactory);
            return (TContext)factory(simulation, entity, container);
        }
    }
}
