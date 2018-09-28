using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Dictionary of simulation states
    /// </summary>
    public class StateDictionary : TypeDictionary<SimulationState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateDictionary"/> class.
        /// </summary>
        public StateDictionary()
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
