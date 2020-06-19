using SpiceSharp.Diagnostics;
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
    /// <seealso cref="InterfaceTypeDictionary{Behavior}" />
    /// <seealso cref="IBehaviorContainer" />
    /// <seealso cref="IParameterSetCollection"/>
    public class BehaviorContainer : InterfaceTypeDictionary<IBehavior>,
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

        /// <inheritdoc/>
        public P GetParameterSet<P>() where P : IParameterSet
        {
            foreach (var behavior in Values)
            {
                if (behavior.TryGetParameterSet(out P value))
                    return value;
            }
            throw new TypeNotFoundException(Properties.Resources.ParameterSets_NotDefined.FormatString(typeof(P).FullName));
        }

        /// <inheritdoc/>
        public bool TryGetParameterSet<P>(out P value) where P : IParameterSet
        {
            foreach (var behavior in Values)
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
                foreach (var behavior in Values)
                {
                    foreach (var ps in behavior.ParameterSets)
                        yield return ps;
                }
            }
        }

        /// <inheritdoc/>
        public void SetParameter<P>(string name, P value)
        {
            foreach (var behavior in Values)
            {
                if (behavior.TrySetParameter(name, value))
                    return;
            }
            throw new ParameterNotFoundException(this, name, typeof(P));
        }

        /// <inheritdoc/>
        public bool TrySetParameter<P>(string name, P value)
        {
            foreach (var behavior in Values)
            {
                if (behavior.TrySetParameter(name, value))
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public Action<P> CreateParameterSetter<P>(string name)
        {
            foreach (var behavior in Values)
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
            foreach (var behavior in Values)
            {
                if (behavior.TryGetProperty(name, out P value))
                    return value;
            }
            throw new ParameterNotFoundException(this, name, typeof(P));
        }

        /// <inheritdoc/>
        public bool TryGetProperty<P>(string name, out P value)
        {
            foreach (var behavior in Values)
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
            foreach (var behavior in Values)
            {
                var result = behavior.CreatePropertyGetter<P>(name);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <inheritdoc/>
        public IBehaviorContainer AddIfNo<B>(ISimulation simulation, Func<B> factory) where B : IBehavior
        {
            if (!simulation.UsesBehaviors<B>())
                return this;
            if (!ContainsKey(typeof(B)))
                Add(factory());
            return this;
        }
    }
}
