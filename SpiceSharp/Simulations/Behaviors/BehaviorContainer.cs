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
    /// <seealso cref="IBehaviorContainer" />
    /// <seealso cref="InterfaceTypeDictionary{Behavior}" />
    public class BehaviorContainer : InterfaceTypeDictionary<IBehavior>, IBehaviorContainer, IParameterized
    {
        /// <summary>
        /// Gets the source name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainer"/> class.
        /// </summary>
        /// <param name="source">The entity name that will provide the behaviors.</param>
        public BehaviorContainer(string source)
        {
            Name = source.ThrowIfNull(nameof(source));
        }

        /// <summary>
        /// Gets the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <returns>
        /// The parameter set.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the parameter set could not be found.</exception>
        public P GetParameterSet<P>() where P : IParameterSet
        {
            foreach (var behavior in Values)
            {
                if (behavior.TryGetParameterSet(out P value))
                    return value;
            }
            throw new ArgumentException(Properties.Resources.Parameters_ParameterSetNotFound);
        }

        /// <summary>
        /// Tries to get the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="value">The parameter set.</param>
        /// <returns>
        ///   <c>true</c> if the parameter set was found; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Gets all parameter sets.
        /// </summary>
        /// <value>
        /// The parameter sets.
        /// </value>
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

        /// <summary>
        /// Gets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public P GetProperty<P>(string name)
        {
            foreach (var behavior in Values)
            {
                if (behavior.TryGetProperty(name, out P value))
                    return value;
            }
            throw new ParameterNotFoundException(this, name, typeof(P));
        }

        /// <summary>
        /// Tries to get the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was found; otherwise <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Creates a getter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A getter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
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

        /// <summary>
        /// Adds a behavior to the container only if the behavior type doesn't exist yet.
        /// </summary>
        /// <typeparam name="B">The behavior (interface) type to be registering for.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="factory">The factory.</param>
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
