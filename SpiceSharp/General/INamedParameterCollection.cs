using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// Interface that describes a class that contains one or more <see cref="INamedParameters"/> instances.
    /// </summary>
    public interface INamedParameterCollection : INamedParameters
    {
        /// <summary>
        /// Gets the parameters in the collection.
        /// </summary>
        /// <value>
        /// The parameters in the collection.
        /// </value>
        IEnumerable<INamedParameters> NamedParameters { get; }
    }
}
