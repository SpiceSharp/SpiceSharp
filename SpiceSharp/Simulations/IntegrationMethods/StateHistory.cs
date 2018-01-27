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
        protected StatePool Source { get; private set; }

        /// <summary>
        /// The identifier/index for the state variable
        /// </summary>
        protected int Index { get; private set; }

        /// <summary>
        /// Gets or sets the value of the state at the current timepoint
        /// </summary>
        public double Value
        {
            get => Source.Values[Index];
            set => Source.Values[Index] = value;
        }

        /// <summary>
        /// Constructor
        /// This constructor should not be used, except for in the <see cref="StatePool"/> class.
        /// </summary>
        /// <param name="source">Pool of states instantiating the variable</param>
        /// <param name="index">The index/identifier of the state variable</param>
        internal StateHistory(StatePool source, int index)
        {
            Source = source;
            Index = index;
        }

        /// <summary>
        /// Get a value of the state variable in history
        /// </summary>
        /// <param name="history">Number of points to go back in history</param>
        /// <returns></returns>
        public double GetPreviousValue(int history) => Source.GetPreviousValue(Index, history);

        /// <summary>
        /// Get the timestep (in history)
        /// </summary>
        /// <param name="history">Number of steps to go back in history</param>
        /// <returns></returns>
        public double GetTimestep(int history) => Source.GetTimestep(history);
    }
}
