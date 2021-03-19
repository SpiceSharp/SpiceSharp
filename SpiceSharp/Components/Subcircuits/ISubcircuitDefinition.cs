using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System.Collections.Generic;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Interface that describes an entity collection for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="IParameterSetCollection"/>
    public interface ISubcircuitDefinition : ICloneable<ISubcircuitDefinition>
    {
        /// <summary>
        /// Gets the entities defined in the subcircuit.
        /// </summary>
        /// <value>
        /// The entities inside the subcircuit.
        /// </value>
        IEntityCollection Entities { get; }

        /// <summary>
        /// Gets the pin names. These are the nodes that can be connected to the outside.
        /// </summary>
        /// <value>
        /// The pins.
        /// </value>
        IReadOnlyList<string> Pins { get; }
    }
}
