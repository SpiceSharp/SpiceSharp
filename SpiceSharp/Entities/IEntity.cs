using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Interface describing an entity that can provide behaviors to a <see cref="ISimulation"/>.
    /// </summary>
    public interface IEntity : ICloneable, IParameterized<IEntity>
    {
        /// <summary>
        /// Gets the name of the entity.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Creates the behaviors and stores them in the specified container.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="behaviors">An <see cref="IBehaviorContainer"/> where the behaviors can be stored.</param>
        /// <returns>A dictionary of behaviors that can be used by the simulation.</returns>
        void CreateBehaviors(ISimulation simulation, IBehaviorContainer behaviors);
    }
}
