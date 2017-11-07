using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="CurrentControlledCurrentsource"/>
    /// </summary>
    public class CurrentControlledCurrentsourceAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var src = ComponentTyped<CurrentControlledCurrentsource>();
            var cstate = ckt.State;
            src.CCCSposContBrptr.Add(src.CCCScoeff.Value);
            src.CCCSnegContBrptr.Sub(src.CCCScoeff.Value);
        }
    }
}
