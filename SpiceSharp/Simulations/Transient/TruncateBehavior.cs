﻿using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Behavior for truncating the current timestep
    /// </summary>
    public abstract class TruncateBehavior : Behavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected TruncateBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Truncate the current timestep
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="timestep">Timestep</param>
        public abstract void Truncate(TimeSimulation simulation, ref double timestep);
    }
}
