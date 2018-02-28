using System;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Class responsible for managing state variables
    /// </summary>
    public class StatePool
    {
        /// <summary>
        /// Gets the integration method for the state pool
        /// </summary>
        public IntegrationMethod Method { get; }

        /// <summary>
        /// Gets the history
        /// </summary>
        public History<Vector<double>> History { get; }
        
        /// <summary>
        /// Number of states in the pool
        /// </summary>
        public int States { get; private set; }

        /// <summary>
        /// Number of states and derivatives in the pool
        /// </summary>
        protected int Size { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="method">The integration method</param>
        public StatePool(IntegrationMethod method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            int count = method.MaxOrder + 2;
            History = new ArrayHistory<Vector<double>>(count, (Vector<double>)null);
        }

        /// <summary>
        /// Create a state that can be differentiated
        /// </summary>
        /// <returns></returns>
        public StateDerivative CreateDerivative()
        {
            StateDerivative result = new StateDerivative(this, Size + 1);

            // Increase amount of states
            States++;

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
            States++;
            Size++;
            return result;
        }

        /// <summary>
        /// Build the arrays for all history points
        /// </summary>
        public void BuildStates() => History.Clear((int index) => new DenseVector<double>(Size));

        /// <summary>
        /// Integrate a state variable
        /// </summary>
        /// <param name="index">State index</param>
        public void Integrate(int index) => Method.Integrate(History, index);

        /// <summary>
        /// Truncate timestep
        /// </summary>
        /// <param name="index"></param>
        public void LocalTruncationError(int index, ref double timestep) => Method.LocalTruncateError(History, index, ref timestep);

        /// <summary>
        /// Clear all states for DC
        /// </summary>
        public void ClearDC()
        {
            // Copy current values to all other states
            var current = History.Current;
            foreach (var states in History)
            {
                if (states != current)
                    current.CopyTo(states);
            }
        }
    }
}
