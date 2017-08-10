using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a current-controlled switch
    /// </summary>
    public class CurrentSwitch : CircuitComponent
    {
        /// <summary>
        /// Gets or sets the model for the current-controlled switch
        /// </summary>
        public CurrentSwitchModel Model { get; set; }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("on"), SpiceInfo("Initially closed")]
        public void SetOn() { CSWzero_state = true; }
        [SpiceName("off"), SpiceInfo("Initially open")]
        public void SetOff() { CSWzero_state = false; }
        [SpiceName("control"), SpiceInfo("Name of the controlling source")]
        public string CSWcontName { get; set; }
        [SpiceName("i"), SpiceInfo("Switch current")]
        public double GetCurrent(Circuit ckt) => (ckt.State.Real.Solution[CSWposNode] - ckt.State.Real.Solution[CSWnegNode]) * CSWcond;
        [SpiceName("p"), SpiceInfo("Instantaneous power")]
        public double GetPower(Circuit ckt) => (ckt.State.Real.Solution[CSWposNode] - ckt.State.Real.Solution[CSWnegNode]) *
            (ckt.State.Real.Solution[CSWposNode] - ckt.State.Real.Solution[CSWnegNode]) * CSWcond;
        public double CSWcond { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node"), SpiceInfo("Positive node of the switch")]
        public int CSWposNode { get; private set; }
        [SpiceName("neg_node"), SpiceInfo("Negative node of the switch")]
        public int CSWnegNode { get; private set; }
        private int CSWcontBranch;
        public int CSWstate { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private bool CSWzero_state = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled switch</param>
        public CurrentSwitch(string name) : base(name, "W+", "W-")
        {
            // Make sure the current switch is processed after voltage sources
            Priority = -1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="vsource">The controlling voltage source</param>
        public CurrentSwitch(string name, string pos, string neg, string vsource) : base(name, "W+", "W-")
        {
            Connect(pos, neg);
            CSWcontName = vsource;
            Priority = -1;
        }

        /// <summary>
        /// Get the model for the current-controlled switch
        /// </summary>
        /// <returns></returns>
        public override CircuitModel GetModel() => Model;

        /// <summary>
        /// Setup the current-controlled switch
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            CSWposNode = nodes[0].Index;
            CSWnegNode = nodes[1].Index;

            // Find the voltage source
            var vsource = ckt.Components[CSWcontName];
            if (vsource is Voltagesource)
                CSWcontBranch = ((Voltagesource)vsource).VSRCbranch;

            CSWstate = ckt.State.GetState();
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt) { }

        /// <summary>
        /// Load the current-controlled switch
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            double g_now;
            double i_ctrl;
            double previous_state;
            double current_state = 0.0;
            var state = ckt.State;
            var rstate = state.Real;

            // decide the state of the switch
            if (state.Init == CircuitState.InitFlags.InitFix || state.Init == CircuitState.InitFlags.InitJct)
            {
                if (CSWzero_state)
                {
                    // Switch specified "on"
                    state.States[0][CSWstate] = 1.0;
                    current_state = 1.0;
                }
                else
                {
                    // Switch specified "off"
                    state.States[0][CSWstate] = 0.0;
                    current_state = 0.0;
                }
            }
            else if (state.UseSmallSignal)
            {
                previous_state = state.States[0][CSWstate];
                current_state = previous_state;
            }
            else if (state.UseDC)
            {
                // No time-dependence, so use current state instead
                previous_state = state.States[0][CSWstate];
                i_ctrl = rstate.OldSolution[CSWcontBranch];
                if (i_ctrl > (Model.CSWthresh + Model.CSWhyst))
                    current_state = 1.0;
                else if (i_ctrl < (Model.CSWthresh - Model.CSWhyst))
                    current_state = 0.0;
                else
                    current_state = previous_state;

                // Store the current state
                if (current_state == 0)
                    state.States[0][CSWstate] = 0.0;
                else
                    state.States[0][CSWstate] = 1.0;

                // Ensure one more iteration
                if (current_state != previous_state)
                    state.IsCon = false;
            }
            else
            {
                // Get the previous state
                previous_state = state.States[1][CSWstate];
                i_ctrl = rstate.OldSolution[CSWcontBranch];

                // Calculate the current state
                if (i_ctrl > (Model.CSWthresh + Model.CSWhyst))
                    current_state = 1;
                else if (i_ctrl < (Model.CSWthresh - Model.CSWhyst))
                    current_state = 0;
                else
                    current_state = previous_state;

                // Store the current state
                if (current_state == 0)
                    state.States[0][CSWstate] = 0.0;
                else
                    state.States[0][CSWstate] = 1.0;
            }

            // Get the current conduction
            g_now = current_state != 0.0 ? (Model.CSWonConduct) : (Model.CSWoffConduct);
            CSWcond = g_now;

            // Load the Y-matrix
            rstate.Matrix[CSWposNode, CSWposNode] += g_now;
            rstate.Matrix[CSWposNode, CSWnegNode] -= g_now;
            rstate.Matrix[CSWnegNode, CSWposNode] -= g_now;
            rstate.Matrix[CSWnegNode, CSWnegNode] += g_now;
        }

        /// <summary>
        /// Load the current-controlled switch for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            double current_state;
            double g_now;
            var state = ckt.State;
            var cstate = state.Complex;

            // Get the current state
            current_state = state.States[0][CSWstate];
            g_now = current_state > 0.0 ? Model.CSWonConduct : Model.CSWoffConduct;

            // Load the Y-matrix
            cstate.Matrix[CSWposNode, CSWposNode] += g_now;
            cstate.Matrix[CSWposNode, CSWnegNode] -= g_now;
            cstate.Matrix[CSWnegNode, CSWposNode] -= g_now;
            cstate.Matrix[CSWnegNode, CSWnegNode] += g_now;
        }
    }
}
