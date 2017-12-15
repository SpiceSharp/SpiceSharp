using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.IND
{
    /// <summary>
    /// Accept behavior for inductors
    /// </summary>
    public class AcceptBehavior : CircuitObjectBehaviorAccept
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Componnet</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Accept(Circuit ckt)
        {
            if (ckt.State.Init == CircuitState.InitFlags.InitTransient)
                ckt.State.CopyDC(load.INDstate + LoadBehavior.INDflux);
        }
    }
}
