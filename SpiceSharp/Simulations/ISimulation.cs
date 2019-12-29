using SpiceSharp.Behaviors;
using SpiceSharp.Entities;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Interface that describes a simulation.
    /// </summary>
    /// <seealso cref="IStateful" />
    /// <seealso cref="IBehavioral" />
    /// <seealso cref="IParameterized" />
    public interface ISimulation : IStateful, IBehavioral, IParameterized
    {
        /// <summary>
        /// Gets the name of the <see cref="ISimulation"/>.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the current status of the <see cref="ISimulation"/>.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        SimulationStatus Status { get; }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        IVariableSet Variables { get; }

        /// <summary>
        /// Gets the entity behaviors.
        /// </summary>
        /// <value>
        /// The entity behaviors.
        /// </value>
        IBehaviorContainerCollection EntityBehaviors { get; }

        /// <summary>
        /// Runs the <see cref="ISimulation"/> on the specified <see cref="IEntityCollection"/>.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void Run(IEntityCollection entities);
    }

    /// <summary>
    /// Possible statuses for a simulation.
    /// </summary>
    public enum SimulationStatus
    {
        /// <summary>
        /// Indicates that the simulation has not started.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that the simulation is now in its setup phase.
        /// </summary>
        Setup,

        /// <summary>
        /// Indicates that the simulation is validating the input.
        /// </summary>
        Validation,

        /// <summary>
        /// Indicates that the simulation is running.
        /// </summary>
        Running,

        /// <summary>
        /// Indicates that the simulation is cleaning up all its resources.
        /// </summary>
        Unsetup
    }
}
