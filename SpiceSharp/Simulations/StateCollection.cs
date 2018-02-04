using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Collection
    /// </summary>
    public class StateCollection : TypeDictionary<State>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public StateCollection()
            : base(typeof(State))
        {
        }

        /// <summary>
        /// Add a state
        /// </summary>
        /// <param name="state">State</param>
        public void Add(State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            Add(state.GetType(), state);
        }
    }
}
