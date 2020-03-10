using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An <see cref="IConvergenceBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="BiasingBehavior" />
    /// <seealso cref="IConvergenceBehavior" />
    public partial class ConvergenceBehavior : BiasingBehavior, IConvergenceBehavior
    {
        /// <summary>
        /// Prepares the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public static void Prepare(ParallelSimulation simulation)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<BaseParameters>();

            if (simulation.UsesState<IBiasingSimulationState>())
            {
                if (parameters.LoadDistributor != null && !simulation.LocalStates.ContainsKey(typeof(IBiasingSimulationState)))
                {
                    var state = simulation.GetParentState<IBiasingSimulationState>();
                    simulation.LocalStates.Add(new BiasingBehavior.BiasingSimulationState(state));
                }
            }

            if (simulation.UsesState<IIterationSimulationState>())
            {
                if ((parameters.ConvergenceDistributor != null || parameters.LoadDistributor != null) && !simulation.LocalStates.ContainsKey(typeof(IIterationSimulationState)))
                {
                    var state = simulation.GetParentState<IIterationSimulationState>();
                    simulation.LocalStates.Add(new IterationSimulationState(state));
                }
            }
        }

        private readonly BehaviorList<IConvergenceBehavior> _convergenceBehaviors;
        private readonly Workload<bool> _convergenceWorkload;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvergenceBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public ConvergenceBehavior(string name, ParallelSimulation simulation)
            : base(name, simulation)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<BaseParameters>();
            if (parameters.ConvergenceDistributor != null)
                _convergenceWorkload = new Workload<bool>(parameters.ConvergenceDistributor, simulation.EntityBehaviors.Count);
            _convergenceBehaviors = simulation.EntityBehaviors.GetBehaviorList<IConvergenceBehavior>();
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
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
