using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Interface that describes an entity collection for a <see cref="Subcircuit"/>.
    /// </summary>
    public interface ISubcircuitDefinition : IParameterized
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
        /// <param name="subcircuit">The subcircuit that wants to create the behaviors through the definition.</param>
        /// <param name="parentSimulation">The parent simulation.</param>
        /// <param name="behaviors">The <see cref="IBehaviorContainer" /> used for the subcircuit.</param>
        void CreateBehaviors(Subcircuit subcircuit, ISimulation parentSimulation, IBehaviorContainer behaviors);
    }
}
