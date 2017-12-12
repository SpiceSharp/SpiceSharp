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
        /// Necessary behaviors
        /// </summary>
        private InductorLoadBehavior load;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Componnet</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            load = GetBehavior<InductorLoadBehavior>(component);
            return true;
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Accept(Circuit ckt)
        {
            if (ckt.State.Init == CircuitState.InitFlags.InitTransient)
                ckt.State.CopyDC(load.INDstate + InductorLoadBehavior.INDflux);
        }
    }
}
