using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharp.Entities
{
    public static partial class DI
    {
        /// <summary>
        /// The dependency injection container that is used by entities to
        /// create behaviors.
        /// </summary>
        protected class Cache<TContext> : IBehaviorResolver<TContext>
            where TContext : IBindingContext
        {
            private readonly List<BuildBehavior> _builders = new List<BuildBehavior>();

            private delegate IBehaviorResolver<TContext> RegisterMethod(Cache<TContext> instance, Func<TContext, IBehavior> factory);
            private class RegisterMethods
            {
                public RegisterMethod After;
                public RegisterMethod Before;
            }
            private static readonly ConcurrentDictionary<Type, RegisterMethods> _methodCache = new ConcurrentDictionary<Type, RegisterMethods>();

            /// <summary>
            /// A delegate for building behaviors using the specified binding context.
            /// </summary>
            /// <param name="builder">The context.</param>
            public delegate void BuildBehavior(IBehaviorContainerBuilder<TContext> builder);

            /// <inheritdoc/>
            public IBehaviorResolver<TContext> RegisterAfter<TBehavior>(Func<TContext, IBehavior> factory)
                where TBehavior : IBehavior
            {
                _builders.Add(container => container.AddIfNo<TBehavior>(factory));
                return this;
            }

            /// <inheritdoc/>
            public IBehaviorResolver<TContext> RegisterBefore<TBehavior>(Func<TContext, IBehavior> factory)
                where TBehavior : IBehavior
            {
                _builders.Insert(0, container => container.AddIfNo<TBehavior>(factory));
                return this;
            }

            /// <inheritdoc/>
            public IBehaviorResolver<TContext> RegisterAfter<TBehavior, TBehaviorImpl>()
                where TBehavior : IBehavior
                where TBehaviorImpl : TBehavior, IBehavior
                => RegisterAfter<TBehavior>(CreateFactory(typeof(TBehaviorImpl)));

            /// <inheritdoc/>
            public IBehaviorResolver<TContext> RegisterBefore<TBehavior, TBehaviorImpl>()
                where TBehavior : IBehavior
                where TBehaviorImpl : TBehavior, IBehavior
                => RegisterBefore<TBehavior>(CreateFactory(typeof(TBehaviorImpl)));

            /// <summary>
            /// Creates a factory for a behavior.
            /// </summary>
            /// <param name="behaviorType">Type of the behavior.</param>
            /// <returns>
            /// The factory.
            /// </returns>
            /// <exception cref="ArgumentException">Thrown if the constructor cannot be resolved.</exception>
            private Func<TContext, IBehavior> CreateFactory(Type behaviorType)
            {
                try
                {
                    var ctors = behaviorType.GetTypeInfo().GetConstructors();
                    if (ctors == null || ctors.Length != 1)
                        throw new ArgumentException();
                    var pContext = Expression.Parameter(typeof(TContext), "context");
                    return Expression.Lambda<Func<TContext, IBehavior>>(Expression.New(ctors[0], pContext), pContext).Compile();
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException(Properties.Resources.DI_CannotResolveConstructor.FormatString(behaviorType.FullName));
                }
            }

            /// <inheritdoc/>
            public void Clear()
            {
                _builders.Clear();
            }

            /// <inheritdoc/>
            public void Resolve(ISimulation simulation, IEntity entity, IBehaviorContainer container)
            {
                // Reading is not really a problem
                var context = Context<TContext>.Get(simulation, entity, container);
                var builder = container.Build(simulation, context);
                foreach (var action in _builders)
                    action(builder);
            }

            /// <inheritdoc/>
            public void Resolve(ISimulation simulation, IBehaviorContainer container, TContext context)
            {
                var builder = container.Build(simulation, context);
                foreach (var action in _builders)
                    action(builder);
            }

            /// <inheritdoc/>
            IBehaviorResolver IBehaviorResolver.RegisterAfter(Type behavior, Type behaviorImplementation)
            {
                var factory = CreateFactory(behaviorImplementation);
                GetRegisterMethods(behavior).After(this, factory);
                return this;
            }

            /// <inheritdoc/>
            IBehaviorResolver IBehaviorResolver.RegisterBefore(Type behavior, Type behaviorImplementation)
            {
                var factory = CreateFactory(behaviorImplementation);
                GetRegisterMethods(behavior).Before(this, factory);
                return this;
            }

            /// <summary>
            /// Gets the register methods for a specified behavior.
            /// </summary>
            /// <param name="behavior">The behavior.</param>
            /// <returns></returns>
            private RegisterMethods GetRegisterMethods(Type behavior)
            {
                return _methodCache.GetOrAdd(behavior, type =>
                {
                    var info = typeof(Cache<TContext>).GetTypeInfo();
                    var result = new RegisterMethods();
                    var pCache = Expression.Parameter(typeof(Cache<TContext>), "cache");
                    var pFunc = Expression.Parameter(typeof(Func<TContext, IBehavior>), "factory");
                    foreach (var method in info.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (!method.IsGenericMethod || method.GetParameters().Length != 1)
                            continue;
                        if (string.CompareOrdinal(method.Name, nameof(RegisterAfter)) == 0)
                            result.After = Expression.Lambda<RegisterMethod>(Expression.Call(pCache, method.MakeGenericMethod(behavior), pFunc), pCache, pFunc).Compile();
                        else if (string.CompareOrdinal(method.Name, nameof(RegisterBefore)) == 0)
                            result.Before = Expression.Lambda<RegisterMethod>(Expression.Call(pCache, method.MakeGenericMethod(behavior), pFunc), pCache, pFunc).Compile();
                    }
                    return result;
                });
            }
        }
    }
}
