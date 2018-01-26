using System;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.CurrentsourceBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="Currentsource"/>
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
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            
            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Accept(TimeSimulation sim)
        {
            bp.Waveform?.Accept(sim);
        }
    }
}
