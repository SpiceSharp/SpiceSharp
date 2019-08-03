using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Event arguments for probing a new time point.
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class TruncateTimestepEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the time-based simulation.
        /// </summary>
        public TimeSimulation Simulation { get; }

        /// <summary>
        /// Gets or sets the timestep to be probed.
        /// </summary>
        /// <remarks>
        /// Be careful when increasing the timestep, as it could cause truncation errors!
        /// </remarks>
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
        /// Initializes a new instance of the <see cref="TruncateTimestepEventArgs"/> class.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public TruncateTimestepEventArgs(TimeSimulation simulation)
            : this(simulation, double.PositiveInfinity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TruncateTimestepEventArgs"/> class.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        /// <param name="delta">The maximum timestep.</param>
        public TruncateTimestepEventArgs(TimeSimulation simulation, double delta)
        {
            Simulation = simulation.ThrowIfNull(nameof(simulation));
            _delta = delta;
        }
    }
}
