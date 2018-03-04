using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Algebra;
using System;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.CurrentsourceBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="CurrentSource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private BaseParameters _bp;

        /// <summary>
        /// Gets voltage across the voltage source
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [PropertyName("v"), PropertyInfo("Voltage accross the supply")]
        public double GetV(RealState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return (state.Solution[_posNode] - state.Solution[_negNode]);
        }
        [PropertyName("p"), PropertyInfo("Power supplied by the source")]
        public double GetP(RealState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return (state.Solution[_posNode] - state.Solution[_posNode]) * -Current;
        }
        [PropertyName("c"), PropertyName("i"), PropertyInfo("Current through current source")]
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
        public LoadBehavior(Identifier name) : base(name) { }
        
        /// <summary>
        /// Create an export method
        /// </summary>
        /// <param name="propertyName">Parameter name</param>
        /// <returns></returns>
        public override Func<RealState, double> CreateExport(string propertyName)
        {
            // Avoid using reflection for common components
            switch (propertyName)
            {
                case "v": return GetV;
                case "p": return GetP;
                case "i":
                case "c": return (RealState state) => Current;
                default: return null;
            }
        }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>("entity");

            // Give some warnings if no value is given
            if (!_bp.DcValue.Given)
            {
                // no DC value - either have a transient value or none
                if (_bp.Waveform != null)
                    CircuitWarning.Warning(this, "{0} has no DC value, transient time 0 value used".FormatString(Name));
                else
                    CircuitWarning.Warning(this, "{0} has no value, DC 0 assumed".FormatString(Name));
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
        /// <param name="nodes"></param>
        /// <param name="solver"></param>
        public override void GetEquationPointers(Nodes nodes, Solver<double> solver)
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

            double value = 0.0;
            double time = 0.0;

            // Time domain analysis
            if (state.Domain == RealState.DomainType.Time)
            {
                if (simulation is TimeSimulation tsim)
                    time = tsim.Method.Time;

                // Use the waveform if possible
                if (_bp.Waveform != null)
                    value = _bp.Waveform.At(time);
                else
                    value = _bp.DcValue * state.SourceFactor;
            }
            else
            {
                // AC or DC analysis use the DC value
                value = _bp.DcValue * state.SourceFactor;
            }

            _posPtr.Value += value;
            _negPtr.Value -= value;
            Current = value;
        }
    }
}
