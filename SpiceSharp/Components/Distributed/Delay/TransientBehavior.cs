using System;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="Delay" />
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseTransientBehavior" />
    public class TransientBehavior : BaseTransientBehavior, IConnectedBehavior
    {
        // Necessary behaviors and parameters
        private BaseParameters _bp;
        private LoadBehavior _load;

        /// <summary>
        /// States
        /// </summary>
        protected StateDerivative OutputRate { get; private set; }
        public Tracker InputTracker { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _branch, _input;
        protected VectorElement<double> BranchPtr { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior" /> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TransientBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Connect the behavior in the circuit
        /// </summary>
        /// <param name="pins">Pin indices in order</param>
        public void Connect(int[] pins)
        {
            _input = pins[0];
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();

            // Get behaviors
            _load = provider.GetBehavior<LoadBehavior>();

            // Make sure the timestep doesn't go beyond the delay
            if (simulation is TimeSimulation ts)
            {
                if (_bp.TimeDelay < simulation.Configurations.Get<TimeConfiguration>().MaxStep)
                    ts.Method.TruncateProbe += TruncateProbeDelay;
            }
        }

        /// <summary>
        /// Truncates the probe delay.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="truncateTimestepEventArgs">The <see cref="TruncateTimestepEventArgs"/> instance containing the event data.</param>
        private void TruncateProbeDelay(object sender, TruncateTimestepEventArgs truncateTimestepEventArgs)
        {
            // Internally the timestep will only be shorter
            truncateTimestepEventArgs.Delta = _bp.TimeDelay;
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Unsetup(Simulation simulation)
        {
            _bp = null;
            _load = null;
            if (simulation is TimeSimulation ts)
                ts.Method.TruncateProbe -= TruncateProbeDelay;
        }

        /// <summary>
        /// Creates all necessary states for the transient behavior.
        /// </summary>
        /// <param name="method">The integration method.</param>
        public override void CreateStates(IntegrationMethod method)
        {
            OutputRate = method.CreateDerivative();
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public override void GetEquationPointers(Solver<double> solver)
        {
            _branch = _load.Branch;
            BranchPtr = solver.GetRhsElement(_branch);
            
            // Create the tracker for looking back in time
            InputTracker = new Tracker(_input, _bp.TimeDelay);
        }

        /// <summary>
        /// Perform time-dependent calculations.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Transient(TimeSimulation simulation)
        {
            // Get the value looking back in time
            InputTracker.Probe(simulation);
            var value = InputTracker.GetValue(simulation);

            // Update our state to enable timestep truncation
            OutputRate.Current = value;
            OutputRate.Integrate();

            // Load the Y-matrix and RHS-vector
            BranchPtr.Value += value;
        }
    }
}
