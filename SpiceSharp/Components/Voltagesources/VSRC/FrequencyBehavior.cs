using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageSourceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="VoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : BaseFrequencyBehavior, IConnectedBehavior
    {
        // Necessary behaviors and parameters
        private CommonBehaviors.IndependentFrequencyParameters _ap;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode, _branchEq;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement<Complex> PosBranchPtr { get; private set; }
        protected MatrixElement<Complex> NegBranchPtr { get; private set; }
        protected MatrixElement<Complex> BranchPosPtr { get; private set; }
        protected MatrixElement<Complex> BranchNegPtr { get; private set; }
        protected VectorElement<Complex> BranchPtr { get; private set; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex Voltage => _ap.Phasor;
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Complex current")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));
            return state.Solution[_branchEq];
        }
        [ParameterName("p"), ParameterInfo("Complex power")]
        public Complex GetPower(ComplexSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            var v = state.Solution[_posNode] - state.Solution[_negNode];
            var i = state.Solution[_branchEq];
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _ap = provider.GetParameterSet<CommonBehaviors.IndependentFrequencyParameters>();
            
            // Get behaviors
            var load = provider.GetBehavior<LoadBehavior>();
            _branchEq = load.BranchEq;
        }
        
        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            _posNode = pins[0];
            _negNode = pins[1];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get matrix elements
            PosBranchPtr = solver.GetMatrixElement(_posNode, _branchEq);
            BranchPosPtr = solver.GetMatrixElement(_branchEq, _posNode);
            NegBranchPtr = solver.GetMatrixElement(_negNode, _branchEq);
            BranchNegPtr = solver.GetMatrixElement(_branchEq, _negNode);

            // Get rhs elements
            BranchPtr = solver.GetRhsElement(_branchEq);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        /// <param name="simulation"></param>
        public override void Unsetup(Simulation simulation)
        {
            PosBranchPtr = null;
            BranchPosPtr = null;
            NegBranchPtr = null;
            BranchNegPtr = null;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // Load Y-matrix
            PosBranchPtr.Value += 1.0;
            BranchPosPtr.Value += 1.0;
            NegBranchPtr.Value -= 1.0;
            BranchNegPtr.Value -= 1.0;

            // Load Rhs-vector
            BranchPtr.Value += _ap.Phasor;
        }
    }
}
