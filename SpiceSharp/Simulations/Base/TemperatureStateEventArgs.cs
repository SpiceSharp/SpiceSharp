using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Event arguments that are used when a state is loaded.
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class TemperatureStateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the state.
        /// </summary>
        public ITemperatureSimulationState State { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureStateEventArgs"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public TemperatureStateEventArgs(ITemperatureSimulationState state)
        {
            State = state;
        }
    }
}
