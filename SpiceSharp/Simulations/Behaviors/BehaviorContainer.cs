using SpiceSharp.Simulations;
using SpiceSharp.General;
using System;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A dictionary of <see cref="Behavior" />. Only on instance of each type is allowed.
    /// </summary>
    /// <seealso cref="IBehaviorContainer" />
    /// <seealso cref="InterfaceTypeDictionary{Behavior}" />
    public class BehaviorContainer : InterfaceTypeDictionary<IBehavior>, IBehaviorContainer
    {
        /// <summary>
        /// Gets the source identifier.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the parameters used by the behaviors.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public IParameterSetDictionary Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainer"/> class.
        /// </summary>
        /// <param name="source">The entity identifier that will provide the behaviors.</param>
        /// <param name="parameters">The parameters.</param>
        public BehaviorContainer(string source, IParameterSetDictionary parameters)
            : base()
        {
            Name = source.ThrowIfNull(nameof(source));
            Parameters = parameters.ThrowIfNull(nameof(parameters));
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
            return Parameters.GetProperty<P>(name);
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
            return Parameters.TryGetProperty(name, out value);
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
            return Parameters.CreatePropertyGetter<P>(name);
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
