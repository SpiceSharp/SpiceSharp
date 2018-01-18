namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Represents a variable with memory
    /// </summary>
    public class StateHistory
    {
        /// <summary>
        /// The source of all variables
        /// </summary>
        protected StatePool source;

        /// <summary>
        /// The identifier/index for the state variable
        /// </summary>
        protected int index;

        /// <summary>
        /// Gets or sets the value of the state at the current timepoint
        /// </summary>
        public double Value
        {
            get => source.Values[index];
            set => source.Values[index] = value;
        }

        /// <summary>
        /// Constructor
        /// This constructor should not be used, except for in the <see cref="StatePool"/> class.
        /// </summary>
        /// <param name="source">Pool of states instantiating the variable</param>
        /// <param name="index">The index/identifier of the state variable</param>
        internal StateHistory(StatePool source, int index)
        {
            this.source = source;
            this.index = index;
        }

        /// <summary>
        /// Get a value of the state variable in history
        /// </summary>
        /// <param name="history">Number of points to go back in history (last point by default)</param>
        /// <returns></returns>
        public double GetPreviousValue(int history = 1) => source.GetPreviousValue(index, history);

        /// <summary>
        /// Get the timestep (in history)
        /// </summary>
        /// <param name="history">Number of steps to go back in history (current timestep by default)</param>
        /// <returns></returns>
        public double GetTimestep(int history = 0) => source.GetTimestep(history);
    }
}
