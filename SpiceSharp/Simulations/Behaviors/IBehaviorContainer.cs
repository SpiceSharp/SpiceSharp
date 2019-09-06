using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A container for behaviors
    /// </summary>
    /// <seealso cref="SpiceSharp.IParameterSet" />
    public interface IBehaviorContainer : ITypeDictionary<IBehavior>, IParameterSet
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the parameters used by the behaviors.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        ParameterSetDictionary Parameters { get; }
    }
}
