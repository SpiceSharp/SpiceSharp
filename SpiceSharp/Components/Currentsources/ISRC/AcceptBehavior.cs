using SpiceSharp.Circuits;

using SpiceSharp.Components.ISRC;

namespace SpiceSharp.Behaviors.ISRC
{
    /// <summary>
    /// Accept behavior for a <see cref="Components.Currentsource"/>
    /// </summary>
    public class AcceptBehavior : Behaviors.AcceptBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcceptBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="pool">Pool</param>
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
            bp.ISRCwaveform?.Accept(ckt);
        }
    }
}
