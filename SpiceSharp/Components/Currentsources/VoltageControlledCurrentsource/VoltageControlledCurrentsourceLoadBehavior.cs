using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for a <see cref="VoltageControlledCurrentsource"/>
    /// </summary>
    public class VoltageControlledCurrentsourceLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var src = ComponentTyped<VoltageControlledCurrentsource>();
            var rstate = ckt.State;
            src.VCCSposContPosptr.Add(src.VCCScoeff.Value);
            src.VCCSposContNegptr.Sub(src.VCCScoeff.Value);
            src.VCCSnegContPosptr.Sub(src.VCCScoeff.Value);
            src.VCCSnegContNegptr.Add(src.VCCScoeff.Value);
        }
    }
}
