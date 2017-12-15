using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.VSRC
{
    /// <summary>
    /// Accept behavior for a <see cref="Components.Voltagesource"/>
    /// </summary>
    public class AcceptBehavior : Behaviors.AcceptBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            load = GetBehavior<LoadBehavior>(component);
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Accept(Circuit ckt)
        {
            load.VSRCwaveform?.Accept(ckt);
        }
    }
}
