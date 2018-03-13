using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Resistor"/>
    /// </summary>
    public class FrequencyBehavior : BaseFrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private TemperatureBehavior _temp;

        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("v"), PropertyInfo("Voltage")]
        public Complex GetVoltage(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));
            return state.Solution[_posNode] - state.Solution[_negNode];
        }
        [PropertyName("i"), PropertyInfo("Current")]
        public Complex GetCurrent(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            var voltage = state.Solution[_posNode] - state.Solution[_negNode];
            return voltage * _temp.Conductance;
        }
        [PropertyName("p"), PropertyInfo("Power")]
        public Complex GetPower(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            var voltage = state.Solution[_posNode] - state.Solution[_negNode];
            return voltage * Complex.Conjugate(voltage) * _temp.Conductance;
        }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode;
        protected MatrixElement<Complex> PosPosPtr { get; private set; }
        protected MatrixElement<Complex> NegNegPtr { get; private set; }
        protected MatrixElement<Complex> PosNegPtr { get; private set; }
        protected MatrixElement<Complex> NegPosPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>("entity");
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Matrix</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            // Get matrix pointers
            PosPosPtr = solver.GetMatrixElement(_posNode, _posNode);
            NegNegPtr = solver.GetMatrixElement(_negNode, _negNode);
            PosNegPtr = solver.GetMatrixElement(_posNode, _negNode);
            NegPosPtr = solver.GetMatrixElement(_negNode, _posNode);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            PosPosPtr = null;
            NegNegPtr = null;
            PosNegPtr = null;
            NegPosPtr = null;
        }

        /// <summary>
        /// Connect behavior
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
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // Load Y-matrix
            double conductance = _temp.Conductance;
            PosPosPtr.Value += conductance;
            NegNegPtr.Value += conductance;
            PosNegPtr.Value -= conductance;
            NegPosPtr.Value -= conductance;
        }
    }
}
