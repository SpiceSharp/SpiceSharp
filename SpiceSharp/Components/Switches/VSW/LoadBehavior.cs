using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.VSW
{
    /// <summary>
    /// Load behavior for a <see cref="VoltageSwitch"/>
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
        [SpiceName("on"), SpiceInfo("Switch initially closed")]
        public void SetOn() { VSWzero_state = true; }
        [SpiceName("off"), SpiceInfo("Switch initially open")]
        public void SetOff() { VSWzero_state = false; }

        protected bool VSWzero_state = false;
        public int VSWstate { get; internal set; }
        public double VSWcond { get; internal set; }

        /// <summary>
        /// Nodes
        /// </summary>
        protected int VSWposNode, VSWnegNode, VSWcontPosNode, VSWcontNegNode;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement SWposPosptr { get; private set; }
        protected MatrixElement SWnegPosptr { get; private set; }
        protected MatrixElement SWposNegptr { get; private set; }
        protected MatrixElement SWnegNegptr { get; private set; }
        
        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var vsw = component as VoltageSwitch;

            // Get behaviors
            modelload = GetBehavior<ModelLoadBehavior>(vsw.Model);

            // Get nodes
            VSWposNode = vsw.VSWposNode;
            VSWnegNode = vsw.VSWnegNode;
            VSWcontPosNode = vsw.VSWcontPosNode;
            VSWcontNegNode = vsw.VSWcontNegNode;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            SWposPosptr = matrix.GetElement(VSWposNode, VSWposNode);
            SWposNegptr = matrix.GetElement(VSWposNode, VSWnegNode);
            SWnegPosptr = matrix.GetElement(VSWnegNode, VSWposNode);
            SWnegNegptr = matrix.GetElement(VSWnegNode, VSWnegNode);

            VSWstate = ckt.State.GetState();
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            SWposPosptr = null;
            SWposNegptr = null;
            SWnegPosptr = null;
            SWnegNegptr = null;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            double g_now;
            double v_ctrl;
            double previous_state;
            double current_state = 0.0;
            var state = ckt.State;
            var rstate = state;

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
                v_ctrl = rstate.Solution[VSWcontPosNode] - rstate.Solution[VSWcontNegNode];

                // Calculate the current state
                if (v_ctrl > (modelload.VSWthresh + modelload.VSWhyst))
                    current_state = 1.0;
                else if (v_ctrl < (modelload.VSWthresh - modelload.VSWhyst))
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
                v_ctrl = rstate.Solution[VSWcontPosNode] - rstate.Solution[VSWcontNegNode];

                if (v_ctrl > (modelload.VSWthresh + modelload.VSWhyst))
                    current_state = 1.0;
                else if (v_ctrl < (modelload.VSWthresh - modelload.VSWhyst))
                    current_state = 0.0;
                else
                    current_state = previous_state;

                // Store the state
                if (current_state == 0.0)
                    state.States[0][VSWstate] = 0.0;
                else
                    state.States[0][VSWstate] = 1.0;
            }

            g_now = current_state > 0.0 ? modelload.VSWonConduct : modelload.VSWoffConduct;
            VSWcond = g_now;

            // Load the Y-matrix
            SWposPosptr.Add(g_now);
            SWposNegptr.Sub(g_now);
            SWnegPosptr.Sub(g_now);
            SWnegNegptr.Add(g_now);
        }

    }
}
