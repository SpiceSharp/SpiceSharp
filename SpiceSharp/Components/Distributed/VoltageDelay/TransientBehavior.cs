using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Distributed;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DelayBehaviors
{
    public class TransientBehavior : BaseTransientBehavior, IConnectedBehavior
    {
        // Necessary behaviors and parameters
        private BaseParameters _bp;
        private LoadBehavior _load;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _input, _branch;
        protected VectorElement<double> BranchPtr { get; private set; }

        /// <summary>
        /// Gets the delayed signal.
        /// </summary>
        /// <value>
        /// The delayed signal.
        /// </value>
        public Delayed DelayedSignal { get; private set; }

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
            _input = pins[0];
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            _bp = provider.GetParameterSet<BaseParameters>();
            _load = provider.GetBehavior<LoadBehavior>();
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
        }

        /// <summary>
        /// Creates all necessary states for the transient behavior.
        /// </summary>
        /// <param name="method">The integration method.</param>
        public override void CreateStates(IntegrationMethod method)
        {
            DelayedSignal = new Delayed(1, _bp.Delay);
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
            var input = simulation.RealState.Solution[_input];
            DelayedSignal.SetProbedValues(input);
        }

        /// <summary>
        /// Perform time-dependent calculations.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Transient(TimeSimulation simulation)
        {
            // Add the RHS value
            var input = simulation.RealState.Solution[_input];
            DelayedSignal.SetProbedValues(input);
            BranchPtr.Value += DelayedSignal.Values[0];
        }
    }
}
