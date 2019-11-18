using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A dictionary of <see cref="Behavior" />. Only on instance of each type is allowed.
    /// </summary>
    /// <seealso cref="TypeDictionary{Behavior}" />
    public class BehaviorContainer : TypeDictionary<IBehavior>, IBehaviorContainer, INamedParameterCollection
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
        /// Gets the parameters in the collection.
        /// </summary>
        /// <value>
        /// The parameters in the collection.
        /// </value>
        IEnumerable<INamedParameters> INamedParameterCollection.NamedParameters
        {
            get
            {
                foreach (var behavior in Values)
                    yield return behavior;
                foreach (var ps in Parameters.Values)
                    yield return ps;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainer"/> class.
        /// </summary>
        /// <param name="source">The entity identifier that will provide the behaviors.</param>
        public BehaviorContainer(string source)
            : base(true)
        {
            Name = source.ThrowIfNull(nameof(source));
            Parameters = new ParameterSetDictionary();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainer"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="parameters">The parameters.</param>
        public BehaviorContainer(string source, IParameterSetDictionary parameters)
            : base(true)
        {
            Name = source.ThrowIfNull(nameof(source));
            Parameters = parameters.ThrowIfNull(nameof(parameters));
        }
    }
}
