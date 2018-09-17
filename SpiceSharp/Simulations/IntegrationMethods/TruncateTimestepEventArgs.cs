using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Event arguments for probing a new time point
    /// </summary>
    public class TruncateTimestepEventArgs : EventArgs
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
                if (value > 0.0 || double.IsPositiveInfinity(value))
                {
                    if (value < _delta)
                        _delta = value;
                }
                else
                    throw new ArgumentException("Cannot set the the timestep to a non-positive values");
            }
        }
        private double _delta;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simulation">The simulation</param>
        public TruncateTimestepEventArgs(TimeSimulation simulation)
            : this(simulation, double.PositiveInfinity)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simulation">The simulation</param>
        /// <param name="delta">The initial timestep</param>
        public TruncateTimestepEventArgs(TimeSimulation simulation, double delta)
        {
            Simulation = simulation;
            _delta = delta;
        }
    }
}
