using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for a <see cref="VoltageControlledCurrentsource"/>
    /// </summary>
    public class VoltageControlledCurrentsourceAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var src = ComponentTyped<VoltageControlledCurrentsource>();
            var cstate = ckt.State;
            src.VCCSposContPosptr.Add(src.VCCScoeff.Value);
            src.VCCSposContNegptr.Sub(src.VCCScoeff.Value);
            src.VCCSnegContPosptr.Sub(src.VCCScoeff.Value);
            src.VCCSnegContNegptr.Add(src.VCCScoeff.Value);
        }
    }
}
