using System;
using SpiceSharp.Sparse;

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
        public Vector<double> Values { get => First.Values; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="method">The integration method</param>
        public StatePool(IntegrationMethod method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
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
        public double GetPreviousValue(int index, int history)
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
        public double GetTimestep(int history) => Method.DeltaOld[history];

        /// <summary>
        /// Create a state that can be differentiated
        /// </summary>
        /// <returns></returns>
        public StateDerivative Create()
        {
            StateDerivative result = new StateDerivative(this, Size);

            // Increase amount of states
            StateCount++;

            // Increase number of stored values
            Size += 2;
            return result;
        }

        /// <summary>
        /// Create a state that can look back in history
        /// </summary>
        /// <returns></returns>
        public StateHistory CreateHistory()
        {
            StateHistory result = new StateHistory(this, Size);
            StateCount++;
            Size++;
            return result;
        }

        /// <summary>
        /// Integrate a state variable
        /// </summary>
        /// <param name="index">Index</param>
        public void Integrate(int index) => Method.Integrate(First, index);
        
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
                current.Values = new Vector<double>(Size);
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
