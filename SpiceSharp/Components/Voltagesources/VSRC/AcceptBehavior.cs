using SpiceSharp.Circuits;
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
        /// <param name="parameters"></param>
        /// <param name="pool"></param>
        public override void Setup(ParametersCollection parameters, BehaviorPool pool)
        {
            bp = parameters.Get<BaseParameters>();
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Accept(Circuit ckt)
        {
            bp.VSRCwaveform?.Accept(ckt);
        }
    }
}
