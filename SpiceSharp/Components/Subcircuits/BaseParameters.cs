using SpiceSharp.Attributes;
using SpiceSharp.Circuits;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="Subcircuit"/>.
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the entities.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        [ParameterName("entities"), ParameterName("e"), ParameterInfo("The entities in the subcircuit")]
        public IEntityCollection Entities { get; set; }

        /// <summary>
        /// Gets the set of global nodes.
        /// </summary>
        /// <remarks>
        /// Global nodes are nodes that are shared among all subcircuits without explicit connection. Typical examples
        /// are supply voltages ("VDD", "VEE", etc.). The ground node "0" is always treated as a global node, as well 
        /// as any other identifiers that map to the ground node.
        /// </remarks>
        /// <value>
        /// The global nodes.
        /// </value>
        [ParameterName("globals"), ParameterInfo("The global nodes in the subcircuit")]
        public HashSet<string> GlobalNodes { get; set; }

        /// <summary>
        /// Gets the local pin names. These will globally look like other pin names.
        /// </summary>
        /// <value>
        /// The local pin names.
        /// </value>
        public string[] Pins { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="pins">The pins.</param>
        public BaseParameters(IEntityCollection entities, params string[] pins)
        {
            Entities = entities;
            pins.ThrowIfNull(nameof(pins));
            Pins = new string[pins.Length];
            for (var i = 0; i < pins.Length; i++)
                Pins[i] = pins[i].ThrowIfNull("Pin {0}".FormatString(i));
        }
    }
}
