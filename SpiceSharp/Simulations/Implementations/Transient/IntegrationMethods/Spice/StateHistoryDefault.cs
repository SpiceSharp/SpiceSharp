namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// A class that implements a simple default history for states.
    /// </summary>
    /// <seealso cref="StateHistory" />
    /// <remarks>
    /// This class is usually the default as states with just a history aren't specific to the integration method.
    /// </remarks>
    public class StateHistoryDefault : StateHistory
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly int _index;
        private readonly History<IntegrationState> _source;

        /// <summary>
        /// Gets or sets the value of the state at the current timepoint.
        /// </summary>
        public override double Current
        {
            get => _source[0].State[_index];
            set => _source[0].State[_index] = value;
        }

        /// <summary>
        /// Gets the <see cref="double"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public override double this[int index] => _source[index].State[_index];

        /// <summary>
        /// Initializes a new instance of the <see cref="StateHistoryDefault"/> class.
        /// </summary>
        /// <param name="source">The source history.</param>
        /// <param name="manager">The state manager.</param>
        public StateHistoryDefault(History<IntegrationState> source, StateManager manager)
        {
            _source = source;
            _index = manager.AllocateState();
        }
    }
}
