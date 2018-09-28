using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Dictionary of simulation states
    /// </summary>
    public class SimulationStateDictionary : TypeDictionary<SimulationState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationStateDictionary"/> class.
        /// </summary>
        public SimulationStateDictionary()
            : base(typeof(SimulationState))
        {
        }

        /// <summary>
        /// Add a state to the dictionary.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <exception cref="ArgumentNullException">state</exception>
        public void Add(SimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            Add(state.GetType(), state);
        }
    }
}
