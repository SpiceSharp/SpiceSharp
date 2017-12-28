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
        /// Get the number of points in history
        /// </summary>
        public int HistoryCount { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="method">The integration method</param>
        public StatePool(IntegrationMethod method)
        {
            Method = method;
            HistoryCount = method.MaxOrder + 2;

            // Create a linked list for all history points
            First = new HistoryPoint();
            HistoryPoint current = First;
            for (int i = 0; i < HistoryCount; i++)
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
        public IntegrationMethod.Result Integrate(int index, double cap) => Method.Integrate(First, index, cap);

        /// <summary>
        /// Truncate the timestep based on the LTE (Local Truncation Error)
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="timestep">Timestep</param>
        public void LocalTruncationError(int index, ref double timestep) => Method.LocalTruncateError(First, index, ref timestep);
        
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
        /// Clear all states for DC
        /// </summary>
        public void ClearDC()
        {
            // DC means all states are constant in time so df/dt=0
            for (int i = 0; i < StateCount; i++)
                First.Derivatives[i] = 0.0;

            // Copy values to all other states
            HistoryPoint current = First.Next;
            while (current != First)
            {
                for (int i = 0; i < StateCount; i++)
                {
                    current.Values[i] = First.Values[i];
                    current.Derivatives[i] = 0.0;
                }
                current = current.Next;
            }
        }

        /// <summary>
        /// Shift states
        /// </summary>
        public void ShiftStates() => First = First.Next;
    }
}
