using System;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.VoltageSwitchBehaviors
{
    /// <summary>
    /// Load behavior for a <see cref="VoltageSwitch"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelLoadBehavior modelload;
        ModelBaseParameters mbp;

        /// <summary>
        /// Gets or sets the previous state
        /// </summary>
        public bool OldState { get; set; } = false;

        /// <summary>
        /// Flag for using the previous state or not
        /// </summary>
        public bool UseOldState { get; set; } = false;

        /// <summary>
        /// The current state
        /// </summary>
        public bool CurrentState { get; protected set; } = false;

        /// <summary>
        /// The current conductance
        /// </summary>
        public double Cond { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode, contPosourceNode, contNegateNode;
        protected ElementValue PosPosPtr { get; private set; }
        protected ElementValue NegPosPtr { get; private set; }
        protected ElementValue PosNegPtr { get; private set; }
        protected ElementValue NegNegPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);

            // Get behaviors
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
            if (pins.Length != 4)
                throw new Diagnostics.CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
            contPosourceNode = pins[2];
            contNegateNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="matrix"></param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            PosPosPtr = matrix.GetElement(posNode, posNode);
            PosNegPtr = matrix.GetElement(posNode, negNode);
            NegPosPtr = matrix.GetElement(negNode, posNode);
            NegNegPtr = matrix.GetElement(negNode, negNode);
        }
        
        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            PosPosPtr = null;
            PosNegPtr = null;
            NegPosPtr = null;
            NegNegPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            double g_now;
            double v_ctrl;
            bool previous_state;
            bool current_state = false;
            var state = simulation.RealState;

            if (state.Init == RealState.InitializationStates.InitFix || state.Init == RealState.InitializationStates.InitJunction)
            {
                if (bp.ZeroState)
                {
                    // Switch specified "on"
                    CurrentState = true;
                    current_state = true;
                }
                else
                {
                    // Switch specified "off"
                    CurrentState = false;
                    current_state = false;
                }
            }
            else
            {
                if (UseOldState)
                    previous_state = OldState;
                else
                    previous_state = CurrentState;
                v_ctrl = state.Solution[contPosourceNode] - state.Solution[contNegateNode];

                // Calculate the current state
                if (v_ctrl > (mbp.Threshold + mbp.Hysteresis))
                    current_state = true;
                else if (v_ctrl < (mbp.Threshold - mbp.Hysteresis))
                    current_state = false;
                else
                    current_state = previous_state;

                // Store the current state
                CurrentState = current_state;

                // If the state changed, ensure one more iteration
                if (current_state != previous_state)
                    state.IsConvergent = false;
            }

            g_now = current_state == true ? modelload.OnConductance : modelload.OffConductance;
            Cond = g_now;

            // Load the Y-matrix
            PosPosPtr.Add(g_now);
            PosNegPtr.Sub(g_now);
            NegPosPtr.Sub(g_now);
            NegNegPtr.Add(g_now);
        }

    }
}
