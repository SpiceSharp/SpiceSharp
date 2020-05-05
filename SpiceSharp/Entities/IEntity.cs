using System;
using SpiceSharp.Simulations;
using SpiceSharp.General;
using SpiceSharp.Diagnostics;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Interface describing an entity that can provide behaviors to a <see cref="ISimulation"/>.
    /// </summary>
    /// <seealso cref="ICloneable"/>
    /// <seealso cref="IParameterSetCollection"/>
    public interface IEntity :
        ICloneable,
        IParameterSetCollection
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
        /// <exception cref="ArgumentException">Thrown if the simulation does not use an <see cref="IComplexSimulationState"/>.</exception>
        /// <exception cref="TypeNotFoundException">Thrown if a required behavior or parameter set could not be found.</exception>
        /// <exception cref="AmbiguousTypeException">Thrown if a behavior or parameter set could not be resolved unambiguously.</exception>
        void CreateBehaviors(ISimulation simulation);
    }
}
