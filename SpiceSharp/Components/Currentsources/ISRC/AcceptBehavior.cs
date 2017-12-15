using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ISRC
{
    /// <summary>
    /// Accept behavior for current sources
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
            load.ISRCwaveform?.Accept(ckt);
        }
    }
}
