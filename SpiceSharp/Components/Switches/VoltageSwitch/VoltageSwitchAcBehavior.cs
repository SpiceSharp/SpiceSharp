using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="VoltageSwitch"/>
    /// </summary>
    public class VoltageSwitchAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var vsw = ComponentTyped<VoltageSwitch>();
            var model = vsw.Model as VoltageSwitchModel;
            double current_state, g_now;
            var state = ckt.State;
            var cstate = state.Complex;

            // Get the current state
            current_state = state.States[0][vsw.VSWstate];
            g_now = current_state > 0.0 ? model.VSWonConduct : model.VSWoffConduct;

            // Load the Y-matrix
            // cstate.Matrix[vsw.VSWposNode, vsw.VSWposNode] += g_now;
            // cstate.Matrix[vsw.VSWposNode, vsw.VSWnegNode] -= g_now;
            // cstate.Matrix[vsw.VSWnegNode, vsw.VSWposNode] -= g_now;
            // cstate.Matrix[vsw.VSWnegNode, vsw.VSWnegNode] += g_now; *
        }
    }
}
