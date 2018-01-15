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
        protected HistoryPoint First { get; private set; }

        /// <summary>
        /// Number of states in the pool
        /// </summary>
        public int StateCount { get; private set; }

        /// <summary>
        /// Number of states and derivatives in the pool
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Get the number of points in history
        /// </summary>
        public int HistoryCount { get; }

        /// <summary>
        /// Get the values
        /// </summary>
        public double[] Values { get => First.Values; }
        
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
        /// Get a state variable value in history
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="history">Number of points to go back in time</param>
        /// <returns></returns>
        public double GetPreviousValue(int index, int history = 1)
        {
            HistoryPoint hp = First;
            for (int i = 0; i < history; i++)
                hp = hp.Previous;
            return hp.Values[index];
        }

        /// <summary>
        /// Get a timestep
        /// </summary>
        /// <param name="history">The number of timesteps to go back in history (current timestep by default)</param>
        /// <returns></returns>
        public double GetTimestep(int history = 0) => Method.DeltaOld[history];

        /// <summary>
        /// Create a new state
        /// </summary>
        /// <param name="order">Order of the state variable</param>
        /// <returns></returns>
        public StateVariable Create(int order = 1)
        {
            StateVariable result = new StateVariable(this, Size, order);

            // Increase amount of states
            StateCount++;

            // Increase number of stored values
            Size += order + 1;

            return result;
        }

        /// <summary>
        /// Integrate a state variable
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns></returns>
        public void Integrate(int index) => Method.Integrate(First, index);

        /// <summary>
        /// Integrate a state variable
        /// This method will also calculate contributions for the Y-matrix and Rhs-vector
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="cap">Capacitance</param>
        /// <returns></returns>
        public IntegrationMethod.Result Integrate(int index, double cap) => Method.Integrate(First, index, cap);

        /// <summary>
        /// Integrate a state variable
        /// This method will also calculate contributions for the Y-matrix and Rhs-vector
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="dqdv">The derivative of the state variable w.r.t. a voltage across</param>
        /// <param name="v">The voltage across</param>
        /// <returns>The contributions to the Y-matrix and Rhs-vector</returns>
        public IntegrationMethod.Result Integrate(int index, double dqdv, double v) => Method.Integrate(First, index, dqdv, v);

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
                current.Values = new double[Size];
                current = current.Next;
            }
            while (current != First);
        }

        /// <summary>
        /// Clear all states for DC
        /// </summary>
        public void ClearDC()
        {
            // Copy values to all other states
            HistoryPoint current = First.Next;
            while (current != First)
            {
                for (int i = 0; i < StateCount; i++)
                    current.Values[i] = First.Values[i];
                current = current.Next;
            }
        }

        /// <summary>
        /// Shift states
        /// </summary>
        public void ShiftStates() => First = First.Next;
    }
}
