using System;
using System.Collections.Generic;
using System.Text;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.ComponentBindingContext" />
    public class SubcircuitBindingContext : ComponentBindingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitBindingContext"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="behaviors">The behaviors.</param>
        /// <param name="nodes">The nodes that the component is connected to.</param>
        /// <param name="model">The model.</param>
        public SubcircuitBindingContext(ISimulation simulation, IBehaviorContainer behaviors, IEnumerable<Variable> nodes, string model) 
            : base(simulation, behaviors, nodes, model)
        {
        }
    }
}
