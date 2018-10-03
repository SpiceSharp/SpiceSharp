using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CurrentSwitchBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class AcceptBehavior : BaseAcceptBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private LoadBehavior _load;

        /// <summary>
        /// Gets the previous state
        /// </summary>
        public bool PreviousState { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcceptBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get behaviors
            _load = provider.GetBehavior<LoadBehavior>();
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Accept(TimeSimulation simulation)
        {
            // Flag the load behavior to use our previous state
            _load.UseOldState = true;

            // Store the last state
            PreviousState = _load.CurrentState;
            _load.PreviousState = PreviousState;
        }
    }
}
