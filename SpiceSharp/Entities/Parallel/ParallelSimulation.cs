using SpiceSharp.Entities.Local;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Local simulation for a <see cref="ParallelEntity"/>
    /// </summary>
    /// <seealso cref="LocalSimulation" />
    public class ParallelSimulation : LocalSimulation, IParallelSimulation
    {
        /// <summary>
        /// Gets the task that the simulation will run with.
        /// </summary>
        /// <value>
        /// The task.
        /// </value>
        public int Task { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelSimulation"/> class.
        /// </summary>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="task">The task.</param>
        public ParallelSimulation(ISimulation parent, int task)
            : base(parent)
        {
            Task = task;
            States = new TypeDictionary<ISimulationState>();
            EntityBehaviors = new LocalBehaviorContainerCollection(parent.EntityBehaviors);
        }
    }
}
