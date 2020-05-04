using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;
using SpiceSharp.Validation;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Interface that describes an entity collection for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="IParameterized"/>
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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="subcircuit"/>, <paramref name="parentSimulation"/> or <paramref name="behaviors"/> is <c>null</c>.</exception>
        /// <exception cref="NodeMismatchException">Thrown if the number of pins of the instance don't match the number of pins defined by the definition.</exception>
        void CreateBehaviors(Subcircuit subcircuit, ISimulation parentSimulation, IBehaviorContainer behaviors);

        /// <summary>
        /// Applies the rules to the entities in the subcircuit.
        /// </summary>
        /// <param name="subcircuit">The subcircuit.</param>
        /// <param name="rules">The rules.</param>
        void Apply(Subcircuit subcircuit, IRules rules);
    }
}
