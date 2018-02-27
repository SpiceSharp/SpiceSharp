using System;
using System.Numerics;
using SpiceSharp.NewSparse;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Resistor"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("v"), PropertyInfo("Voltage")]
        public Complex GetVoltage(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return state.Solution[posNode] - state.Solution[negNode];
        }
        [PropertyName("i"), PropertyInfo("Current")]
        public Complex GetCurrent(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            var voltage = state.Solution[posNode] - state.Solution[negNode];
            return voltage * load.Conductance;
        }
        [PropertyName("p"), PropertyInfo("Power")]
        public Complex GetPower(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            var voltage = state.Solution[posNode] - state.Solution[negNode];
            return voltage * Complex.Conjugate(voltage) * load.Conductance;
        }

        /// <summary>
        /// Necessary behaviors
        /// </summary>
        LoadBehavior load;

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode;
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
            load = provider.GetBehavior<LoadBehavior>("entity");
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
            PosPosPtr = solver.GetMatrixElement(posNode, posNode);
            NegNegPtr = solver.GetMatrixElement(negNode, negNode);
            PosNegPtr = solver.GetMatrixElement(posNode, negNode);
            NegPosPtr = solver.GetMatrixElement(negNode, posNode);
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
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
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
            PosPosPtr.Value += load.Conductance;
            NegNegPtr.Value += load.Conductance;
            PosNegPtr.Value -= load.Conductance;
            NegPosPtr.Value -= load.Conductance;
        }
    }
}
