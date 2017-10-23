using SpiceSharp.Circuits;
using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// General behaviour for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class CurrentSwitchLoadBehaviour : CircuitObjectBehaviourLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var csw = ComponentTyped<CurrentSwitch>();
            CurrentSwitchModel model = csw.Model as CurrentSwitchModel;
            double g_now;
            double i_ctrl;
            double previous_state;
            double current_state = 0.0;
            var state = ckt.State;
            var rstate = state.Real;

            // decide the state of the switch
            if (state.Init == CircuitState.InitFlags.InitFix || state.Init == CircuitState.InitFlags.InitJct)
            {
                if (csw.CSWzero_state)
                {
                    // Switch specified "on"
                    state.States[0][csw.CSWstate] = 1.0;
                    current_state = 1.0;
                }
                else
                {
                    // Switch specified "off"
                    state.States[0][csw.CSWstate] = 0.0;
                    current_state = 0.0;
                }
            }
            else if (state.UseSmallSignal)
            {
                previous_state = state.States[0][csw.CSWstate];
                current_state = previous_state;
            }
            else if (state.UseDC)
            {
                // No time-dependence, so use current state instead
                previous_state = state.States[0][csw.CSWstate];
                i_ctrl = rstate.OldSolution[csw.CSWcontBranch];
                if (i_ctrl > (model.CSWthresh + model.CSWhyst))
                    current_state = 1.0;
                else if (i_ctrl < (model.CSWthresh - model.CSWhyst))
                    current_state = 0.0;
                else
                    current_state = previous_state;

                // Store the current state
                if (current_state == 0)
                    state.States[0][csw.CSWstate] = 0.0;
                else
                    state.States[0][csw.CSWstate] = 1.0;

                // Ensure one more iteration
                if (current_state != previous_state)
                    state.IsCon = false;
            }
            else
            {
                // Get the previous state
                previous_state = state.States[1][csw.CSWstate];
                i_ctrl = rstate.OldSolution[csw.CSWcontBranch];

                // Calculate the current state
                if (i_ctrl > (model.CSWthresh + model.CSWhyst))
                    current_state = 1;
                else if (i_ctrl < (model.CSWthresh - model.CSWhyst))
                    current_state = 0;
                else
                    current_state = previous_state;

                // Store the current state
                if (current_state == 0)
                    state.States[0][csw.CSWstate] = 0.0;
                else
                    state.States[0][csw.CSWstate] = 1.0;
            }

            // Get the current conduction
            g_now = current_state != 0.0 ? (model.CSWonConduct) : (model.CSWoffConduct);
            csw.CSWcond = g_now;

            // Load the Y-matrix
            rstate.Matrix[csw.CSWposNode, csw.CSWposNode] += g_now;
            rstate.Matrix[csw.CSWposNode, csw.CSWnegNode] -= g_now;
            rstate.Matrix[csw.CSWnegNode, csw.CSWposNode] -= g_now;
            rstate.Matrix[csw.CSWnegNode, csw.CSWnegNode] += g_now;
        }
    }
}
