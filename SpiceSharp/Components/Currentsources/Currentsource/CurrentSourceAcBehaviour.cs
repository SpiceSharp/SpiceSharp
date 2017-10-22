using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// Behaviour of a currentsource in AC analysis
    /// </summary>
    public class CurrentsourceAcBehaviour : CircuitObjectBehaviourAcLoad
    {
        /// <summary>
        /// Execute AC behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var source = ComponentTyped<Currentsource>();

            var cstate = ckt.State.Complex;
            cstate.Rhs[source.ISRCposNode] += source.ISRCac;
            cstate.Rhs[source.ISRCnegNode] -= source.ISRCac;
        }
    }
}
