using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.VSW
{
    /// <summary>
    /// Accept behavior for a <see cref="Components.VoltageSwitch"/>
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
        public AcceptBehavior(Identifier name) : base (name) { }

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
            // Flag the load behavior to use our old state
            load.VSWuseOldState = true;

            // Copy the current state to the old state
            load.VSWoldState = load.VSWcurrentState;
        }
    }
}
