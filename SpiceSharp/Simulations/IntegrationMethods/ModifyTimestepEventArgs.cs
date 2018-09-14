using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Event arguments for probing a new time point
    /// </summary>
    public class ModifyTimestepEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the time simulation
        /// </summary>
        public TimeSimulation Simulation { get; }

        /// <summary>
        /// Gets or sets the timestep to be probed
        /// Be careful when increasing the timestep, as it could cause truncation errors
        /// </summary>
        public double Delta
        {
            get => _delta;
            set
            {
                if (value > 0.0)
                    _delta = value;
                else
                    throw new ArgumentException("Cannot set the a timestep to 0 or negative values");
            }
        }
        private double _delta;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simulation">The simulation</param>
        /// <param name="delta">The initial timestep</param>
        public ModifyTimestepEventArgs(TimeSimulation simulation, double delta)
        {
            Simulation = simulation;
            _delta = delta;
        }
    }
}
