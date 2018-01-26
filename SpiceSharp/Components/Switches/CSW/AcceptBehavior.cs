using System;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.CurrentSwitchBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class AcceptBehavior : Behaviors.AcceptBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        LoadBehavior load;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcceptBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>(0);
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Accept(TimeSimulation sim)
        {
            // Flag the load behavior to use our previous state
            load.UseOldState = true;

            // Store the last state
            load.OldState = load.CurrentState;
        }
    }
}
