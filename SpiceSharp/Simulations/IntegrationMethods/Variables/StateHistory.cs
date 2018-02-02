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
        protected StatePool Source { get; }

        /// <summary>
        /// The identifier/index for the state variable
        /// </summary>
        protected int StateIndex { get; }

        /// <summary>
        /// Gets or sets the value of the state at the current timepoint
        /// </summary>
        public double Current
        {
            get => Source.History.Current[StateIndex];
            set => Source.History.Current[StateIndex] = value;
        }

        /// <summary>
        /// Gets a point in history
        /// </summary>
        /// <param name="index">Steps to go back in history</param>
        /// <returns></returns>
        public double this[int index]
        {
            get => Source.History[index][StateIndex];
        }

        /// <summary>
        /// Gets the timesteps in history
        /// </summary>
        public ReadOnlyHistory<double> Timesteps
        {
            get => new ReadOnlyHistory<double>(Source.Method.DeltaOld);
        }

        /// <summary>
        /// Constructor
        /// This constructor should not be used, except for in the <see cref="StatePool"/> class.
        /// </summary>
        /// <param name="source">Pool of states instantiating the variable</param>
        /// <param name="index">The index/identifier of the state variable</param>
        public StateHistory(StatePool source, int index)
        {
            Source = source;
            StateIndex = index;
        }
    }
}
