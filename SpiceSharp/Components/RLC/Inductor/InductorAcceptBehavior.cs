using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Accept behavior for inductors
    /// </summary>
    public class InductorAcceptBehavior : CircuitObjectBehaviorAccept
    {
        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Accept(Circuit ckt)
        {
            var ind = ComponentTyped<Inductor>();
            if (ckt.State.Init == CircuitState.InitFlags.InitTransient)
                ckt.State.CopyDC(ind.INDstate + Inductor.INDflux);
        }
    }
}
