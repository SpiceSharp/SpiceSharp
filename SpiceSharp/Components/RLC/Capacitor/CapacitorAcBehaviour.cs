using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// AC behaviour for <see cref="Capacitor"/>
    /// </summary>
    public class CapacitorAcBehaviour : CircuitObjectBehaviourAcLoad
    {
        /// <summary>
        /// Execute behaviour for AC analysis
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            Capacitor cap = ComponentTyped<Capacitor>();
            var cstate = ckt.State.Complex;
            var val = cstate.Laplace * cap.CAPcapac.Value;

            // Load the matrix
            cstate.Matrix[cap.CAPposNode, cap.CAPposNode] += val;
            cstate.Matrix[cap.CAPposNode, cap.CAPnegNode] -= val;
            cstate.Matrix[cap.CAPnegNode, cap.CAPposNode] -= val;
            cstate.Matrix[cap.CAPnegNode, cap.CAPnegNode] += val;
        }
    }
}
