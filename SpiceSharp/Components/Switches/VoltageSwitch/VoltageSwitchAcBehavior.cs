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
            var cstate = state;

            // Get the current state
            current_state = state.States[0][vsw.VSWstate];
            g_now = current_state > 0.0 ? model.VSWonConduct : model.VSWoffConduct;

            // Load the Y-matrix
            vsw.SWposPosptr.Add(g_now);
            vsw.SWposNegptr.Sub(g_now);
            vsw.SWnegPosptr.Sub(g_now);
            vsw.SWnegNegptr.Sub(g_now);
        }
    }
}
