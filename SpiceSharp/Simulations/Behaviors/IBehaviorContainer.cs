using SpiceSharp.Behaviors;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A container for behaviors
    /// </summary>
    /// <seealso cref="ITypeDictionary{T}" />
    public interface IBehaviorContainer : 
        ITypeDictionary<IBehavior>, IExportParameterSet
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
