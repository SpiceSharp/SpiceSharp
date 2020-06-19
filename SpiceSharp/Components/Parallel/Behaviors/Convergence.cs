using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="IConvergenceBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="Biasing" />
    /// <seealso cref="IConvergenceBehavior" />
    public partial class Convergence : Biasing,
        IConvergenceBehavior
    {
        /// <summary>
        /// Prepares the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public static void Prepare(ParallelSimulation simulation)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<Parameters>();

            if (simulation.UsesState<IBiasingSimulationState>())
            {
                if (parameters.BiasLoadDistributor != null && !simulation.LocalStates.ContainsKey(typeof(IBiasingSimulationState)))
                {
                    var state = simulation.GetParentState<IBiasingSimulationState>();
                    simulation.LocalStates.Add(new Biasing.BiasingSimulationState(state));
                }
            }

            if (simulation.UsesState<IIterationSimulationState>())
            {
                if ((parameters.BiasConvergenceDistributor != null || parameters.BiasLoadDistributor != null) && !simulation.LocalStates.ContainsKey(typeof(IIterationSimulationState)))
                {
                    var state = simulation.GetParentState<IIterationSimulationState>();
                    simulation.LocalStates.Add(new IterationSimulationState(state));
                }
            }
        }

        private readonly BehaviorList<IConvergenceBehavior> _convergenceBehaviors;
        private readonly Workload<bool> _convergenceWorkload;

        /// <summary>
        /// Initializes a new instance of the <see cref="Convergence"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public Convergence(string name, ParallelSimulation simulation)
            : base(name, simulation)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<Parameters>();
            if (parameters.BiasConvergenceDistributor != null)
                _convergenceWorkload = new Workload<bool>(parameters.BiasConvergenceDistributor, simulation.EntityBehaviors.Count);
            _convergenceBehaviors = simulation.EntityBehaviors.GetBehaviorList<IConvergenceBehavior>();
        }

        /// <inheritdoc/>
        bool IConvergenceBehavior.IsConvergent()
        {
            if (_convergenceWorkload != null)
                return _convergenceWorkload.Execute();
            else
            {
                var convergence = true;
                foreach (var behavior in _convergenceBehaviors)
                    convergence &= behavior.IsConvergent();
                return convergence;
            }
        }
    }
}
