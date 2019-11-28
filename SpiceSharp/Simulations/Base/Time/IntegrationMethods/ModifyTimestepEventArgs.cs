using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Event arguments for probing a new time point.
    /// </summary>
    public class ModifyTimestepEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the time simulation.
        /// </summary>
        public ITimeSimulation Simulation { get; }

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
                if (value > 0.0)
                    _delta = value;
                else
                    throw new ArgumentException(Properties.Resources.Simulations_Time_TimestepInvalid);
            }
        }
        private double _delta;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyTimestepEventArgs"/> class.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        /// <param name="delta">The timestep.</param>
        public ModifyTimestepEventArgs(ITimeSimulation simulation, double delta)
        {
            Simulation = simulation.ThrowIfNull(nameof(simulation));
            _delta = delta;
        }
    }
}
