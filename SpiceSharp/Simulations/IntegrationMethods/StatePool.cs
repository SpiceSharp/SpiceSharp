namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Class responsible for managing state variables
    /// </summary>
    public class StatePool
    {
        /// <summary>
        /// Class for points in history
        /// </summary>
        public class HistoryPoint
        {
            public double[] Values;
            public double[] Derivatives;
            public HistoryPoint Next;
            public HistoryPoint Previous;
        }

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
        /// <param name="points">Number of points in history</param>
        public StatePool(int points)
        {
            // Create a linked list for all history points
            First = new HistoryPoint();
            HistoryPoint current = First;
            for (int i = 0; i < points; i++)
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
    }
}
