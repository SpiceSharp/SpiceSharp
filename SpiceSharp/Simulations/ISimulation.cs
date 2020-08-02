using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Interface that describes a simulation that outputs its solved variables.
    /// </summary>
    /// <typeparam name="V">The type of variables.</typeparam>
    /// <seealso cref="ISimulation" />
    public interface ISimulation<V> : ISimulation where V : IVariable
    {
        /// <summary>
        /// Gets the variables that are being solved by the simulation.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        IVariableDictionary<V> Solved { get; }
    }

    /// <summary>
    /// Interface that describes a simulation.
    /// </summary>
    /// <seealso cref="IStateful" />
    /// <seealso cref="IBehavioral" />
    /// <seealso cref="IParameterSetCollection" />
    public interface ISimulation : IStateful, IBehavioral, IParameterSetCollection
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
        /// The current status.
        /// </value>
        SimulationStatus Status { get; }

        /// <summary>
        /// Runs the <see cref="ISimulation"/> on the specified <see cref="IEntityCollection"/>.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void Run(IEntityCollection entities);

        /// <summary>
        /// Reruns the <see cref="ISimulation"/> with the previous behaviors.
        /// </summary>
        void Rerun();
    }
}
