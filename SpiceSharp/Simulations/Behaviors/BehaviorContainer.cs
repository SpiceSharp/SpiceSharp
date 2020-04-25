using SpiceSharp.Simulations;
using SpiceSharp.General;
using System;
using System.Collections.Generic;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A dictionary of <see cref="Behavior" />. Only on instance of each type is allowed.
    /// </summary>
    /// <seealso cref="InterfaceTypeDictionary{Behavior}" />
    /// <seealso cref="IBehaviorContainer" />
    /// <seealso cref="IParameterized"/>
    public class BehaviorContainer : InterfaceTypeDictionary<IBehavior>,
        IBehaviorContainer, 
        IParameterized
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
            throw new ArgumentException(Properties.Resources.Parameters_ParameterSetNotFound);
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
