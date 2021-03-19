using SpiceSharp.General;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Interface describing an entity that can provide behaviors to a <see cref="ISimulation"/>.
    /// </summary>
    /// <seealso cref="IParameterSetCollection"/>
    public interface IEntity :
        IParameterSetCollection, ICloneable<IEntity>
    {
        /// <summary>
        /// Gets the name of the entity.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether behaviors need to be linked to the original entity. If this is <c>true</c>,
        /// then changing parameter values on the entity will be reflected in any behaviors that this entity creates. If
        /// <c>false</c>, then parameters are cloned as behaviors request it.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [link parameters]; otherwise, <c>false</c>.
        /// </value>
        bool LinkParameters { get; }

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
