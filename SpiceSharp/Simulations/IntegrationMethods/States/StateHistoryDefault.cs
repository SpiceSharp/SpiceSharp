namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// A class that implements a simple history for states
    /// This class is usually the default as states with just a history aren't specific to the integration method
    /// </summary>
    public class StateHistoryDefault : StateHistory
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly int _index;
        private readonly History<IntegrationState> _source;

        /// <summary>
        /// Gets or sets the current value
        /// </summary>
        public override double Current
        {
            get => _source[0].State[_index];
            set => _source[0].State[_index] = value;
        }

        /// <summary>
        /// Gets a value in history
        /// </summary>
        /// <param name="index">The number of points to go back</param>
        /// <returns></returns>
        public override double this[int index] => _source[0].State[_index];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The integration states</param>
        /// <param name="manager">The state manager</param>
        public StateHistoryDefault(History<IntegrationState> source, StateManager manager)
        {
            _source = source;
            _index = manager.AllocateState();
        }
    }
}
