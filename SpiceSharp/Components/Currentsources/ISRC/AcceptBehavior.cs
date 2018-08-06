using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CurrentSourceBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="CurrentSource"/>
    /// </summary>
    public class AcceptBehavior : BaseAcceptBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;

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
            _bp = provider.GetParameterSet<BaseParameters>();
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Accept(TimeSimulation simulation)
        {
            _bp.Waveform?.Accept(simulation);
        }
    }
}
