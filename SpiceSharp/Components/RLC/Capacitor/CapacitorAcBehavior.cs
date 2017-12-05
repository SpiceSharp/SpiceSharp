using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="Capacitor"/>
    /// </summary>
    public class CapacitorAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Execute behaviour for AC analysis
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            Capacitor cap = ComponentTyped<Capacitor>();
            var cstate = ckt.State;
            var val = cstate.Laplace * cap.CAPcapac.Value;

            // Load the matrix
            cap.CAPposPosptr.Add(val);
            cap.CAPnegNegptr.Add(val);
            cap.CAPposNegptr.Sub(val);
            cap.CAPnegPosptr.Sub(val);
        }
    }
}
