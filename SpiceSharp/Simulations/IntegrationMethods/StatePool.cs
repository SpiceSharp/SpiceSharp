namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Class responsible for managing state variables
    /// </summary>
    public class StatePool
    {
        /// <summary>
        /// Get the integration method for the state pool
        /// </summary>
        public IntegrationMethod Method { get; }

        /// <summary>
        /// The current point
        /// </summary>
        public HistoryPoint First { get; private set; }

        /// <summary>
        /// Number of states in the pool
        /// </summary>
        public int StateCount { get; private set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="method">The integration method</param>
        public StatePool(IntegrationMethod method)
        {
            Method = method;

            // Create a linked list for all history points
            First = new HistoryPoint();
            HistoryPoint current = First;
            for (int i = 0; i < method.MaxOrder + 2; i++)
            {
                HistoryPoint next = new HistoryPoint();
                next.Previous = current;
                current.Next = next;
                current = next;
            }

            // Close the loop
            First.Previous = current;
            current.Next = First;
        }

        /// <summary>
        /// Create a state variable
        /// </summary>
        /// <returns></returns>
        public StateVariable Create()
        {
            StateVariable result = new StateVariable(this, StateCount);
            StateCount++;
            return result;
        }

        /// <summary>
        /// Integrate a state variable
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns></returns>
        public IntegrationMethod.Result Integrate(int index, double cap)
        {
            return Method.Integrate(First, index, cap);
        }
        
        /// <summary>
        /// Build the arrays for all history points
        /// </summary>
        public void BuildStates()
        {
            HistoryPoint current = First;
            do
            {
                current.Values = new double[StateCount];
                current.Derivatives = new double[StateCount];
                current = current.Next;
            }
            while (current != First);
        }

        /// <summary>
        /// Shift states
        /// </summary>
        public void ShiftStates() => First = First.Next;
    }
}
