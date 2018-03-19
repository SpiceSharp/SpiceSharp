using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageSwitchBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="VoltageSwitch"/>
    /// </summary>
    public class AcceptBehavior : BaseAcceptBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private LoadBehavior _load;

        /// <summary>
        /// Gets the previous state (last time point)
        /// </summary>
        public bool PreviousState { get; private set; }

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
            _load = provider.GetBehavior<LoadBehavior>("entity");
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Accept(TimeSimulation simulation)
        {
            // Flag the load behavior to use our old state
            _load.UseOldState = true;

            // Copy the current state to the old state
            PreviousState = _load.CurrentState;
            _load.PreviousState = PreviousState;
        }
    }
}
