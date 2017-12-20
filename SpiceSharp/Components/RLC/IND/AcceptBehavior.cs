using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.IND
{
    /// <summary>
    /// Accept behavior for inductors
    /// </summary>
    public class AcceptBehavior : Behaviors.AcceptBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private TransientBehavior load;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Componnet</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(Entity component, Circuit ckt)
        {
            // Get behaviors
            load = GetBehavior<TransientBehavior>(component);
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Accept(Circuit ckt)
        {
            if (ckt.State.Init == State.InitFlags.InitTransient)
                ckt.State.CopyDC(load.INDstate + TransientBehavior.INDflux);
        }
    }
}
