using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.CSW
{
    /// <summary>
    /// General behavior for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private ModelLoadBehavior modelload;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("on"), SpiceInfo("Initially closed")]
        public void SetOn() { CSWzero_state = true; }
        [SpiceName("off"), SpiceInfo("Initially open")]
        public void SetOff() { CSWzero_state = false; }
        [SpiceName("i"), SpiceInfo("Switch current")]
        public double GetCurrent(Circuit ckt) => (ckt.State.Solution[CSWposNode] - ckt.State.Solution[CSWnegNode]) * CSWcond;
        [SpiceName("p"), SpiceInfo("Instantaneous power")]
        public double GetPower(Circuit ckt) => (ckt.State.Solution[CSWposNode] - ckt.State.Solution[CSWnegNode]) *
            (ckt.State.Solution[CSWposNode] - ckt.State.Solution[CSWnegNode]) * CSWcond;
        public double CSWcond { get; internal set; }

        /// <summary>
        /// Nodes
        /// </summary>
        protected int CSWposNode, CSWnegNode, CSWcontBranch;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement CSWposPosptr { get; private set; }
        protected MatrixElement CSWnegPosptr { get; private set; }
        protected MatrixElement CSWposNegptr { get; private set; }
        protected MatrixElement CSWnegNegptr { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public bool CSWzero_state { get; protected set; } = false;
        public int CSWstate { get; protected set; }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(Entity component, Circuit ckt)
        {
            var csw = component as CurrentSwitch;

            // Get behaviors
            modelload = GetBehavior<ModelLoadBehavior>(csw.Model);
            var vsrcload = GetBehavior<VSRC.LoadBehavior>(csw.CSWcontSource);

            // Nodes
            CSWposNode = csw.CSWposNode;
            CSWnegNode = csw.CSWnegNode;
            CSWcontBranch = vsrcload.VSRCbranch;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            CSWposPosptr = matrix.GetElement(CSWposNode, CSWposNode);
            CSWposNegptr = matrix.GetElement(CSWposNode, CSWnegNode);
            CSWnegPosptr = matrix.GetElement(CSWnegNode, CSWposNode);
            CSWnegNegptr = matrix.GetElement(CSWnegNode, CSWnegNode);

            // Get states
            CSWstate = ckt.State.GetState();
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            CSWposPosptr = null;
            CSWposNegptr = null;
            CSWnegPosptr = null;
            CSWnegNegptr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            double g_now;
            double i_ctrl;
            double previous_state;
            double current_state = 0.0;
            var state = ckt.State;
            var rstate = state;

            // decide the state of the switch
            if (state.Init == State.InitFlags.InitFix || state.Init == State.InitFlags.InitJct)
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
                i_ctrl = rstate.Solution[CSWcontBranch];
                if (i_ctrl > (modelload.CSWthresh + modelload.CSWhyst))
                    current_state = 1.0;
                else if (i_ctrl < (modelload.CSWthresh - modelload.CSWhyst))
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
                i_ctrl = rstate.Solution[CSWcontBranch];

                // Calculate the current state
                if (i_ctrl > (modelload.CSWthresh + modelload.CSWhyst))
                    current_state = 1;
                else if (i_ctrl < (modelload.CSWthresh - modelload.CSWhyst))
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
            g_now = current_state != 0.0 ? (modelload.CSWonConduct) : (modelload.CSWoffConduct);
            CSWcond = g_now;

            // Load the Y-matrix
            CSWposPosptr.Add(g_now);
            CSWposNegptr.Sub(g_now);
            CSWnegPosptr.Sub(g_now);
            CSWnegNegptr.Add(g_now);
        }
    }
}
