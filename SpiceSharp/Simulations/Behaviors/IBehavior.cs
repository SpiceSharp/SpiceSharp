using SpiceSharp.ParameterSets;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Contract for a behavior.
    /// </summary>
    /// <seealso cref="IParameterSet"/>
    public interface IBehavior : IParameterSetCollection
    {
        /// <summary>
        /// Gets the name of the behavior.
        /// </summary>
        /// <value>
        /// The name of the behavior.
        /// </value>
        /// <remarks>
        /// This is typically the name of the entity that created it.
        /// </remarks>
        string Name { get; }
    }
}
