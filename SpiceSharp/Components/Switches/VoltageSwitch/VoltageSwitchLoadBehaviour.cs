using SpiceSharp.Circuits;
using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    public class VoltageSwitchLoadBehaviour : CircuitObjectBehaviourLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var vsw = ComponentTyped<VoltageSwitch>();
            var model = vsw.Model as VoltageSwitchModel;
            double g_now;
            double v_ctrl;
            double previous_state;
            double current_state = 0.0;
            var state = ckt.State;
            var rstate = state.Real;

            if (state.Init == CircuitState.InitFlags.InitFix || state.Init == CircuitState.InitFlags.InitJct)
            {
                if (vsw.VSWzero_state)
                {
                    // Switch specified "on"
                    state.States[0][vsw.VSWstate] = 1.0;
                    current_state = 1.0;
                }
                else
                {
                    // Switch specified "off"
                    state.States[0][vsw.VSWstate] = 0.0;
                    current_state = 0.0;
                }
            }
            else if (state.UseSmallSignal)
            {
                previous_state = state.States[0][vsw.VSWstate];
                current_state = previous_state;
            }
            else if (state.UseDC)
            {
                // Time-independent calculations: use current state
                previous_state = state.States[0][vsw.VSWstate];
                v_ctrl = rstate.OldSolution[vsw.VSWcontPosNode] - rstate.OldSolution[vsw.VSWcontNegNode];

                // Calculate the current state
                if (v_ctrl > (model.VSWthresh + model.VSWhyst))
                    current_state = 1.0;
                else if (v_ctrl < (model.VSWthresh - model.VSWhyst))
                    current_state = 0.0;
                else
                    current_state = previous_state;

                // Store the current state
                if (current_state == 0.0)
                    state.States[0][vsw.VSWstate] = 0.0;
                else
                    state.States[0][vsw.VSWstate] = 1.0;

                // If the state changed, ensure one more iteration
                if (current_state != previous_state)
                    state.IsCon = false;
            }
            else
            {
                // Get the previous state
                previous_state = state.States[1][vsw.VSWstate];
                v_ctrl = rstate.OldSolution[vsw.VSWcontPosNode] - rstate.OldSolution[vsw.VSWcontNegNode];

                if (v_ctrl > (model.VSWthresh + model.VSWhyst))
                    current_state = 1.0;
                else if (v_ctrl < (model.VSWthresh - model.VSWhyst))
                    current_state = 0.0;
                else
                    current_state = previous_state;

                // Store the state
                if (current_state == 0.0)
                    state.States[0][vsw.VSWstate] = 0.0;
                else
                    state.States[0][vsw.VSWstate] = 1.0;
            }

            g_now = current_state > 0.0 ? model.VSWonConduct : model.VSWoffConduct;
            vsw.VSWcond = g_now;

            // Load the Y-matrix
            rstate.Matrix[vsw.VSWposNode, vsw.VSWposNode] += g_now;
            rstate.Matrix[vsw.VSWposNode, vsw.VSWnegNode] -= g_now;
            rstate.Matrix[vsw.VSWnegNode, vsw.VSWposNode] -= g_now;
            rstate.Matrix[vsw.VSWnegNode, vsw.VSWnegNode] += g_now;
        }

    }
}
