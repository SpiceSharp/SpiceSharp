using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Accepting behavior for switches.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.SwitchBehaviors.BiasingBehavior" />
    /// <seealso cref="SpiceSharp.Behaviors.IAcceptBehavior" />
    public class AcceptBehavior : BiasingBehavior, IAcceptBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="method">The method.</param>
        public AcceptBehavior(string name, Controller method) : base(name, method)
        {
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public void Probe(TimeSimulation simulation)
        {
        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        public void Accept(TimeSimulation simulation)
        {
            UseOldState = true;
            PreviousState = CurrentState;
        }
    }
}
