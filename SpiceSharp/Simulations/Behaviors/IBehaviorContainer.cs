using SpiceSharp.General;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A container for behaviors
    /// </summary>
    /// <seealso cref="IParameterSetCollection"/>
    public interface IBehaviorContainer :
        ITypeSet<IBehavior>,
        IParameterSetCollection
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name of the behavior container.
        /// </value>
        /// <remarks>
        /// This is typically the name of the entity that creates the behaviors in this container.
        /// </remarks>
        string Name { get; }
    }
}
