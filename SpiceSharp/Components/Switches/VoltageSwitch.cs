using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a voltage-controlled switch
    /// </summary>
    [SpicePins("S+", "S-", "SC+", "SC-"), ConnectedPins(0, 1)]
    public class VoltageSwitch : CircuitComponent<VoltageSwitch>
    {
        /// <summary>
        /// Gets or sets the model
        /// </summary>
        public void SetModel(VoltageSwitchModel model) => Model = (ICircuitObject)model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("on"), SpiceInfo("Switch initially closed")]
        public void SetOn() { VSWzero_state = true; }
        [SpiceName("off"), SpiceInfo("Switch initially open")]
        public void SetOff() { VSWzero_state = false; }

        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node"), SpiceInfo("Positive node of the switch")]
        public int VSWposNode { get; private set; }
        [SpiceName("neg_node"), SpiceInfo("Negative node of the switch")]
        public int VSWnegNode { get; private set; }
        [SpiceName("cont_p_node"), SpiceInfo("Positive controlling node of the switch")]
        public int VSWcontPosNode { get; private set; }
        [SpiceName("cont_n_node"), SpiceInfo("Negative controlling node of the switch")]
        public int VSWcontNegNode { get; private set; }
        private bool VSWzero_state = false;
        public int VSWstate { get; private set; }
        public double VSWcond { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        public VoltageSwitch(string name) : base(name) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cont_pos">The positive controlling node</param>
        /// <param name="cont_neg">The negative controlling node</param>
        public VoltageSwitch(string name, string pos, string neg, string cont_pos, string cont_neg) : base(name)
        {
            Connect(pos, neg, cont_pos, cont_neg);
        }

        /// <summary>
        /// Setup the voltage-controlled switch
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            VSWposNode = nodes[0].Index;
            VSWnegNode = nodes[1].Index;
            VSWcontPosNode = nodes[2].Index;
            VSWcontNegNode = nodes[3].Index;

            VSWstate = ckt.State.GetState();
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
        }

        /// <summary>
        /// Load the voltage-controlled switch
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var model = Model as VoltageSwitchModel;
            double g_now;
            double v_ctrl;
            double previous_state;
            double current_state = 0.0;
            var state = ckt.State;
            var rstate = state.Real;

            if (state.Init == CircuitState.InitFlags.InitFix || state.Init == CircuitState.InitFlags.InitJct)
            {
                if (VSWzero_state)
                {
                    // Switch specified "on"
                    state.States[0][VSWstate] = 1.0;
                    current_state = 1.0;
                }
                else
                {
                    // Switch specified "off"
                    state.States[0][VSWstate] = 0.0;
                    current_state = 0.0;
                }
            }
            else if (state.UseSmallSignal)
            {
                previous_state = state.States[0][VSWstate];
                current_state = previous_state;
            }
            else if (state.UseDC)
            {
                // Time-independent calculations: use current state
                previous_state = state.States[0][VSWstate];
                v_ctrl = rstate.OldSolution[VSWcontPosNode] - rstate.OldSolution[VSWcontNegNode];

                // Calculate the current state
                if (v_ctrl > (model.VSWthresh + model.VSWhyst))
                    current_state = 1.0;
                else if (v_ctrl < (model.VSWthresh - model.VSWhyst))
                    current_state = 0.0;
                else
                    current_state = previous_state;

                // Store the current state
                if (current_state == 0.0)
                    state.States[0][VSWstate] = 0.0;
                else
                    state.States[0][VSWstate] = 1.0;

                // If the state changed, ensure one more iteration
                if (current_state != previous_state)
                    state.IsCon = false;
            }
            else
            {
                // Get the previous state
                previous_state = state.States[1][VSWstate];
                v_ctrl = rstate.OldSolution[VSWcontPosNode] - rstate.OldSolution[VSWcontNegNode];

                if (v_ctrl > (model.VSWthresh + model.VSWhyst))
                    current_state = 1.0;
                else if (v_ctrl < (model.VSWthresh - model.VSWhyst))
                    current_state = 0.0;
                else
                    current_state = previous_state;

                // Store the state
                if (current_state == 0.0)
                    state.States[0][VSWstate] = 0.0;
                else
                    state.States[0][VSWstate] = 1.0;
            }

            g_now = current_state > 0.0 ? model.VSWonConduct : model.VSWoffConduct;
            VSWcond = g_now;

            // Load the Y-matrix
            rstate.Matrix[VSWposNode, VSWposNode] += g_now;
            rstate.Matrix[VSWposNode, VSWnegNode] -= g_now;
            rstate.Matrix[VSWnegNode, VSWposNode] -= g_now;
            rstate.Matrix[VSWnegNode, VSWnegNode] += g_now;
        }

        /// <summary>
        /// Load the voltage-controlled switch for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var model = Model as VoltageSwitchModel;
            double current_state, g_now;
            var state = ckt.State;
            var cstate = state.Complex;

            // Get the current state
            current_state = state.States[0][VSWstate];
            g_now = current_state > 0.0 ? model.VSWonConduct : model.VSWoffConduct;

            // Load the Y-matrix
            cstate.Matrix[VSWposNode, VSWposNode] += g_now;
            cstate.Matrix[VSWposNode, VSWnegNode] -= g_now;
            cstate.Matrix[VSWnegNode, VSWposNode] -= g_now;
            cstate.Matrix[VSWnegNode, VSWnegNode] += g_now;
        }
    }
}
