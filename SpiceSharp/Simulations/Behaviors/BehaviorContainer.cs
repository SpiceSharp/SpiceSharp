using System;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A dictionary of <see cref="Behavior" />. Only on instance of each type is allowed.
    /// </summary>
    /// <seealso cref="TypeDictionary{Behavior}" />
    public class BehaviorContainer : InheritedTypeDictionary<IBehavior>, IBehaviorContainer
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
        public P Get<P>(string name)
        {
            foreach (var behavior in Values)
            {
                if (behavior.TryGet(name, out P value))
                    return value;
            }
            return Parameters.Get<P>(name);
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
        public bool TryGet<P>(string name, out P value)
        {
            foreach (var behavior in Values)
            {
                if (behavior.TryGet(name, out value))
                    return true;
            }
            return Parameters.TryGet(name, out value);
        }

        /// <summary>
        /// Creates a getter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A getter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        public Func<P> CreateGetter<P>(string name)
        {
            foreach (var behavior in Values)
            {
                var result = behavior.CreateGetter<P>(name);
                if (result != null)
                    return result;
            }
            return Parameters.CreateGetter<P>(name);
        }
    }
}
