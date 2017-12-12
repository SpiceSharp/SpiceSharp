using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Accept behavior for current sources
    /// </summary>
    public class CurrentsourceAcceptBehavior : CircuitObjectBehaviorAccept
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private CurrentsourceLoadBehavior load;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            load = GetBehavior<CurrentsourceLoadBehavior>(component);
            return true;
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Accept(Circuit ckt)
        {
            load.ISRCwaveform?.Accept(ckt);
        }
    }
}
