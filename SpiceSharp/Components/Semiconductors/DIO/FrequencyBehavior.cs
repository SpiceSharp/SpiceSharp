using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Diode"/>
    /// </summary>
    public class FrequencyBehavior : BaseFrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private LoadBehavior _load;
        private TemperatureBehavior _temp;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode, _posPrimeNode;
        protected MatrixElement<Complex> PosPosPrimePtr { get; private set; }
        protected MatrixElement<Complex> NegPosPrimePtr { get; private set; }
        protected MatrixElement<Complex> PosPrimePosPtr { get; private set; }
        protected MatrixElement<Complex> PosPrimeNegPtr { get; private set; }
        protected MatrixElement<Complex> PosPosPtr { get; private set; }
        protected MatrixElement<Complex> NegNegPtr { get; private set; }
        protected MatrixElement<Complex> PosPrimePosPrimePtr { get; private set; }

        /// <summary>
        /// Gets the junction capacitance
        /// </summary>
        [ParameterName("cd"), ParameterInfo("Diode capacitance")]
        public double Capacitance { get; protected set; }
        [ParameterName("vd"), ParameterInfo("Voltage across the internal diode")]
        public Complex GetDiodeVoltage(ComplexState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return state.Solution[_posPrimeNode] - state.Solution[_negNode];
        }
        [ParameterName("v"), ParameterInfo("Voltage across the diode")]
        public Complex GetVoltage(ComplexState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return state.Solution[_posNode] - state.Solution[_negNode];
        }
        [ParameterName("i"), ParameterName("id"), ParameterInfo("Current through the diode")]
        public Complex GetCurrent(ComplexState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            var geq = Capacitance * state.Laplace + _load.Conduct;
            var voltage = state.Solution[_posPrimeNode] - state.Solution[_negNode];
            return voltage * geq;
        }
        [ParameterName("p"), ParameterName("pd"), ParameterInfo("Power")]
        public Complex GetPower(ComplexState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            var geq = Capacitance * state.Laplace + _load.Conduct;
            var current = (state.Solution[_posPrimeNode] - state.Solution[_negNode]) * geq;
            var voltage = state.Solution[_posNode] - state.Solution[_negNode];
            return voltage * -Complex.Conjugate(current);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

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
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _load = provider.GetBehavior<LoadBehavior>();
            _temp = provider.GetBehavior<TemperatureBehavior>();
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
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
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get node
            _posPrimeNode = _load.PosPrimeNode;

            // Get matrix pointers
            PosPosPrimePtr = solver.GetMatrixElement(_posNode, _posPrimeNode);
            NegPosPrimePtr = solver.GetMatrixElement(_negNode, _posPrimeNode);
            PosPrimePosPtr = solver.GetMatrixElement(_posPrimeNode, _posNode);
            PosPrimeNegPtr = solver.GetMatrixElement(_posPrimeNode, _negNode);
            PosPosPtr = solver.GetMatrixElement(_posNode, _posNode);
            NegNegPtr = solver.GetMatrixElement(_negNode, _negNode);
            PosPrimePosPrimePtr = solver.GetMatrixElement(_posPrimeNode, _posPrimeNode);
        }

        /// <summary>
        /// Unsetup the device
        /// </summary>
        /// <param name="simulation"></param>
        public override void Unsetup(Simulation simulation)
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
        /// Calculate AC parameters
        /// </summary>
        /// <param name="simulation"></param>
        public override void InitializeParameters(FrequencySimulation simulation)
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
                capd = _mbp.TransitTime * _load.Conduct + czero * sarg;
            }
            else
            {
                var czof2 = czero / _modeltemp.F2;
                capd = _mbp.TransitTime * _load.Conduct + czof2 * (_modeltemp.F3 + _mbp.GradingCoefficient * vd / _mbp.JunctionPotential);
            }
            Capacitance = capd;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.ComplexState;

            var gspr = _modeltemp.Conductance * _bp.Area;
            var geq = _load.Conduct;
            var xceq = Capacitance * state.Laplace.Imaginary;

            // Load Y-matrix
            PosPosPtr.Value += gspr;
            NegNegPtr.Value += new Complex(geq, xceq);
            PosPrimePosPrimePtr.Value += new Complex(geq + gspr, xceq);
            PosPosPrimePtr.Value -= gspr;
            NegPosPrimePtr.Value -= new Complex(geq, xceq);
            PosPrimePosPtr.Value -= gspr;
            PosPrimeNegPtr.Value -= new Complex(geq, xceq);
        }
    }
}
