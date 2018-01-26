using System;
using SpiceSharp.Sparse;
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
        int posNode, negNode, contBranch;
        protected MatrixElement PosPosPtr { get; private set; }
        protected MatrixElement NegPosPtr { get; private set; }
        protected MatrixElement PosNegPtr { get; private set; }
        protected MatrixElement NegNegPtr { get; private set; }

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
            load = provider.GetBehavior<LoadBehavior>(0);
            modelload = provider.GetBehavior<ModelLoadBehavior>(1);
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
                throw new Diagnostics.CircuitException($"Pin count mismatch: 2 pins expected, {pins.Length} given");
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            contBranch = load.ControllingBranch;
            PosPosPtr = matrix.GetElement(posNode, posNode);
            PosNegPtr = matrix.GetElement(posNode, negNode);
            NegPosPtr = matrix.GetElement(negNode, posNode);
            NegNegPtr = matrix.GetElement(negNode, negNode);
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
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            bool current_state;
            double g_now;
            var state = sim.State;
            var cstate = state;

            // Get the current state
            current_state = load.CurrentState;
            g_now = current_state != false ? modelload.OnConductance : modelload.OffConductance;

            // Load the Y-matrix
            PosPosPtr.Add(g_now);
            PosNegPtr.Sub(g_now);
            NegPosPtr.Sub(g_now);
            NegNegPtr.Add(g_now);
        }
    }
}
