using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Interface that describes an entity collection for a <see cref="Subcircuit"/>.
    /// </summary>
    public interface ISubcircuitEntityCollection
    {
        /// <summary>
        /// Gets the entities defined in the subcircuit.
        /// </summary>
        /// <value>
        /// The entities inside the subcircuit.
        /// </value>
        IEntityCollection Entities { get; }

        /// <summary>
        /// Gets the number of pins defined by the subcircuit.
        /// </summary>
        /// <value>
        /// The pin count.
        /// </value>
        int PinCount { get; }

        /// <summary>
        /// Creates the behaviors for the entities in the subcircuit.
        /// </summary>
        /// <param name="parentSimulation">The parent simulation.</param>
        /// <param name="parentEntities">The parent entities.</param>
        /// <param name="behaviors">The <see cref="IBehaviorContainer"/> used for this subcircuit.</param>
        /// <param name="nodes">The nodes on the outside of the subcircuit.</param>
        void CreateBehaviors(ISimulation parentSimulation, IEntityCollection parentEntities, IBehaviorContainer behaviors, string[] nodes);
    }
}
