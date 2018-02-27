using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.CurrentSwitchBehaviors
{
    /// <summary>
    /// AC behavior for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        LoadBehavior load;
        ModelLoadBehavior modelload;

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode;
        protected MatrixElement<Complex> PosPosPtr { get; private set; }
        protected MatrixElement<Complex> NegPosPtr { get; private set; }
        protected MatrixElement<Complex> PosNegPtr { get; private set; }
        protected MatrixElement<Complex> NegNegPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>("entity");
            modelload = provider.GetBehavior<ModelLoadBehavior>("model");
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
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            PosPosPtr = solver.GetMatrixElement(posNode, posNode);
            PosNegPtr = solver.GetMatrixElement(posNode, negNode);
            NegPosPtr = solver.GetMatrixElement(negNode, posNode);
            NegNegPtr = solver.GetMatrixElement(negNode, negNode);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            PosPosPtr = null;
            NegNegPtr = null;
            PosNegPtr = null;
            NegPosPtr = null;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            bool current_state;
            double g_now;

            // Get the current state
            current_state = load.CurrentState;
            g_now = current_state != false ? modelload.OnConductance : modelload.OffConductance;

            // Load the Y-matrix
            PosPosPtr.Value += g_now;
            PosNegPtr.Value -= g_now;
            NegPosPtr.Value -= g_now;
            NegNegPtr.Value += g_now;
        }
    }
}
