using SpiceSharp.Attributes;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="Subcircuit"/>.
    /// </summary>
    public class BaseParameters : ParameterSet
    {
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
        /// Gets or sets the number of tasks used when parallel computations are enabled.
        /// When the default 0 is specified, the number of processors is used as the default. When -1
        /// is specified, then each entity in the subcircuit will receive its own task. Otherwise,
        /// the number of tasks is used.
        /// </summary>
        /// <value>
        /// The number of tasks.
        /// </value>
        [ParameterName("tasks"), ParameterInfo("The number of tasks to be used")]
        public int Tasks { get; set; }
    }
}
