using SpiceSharp.Diagnostics;
using SpiceSharp.Entities;
using SpiceSharp.General;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A dictionary of <see cref="Behavior" />. Only on instance of each type is allowed.
    /// </summary>
    /// <seealso cref="IBehaviorContainer" />
    /// <seealso cref="IParameterSetCollection"/>
    public class BehaviorContainer : InterfaceTypeSet<IBehavior>,
        IBehaviorContainer,
        IParameterSetCollection
    {
        /// <summary>
        /// Default implementation of the <see cref="IBehaviorContainerBuilder{TContext}"/>.
        /// </summary>
        /// <typeparam name="TContext">The type of binding context.</typeparam>
        /// <seealso cref="IBehaviorContainerBuilder{TContext}" />
        protected class BehaviorContainerBuilder<TContext> : IBehaviorContainerBuilder<TContext>
            where TContext : IBindingContext
        {
            private readonly IBehaviorContainer _container;
            private readonly ISimulation _simulation;
            private readonly TContext _context;

            /// <summary>
            /// Initializes a new instance of the <see cref="BehaviorContainerBuilder{TContext}"/> class.
            /// </summary>
            /// <param name="container">The container.</param>
            /// <param name="simulation">The simulation.</param>
            /// <param name="context">The context.</param>
            public BehaviorContainerBuilder(IBehaviorContainer container, ISimulation simulation, TContext context)
            {
                _container = container.ThrowIfNull(nameof(container));
                _simulation = simulation.ThrowIfNull(nameof(simulation));
                _context = context;
            }

            /// <inheritdoc/>
            public IBehaviorContainerBuilder<TContext> AddIfNo<TBehavior>(Func<TContext, IBehavior> factory) where TBehavior : IBehavior
            {
                factory.ThrowIfNull(nameof(factory));
                if (!_simulation.UsesBehaviors<TBehavior>())
                    return this;
                if (!_container.ContainsType<TBehavior>())
                    _container.Add(factory(_context));
                return this;
            }
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainer"/> class.
        /// </summary>
        /// <param name="source">The entity name that will provide the behaviors.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <c>null</c>.</exception>
        public BehaviorContainer(string source)
        {
            Name = source.ThrowIfNull(nameof(source));
        }

        /// <inheritdoc/>
        public P GetParameterSet<P>() where P : IParameterSet, ICloneable<P>
        {
            foreach (var behavior in this)
            {
                if (behavior.TryGetParameterSet(out P value))
                    return value;
            }
            throw new TypeNotFoundException(Properties.Resources.ParameterSets_NotDefined.FormatString(typeof(P).FullName));
        }

        /// <inheritdoc/>
        public bool TryGetParameterSet<P>(out P value) where P : IParameterSet, ICloneable<P>
        {
            foreach (var behavior in this)
            {
                if (behavior.TryGetParameterSet(out value))
                    return true;
            }
            value = default;
            return false;
        }

        /// <inheritdoc/>
        public IEnumerable<IParameterSet> ParameterSets
        {
            get
            {
                foreach (var behavior in this)
                {
                    foreach (var ps in behavior.ParameterSets)
                        yield return ps;
                }
            }
        }

        /// <inheritdoc/>
        public void SetParameter<P>(string name, P value)
        {
            foreach (var behavior in this)
            {
                if (behavior.TrySetParameter(name, value))
                    return;
            }
            throw new ParameterNotFoundException(this, name, typeof(P));
        }

        /// <inheritdoc/>
        public bool TrySetParameter<P>(string name, P value)
        {
            foreach (var behavior in this)
            {
                if (behavior.TrySetParameter(name, value))
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public Action<P> CreateParameterSetter<P>(string name)
        {
            foreach (var behavior in this)
            {
                var result = behavior.CreateParameterSetter<P>(name);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <inheritdoc/>
        public P GetProperty<P>(string name)
        {
            foreach (var behavior in this)
            {
                if (behavior.TryGetProperty(name, out P value))
                    return value;
            }
            throw new ParameterNotFoundException(this, name, typeof(P));
        }

        /// <inheritdoc/>
        public bool TryGetProperty<P>(string name, out P value)
        {
            foreach (var behavior in this)
            {
                if (behavior.TryGetProperty(name, out value))
                    return true;
            }
            value = default;
            return false;
        }

        /// <inheritdoc/>
        public Func<P> CreatePropertyGetter<P>(string name)
        {
            foreach (var behavior in this)
            {
                var result = behavior.CreatePropertyGetter<P>(name);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <inheritdoc/>
        public IBehaviorContainerBuilder<TContext> Build<TContext>(ISimulation simulation, TContext context) where TContext : IBindingContext
            => new BehaviorContainerBuilder<TContext>(this, simulation, context);
    }
}
