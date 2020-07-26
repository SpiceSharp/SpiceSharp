using SpiceSharp.Diagnostics;
using SpiceSharp.General;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainer"/> class.
        /// </summary>
        /// <param name="original">The original.</param>
        protected BehaviorContainer(BehaviorContainer original)
            : base(original)
        {
            Name = original.Name;
        }

        /// <inheritdoc/>
        public P GetParameterSet<P>() where P : IParameterSet
        {
            foreach (var behavior in this)
            {
                if (behavior.TryGetParameterSet(out P value))
                    return value;
            }
            throw new TypeNotFoundException(Properties.Resources.ParameterSets_NotDefined.FormatString(typeof(P).FullName));
        }

        /// <inheritdoc/>
        public bool TryGetParameterSet<P>(out P value) where P : IParameterSet
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
        public IBehaviorContainer AddIfNo<Target>(ISimulation simulation, Func<IBehavior> factory) where Target : IBehavior
        {
            if (!simulation.UsesBehaviors<Target>())
                return this;
            if (!ContainsType<Target>())
                Add(factory());
            return this;
        }

        /// <inheritdoc/>
        public override ICloneable Clone() => new BehaviorContainer(this);
    }
}
