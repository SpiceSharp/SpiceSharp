using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using System.Collections.Generic;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A template for a binding context to bind component behaviors to simulations.
    /// </summary>
    /// <seealso cref="IBindingContext" />
    public interface IComponentBindingContext :
        IBindingContext
    {
        /// <summary>
        /// Gets the model behaviors.
        /// </summary>
        /// <value>
        /// The model behaviors.
        /// </value>
        IBehaviorContainer ModelBehaviors { get; }

        /// <summary>
        /// Gets the nodes that the component is connected to.
        /// </summary>
        /// <value>
        /// The nodes.
        /// </value>
        IReadOnlyList<string> Nodes { get; }
    }
}
