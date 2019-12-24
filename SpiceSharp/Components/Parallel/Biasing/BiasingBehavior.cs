using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An <see cref="IBiasingBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingBehavior" />
    public partial class BiasingBehavior : ParallelBehavior<IBiasingBehavior>, IBiasingBehavior
    {
        /// <summary>
        /// Prepares the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public static void Prepare(ParallelSimulation simulation)
        {
            if (simulation.LocalConfigurations.TryGetValue<BiasingParameters>(out var result))
            {
                if (result.LoadDistributor != null && !simulation.LocalStates.ContainsKey(typeof(IBiasingSimulationState)))
                {
                    var state = simulation.GetParentState<IBiasingSimulationState>();
                    simulation.LocalStates.Add(new SimulationState(state));
                }
            }
        }

        private readonly IWorkDistributor _load;
        private readonly IWorkDistributor<bool> _convergence;
        private readonly SimulationState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="simulation">The parallel simulation.</param>
        public BiasingBehavior(string name, ParallelSimulation simulation)
            : base(name, simulation)
        {
            if (simulation.LocalConfigurations.TryGetValue<BiasingParameters>(out var result))
            {
                _convergence = result.ConvergenceDistributor;
                _load = result.LoadDistributor;
                _state = simulation.GetState<SimulationState>();
            }
            else
            {
                _convergence = null;
                _load = null;
                _state = null;
            }
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvergent()
        {
            if (_convergence != null)
            {
                var methods = new Func<bool>[Behaviors.Count];
                for (var i = 0; i < methods.Length; i++)
                {
                    var behavior = Behaviors[i];
                    methods[i] = () => behavior.IsConvergent();
                }
                return _convergence.Execute(methods);
            }
            else
            {
                var convergence = true;
                foreach (var behavior in Behaviors)
                    convergence &= behavior.IsConvergent();
                return convergence;
            }
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        public void Load()
        {
            if (_load != null)
            {
                _state.Reset();

                var methods = new Action[Behaviors.Count];
                for (var i = 0; i < methods.Length; i++)
                {
                    var behavior = Behaviors[i];
                    methods[i] = () => behavior.Load();
                }
                _load.Execute(methods);

                // Apply the changes to the parent
                _state.Apply();
            }
            else
            {
                foreach (var behavior in Behaviors)
                    behavior.Load();
            }
        }
    }
}
