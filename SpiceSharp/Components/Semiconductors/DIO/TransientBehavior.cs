using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="Diode"/>
    /// </summary>
    public class TransientBehavior : BaseTransientBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private LoadBehavior _load;
        private TemperatureBehavior _temp;
        private ModelTemperatureBehavior _modeltemp;
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;

        /// <summary>
        /// Diode capacitance
        /// </summary>
        [ParameterName("cd"), ParameterInfo("Diode capacitance")]
        public double Capacitance { get; protected set; }
        [ParameterName("id"), ParameterName("c"), ParameterInfo("Diode current")]
        public double Current { get; protected set; }

        /// <summary>
        /// The charge on the junction capacitance
        /// </summary>
        public StateDerivative CapCharge { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode, _posPrimeNode;
        protected MatrixElement<double> PosPosPrimePtr { get; private set; }
        protected MatrixElement<double> NegPosPrimePtr { get; private set; }
        protected MatrixElement<double> PosPrimePosPtr { get; private set; }
        protected MatrixElement<double> PosPrimeNegPtr { get; private set; }
        protected MatrixElement<double> PosPosPtr { get; private set; }
        protected MatrixElement<double> NegNegPtr { get; private set; }
        protected MatrixElement<double> PosPrimePosPrimePtr { get; private set; }
        protected VectorElement<double> PosPrimePtr { get; private set; }
        protected VectorElement<double> NegPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>("entity");
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _load = provider.GetBehavior<LoadBehavior>("entity");
            _temp = provider.GetBehavior<TemperatureBehavior>("entity");
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
        }
        
        /// <summary>
        /// Unsetup the device
        /// </summary>
        public override void Unsetup()
        {
            PosPosPrimePtr = null;
            NegPosPrimePtr = null;
            PosPrimePosPtr = null;
            PosPrimeNegPtr = null;
            PosPosPtr = null;
            NegNegPtr = null;
            PosPrimePosPrimePtr = null;
        }

        /// <summary>
        /// Connect
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
        /// Gets equation pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<double> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get extra nodes
            _posPrimeNode = _load.PosPrimeNode;

            // Get matrix elements
            PosPosPrimePtr = solver.GetMatrixElement(_posNode, _posPrimeNode);
            NegPosPrimePtr = solver.GetMatrixElement(_negNode, _posPrimeNode);
            PosPrimePosPtr = solver.GetMatrixElement(_posPrimeNode, _posNode);
            PosPrimeNegPtr = solver.GetMatrixElement(_posPrimeNode, _negNode);
            PosPosPtr = solver.GetMatrixElement(_posNode, _posNode);
            NegNegPtr = solver.GetMatrixElement(_negNode, _negNode);
            PosPrimePosPrimePtr = solver.GetMatrixElement(_posPrimeNode, _posPrimeNode);

            // Get RHS elements
            PosPrimePtr = solver.GetRhsElement(_posPrimeNode);
            NegPtr = solver.GetRhsElement(_negNode);
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            CapCharge = states.CreateDerivative();
        }

        /// <summary>
        /// Calculate the state values
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public override void GetDcState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double capd;
            var vd = state.Solution[_posPrimeNode] - state.Solution[_negNode];

            // charge storage elements
            var czero = _temp.TempJunctionCap * _bp.Area;
            if (vd < _temp.TempDepletionCap)
            {
                var arg = 1 - vd / _mbp.JunctionPotential;
                var sarg = Math.Exp(-_mbp.GradingCoefficient * Math.Log(arg));
                CapCharge.Current = _mbp.TransitTime * _load.Current + _mbp.JunctionPotential * czero * (1 - arg * sarg) / (1 -
                        _mbp.GradingCoefficient);
                capd = _mbp.TransitTime * _load.Conduct + czero * sarg;
            }
            else
            {
                var czof2 = czero / _modeltemp.F2;
                CapCharge.Current = _mbp.TransitTime * _load.Current + czero * _temp.TempFactor1 + czof2 * (_modeltemp.F3 * (vd -
                    _temp.TempDepletionCap) + _mbp.GradingCoefficient / (_mbp.JunctionPotential + _mbp.JunctionPotential) * (vd * vd - _temp.TempDepletionCap * _temp.TempDepletionCap));
                capd = _mbp.TransitTime * _load.Conduct + czof2 * (_modeltemp.F3 + _mbp.GradingCoefficient * vd / _mbp.JunctionPotential);
            }
            Capacitance = capd;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            var vd = state.Solution[_posPrimeNode] - state.Solution[_negNode];

            // This is the same calculation
            GetDcState(simulation);

            // Integrate
            CapCharge.Integrate();
            var geq = CapCharge.Jacobian(Capacitance);
            var ceq = CapCharge.RhsCurrent(geq, vd);

            // Store the current
            Current = _load.Current + CapCharge.Derivative;

            // Load Rhs vector
            NegPtr.Value += ceq;
            PosPrimePtr.Value -= ceq;

            // Load Y-matrix
            NegNegPtr.Value += geq;
            PosPrimePosPrimePtr.Value += geq;
            NegPosPrimePtr.Value -= geq;
            PosPrimeNegPtr.Value -= geq;
        }

        /// <summary>
        /// Use local truncation error to cut timestep
        /// </summary>
        /// <returns>The timestep that satisfies the LTE</returns>
        public override double Truncate() => CapCharge.LocalTruncationError();
    }
}
