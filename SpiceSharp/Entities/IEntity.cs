using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Interface describing an entity that can provide behaviors to a <see cref="ISimulation"/>.
    /// </summary>
    public interface IEntity : ICloneable, IParameterized, IImportParameterSet<IEntity>
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
        /// <returns>A dictionary of behaviors that can be used by the simulation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="simulation"/> is <c>null</c>.</exception>
        void CreateBehaviors(ISimulation simulation);
    }
}
