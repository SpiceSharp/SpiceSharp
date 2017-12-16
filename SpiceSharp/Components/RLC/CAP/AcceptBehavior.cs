using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.CAP
{
    /// <summary>
    /// Accept behavior for a <see cref="Components.Capacitor"/>
    /// </summary>
    public class AcceptBehavior : Behaviors.AcceptBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private TransientBehavior tran = null;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(Entity component, Circuit ckt)
        {
            // Get behaviors
            tran = GetBehavior<TransientBehavior>(component);
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Accept(Circuit ckt)
        {
            // Copy DC states when accepting the first timepoint
            if (ckt.State.Init == State.InitFlags.InitTransient)
                ckt.State.CopyDC(tran.CAPstate + TransientBehavior.CAPqcap);
        }
    }
}
