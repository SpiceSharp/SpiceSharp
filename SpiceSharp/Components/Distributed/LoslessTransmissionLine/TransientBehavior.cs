using System;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Distributed;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseTransientBehavior" />
    public class TransientBehavior : BaseTransientBehavior, IConnectedBehavior
    {
        // Necessary behaviors and parameters
        private BaseParameters _bp;
        private LoadBehavior _load;

        /// <summary>
        /// Gets the delayed signals.
        /// </summary>
        /// <value>
        /// The signals.
        /// </value>
        public DelayedSignal Signals { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _pos1, _neg1, _pos2, _neg2, _branchEq1, _branchEq2;
        protected VectorElement<double> Ibr1Ptr { get; private set; }
        protected VectorElement<double> Ibr2Ptr { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TransientBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Connect the behavior in the circuit
        /// </summary>
        /// <param name="pins">Pin indices in order</param>
        public void Connect(params int[] pins)
        {
            _pos1 = pins[0];
            _neg1 = pins[1];
            _pos2 = pins[2];
            _neg2 = pins[3];
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();

            // Get behaviors
            _load = provider.GetBehavior<LoadBehavior>();
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public override void GetEquationPointers(Solver<double> solver)
        {
            _branchEq1 = _load.BranchEq1;
            _branchEq2 = _load.BranchEq2;

            Ibr1Ptr = solver.GetRhsElement(_branchEq1);
            Ibr2Ptr = solver.GetRhsElement(_branchEq2);
        }

        /// <summary>
        /// Creates all necessary states for the transient behavior.
        /// </summary>
        /// <param name="method">The integration method.</param>
        public override void CreateStates(IntegrationMethod method)
        {
            Signals = new DelayedSignal(2, _bp.Delay);
        }

        /// <summary>
        /// Calculates the state values from the current DC solution.
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        /// <remarks>
        /// In this method, the initial value is calculated based on the operating point solution,
        /// and the result is stored in each respective <see cref="T:SpiceSharp.IntegrationMethods.StateDerivative" /> or <see cref="T:SpiceSharp.IntegrationMethods.StateHistory" />.
        /// </remarks>
        public override void GetDcState(TimeSimulation simulation)
        {
            var sol = simulation.RealState.Solution;

            // Calculate the inputs
            var input1 = sol[_pos2] - sol[_neg2] + _bp.Impedance * sol[_branchEq2];
            var input2 = sol[_pos1] - sol[_neg1] + _bp.Impedance * sol[_branchEq1];
            Signals.SetProbedValues(input1, input2);
        }

        /// <summary>
        /// Perform time-dependent calculations.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Transient(TimeSimulation simulation)
        {
            var sol = simulation.RealState.Solution;

            // Calculate inputs
            var input1 = sol[_pos2] - sol[_neg2] + _bp.Impedance * sol[_branchEq2];
            var input2 = sol[_pos1] - sol[_neg1] + _bp.Impedance * sol[_branchEq1];
            Signals.SetProbedValues(input1, input2);

            // Update the branch equations
            Ibr1Ptr.Value += Signals.Values[0];
            Ibr2Ptr.Value += Signals.Values[1];
        }
    }
}
