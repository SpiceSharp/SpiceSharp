using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System.Collections;
using System.Collections.Generic;

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
        /// Gets the index of the current simulation run.
        /// </summary>
        int CurrentRun { get; }

        /// <summary>
        /// If set, the simulation is repeated. The property is automatically reset
        /// at the start of an execution.
        /// </summary>
        bool Repeat { get; set; }

        /// <summary>
        /// Gets the current status of the <see cref="ISimulation"/>.
        /// </summary>
        /// <value>
        /// The current status.
        /// </value>
        SimulationStatus Status { get; }

        /// <summary>
        /// Runs the <see cref="ISimulation"/> on the specified <see cref="IEntityCollection"/>. Only one
        /// enumerable can run at any given time.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="mask">A bit mask for simulation export identifiers.</param>
        /// <returns>An enumerable that yields a type identifier every time new simulation data is available.</returns>
        IEnumerable<int> Run(IEntityCollection entities, int mask = Simulation.Exports);

        /// <summary>
        /// Reruns the <see cref="ISimulation"/> with the previous behaviors. Only one
        /// enumerable can run at any given time.
        /// </summary>
        /// <param name="mask">A bit mask for simulation export identifiers.</param>
        /// <returns>An enumerable that yields a type identifier every time new simulation data is available.</returns>
        IEnumerable<int> Rerun(int mask = Simulation.Exports);
    }
}
