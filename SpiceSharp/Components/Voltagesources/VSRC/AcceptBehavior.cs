using SpiceSharp.Simulations;
using SpiceSharp.Components.VSRC;

namespace SpiceSharp.Behaviors.VSRC
{
    /// <summary>
    /// Accept behavior for a <see cref="Components.Voltagesource"/>
    /// </summary>
    public class AcceptBehavior : Behaviors.AcceptBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcceptBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Accept(TimeSimulation sim)
        {
            bp.VSRCwaveform?.Accept(sim);
        }
    }
}
