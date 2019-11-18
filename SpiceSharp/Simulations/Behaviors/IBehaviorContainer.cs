using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A container for behaviors
    /// </summary>
    /// <seealso cref="ITypeDictionary{T}" />
    /// <seealso cref="INamedParameterCollection" />
    public interface IBehaviorContainer : ITypeDictionary<IBehavior>, INamedParameterCollection
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        IParameterSetDictionary Parameters { get; }
    }
}
