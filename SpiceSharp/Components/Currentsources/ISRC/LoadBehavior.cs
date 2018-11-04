using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CurrentSourceBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="CurrentSource"/>
    /// </summary>
    public class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private CommonBehaviors.IndependentBaseParameters _bp;

        /// <summary>
        /// Gets voltage across the voltage source
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [ParameterName("v"), ParameterInfo("Voltage accross the supply")]
        public double GetV(BaseSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return state.Solution[_posNode] - state.Solution[_negNode];
        }
        [ParameterName("p"), ParameterInfo("Power supplied by the source")]
        public double GetP(BaseSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return (state.Solution[_posNode] - state.Solution[_posNode]) * -Current;
        }
        [ParameterName("c"), ParameterName("i"), ParameterInfo("Current through current source")]
        public double Current { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode;
        private VectorElement<double> _posPtr, _negPtr;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<CommonBehaviors.IndependentBaseParameters>();

            // Setup the waveform
            _bp.Waveform?.Setup();

            // Give some warnings if no value is given
            if (!_bp.DcValue.Given)
            {
                // no DC value - either have a transient value or none
                CircuitWarning.Warning(this,
                    _bp.Waveform != null
                        ? "{0} has no DC value, transient time 0 value used".FormatString(Name)
                        : "{0} has no value, DC 0 assumed".FormatString(Name));
            }
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
        /// Get the matrix elements
        /// </summary>
        /// <param name="variables">Variables</param>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            _posPtr = solver.GetRhsElement(_posNode);
            _negPtr = solver.GetRhsElement(_negNode);
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double value;

            // Time domain analysis
            if (simulation is TimeSimulation ts)
            {
                var time = ts.Method.Time;

                // Use the waveform if possible
                if (_bp.Waveform != null)
                    value = _bp.Waveform.Value;
                else
                    value = _bp.DcValue * state.SourceFactor;
            }
            else
            {
                // AC or DC analysis use the DC value
                value = _bp.DcValue * state.SourceFactor;
            }

            // NOTE: Spice 3f5's documentation is IXXXX POS NEG VALUE but in the code it is IXXXX NEG POS VALUE
            // I solved it by inverting the current when loading the rhs vector
            _posPtr.Value -= value;
            _negPtr.Value += value;
            Current = value;
        }
    }
}
