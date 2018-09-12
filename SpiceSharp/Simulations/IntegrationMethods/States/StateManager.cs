using SpiceSharp.Algebra;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Class responsible for managing state variables
    /// </summary>
    public class StateManager
    {
        /// <summary>
        /// Number of states in the pool
        /// </summary>
        public int States { get; private set; }

        /// <summary>
        /// Number of states and derivatives in the pool
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Allocate a state in the pool
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>The index in an array</returns>
        public int AllocateState(int order = 0)
        {
            // Get the assigned index
            var result = Size + 1;

            // Increase amount of states
            States++;
            Size += order + 1;

            // Increase number of stored values
            return result;
        }

        /// <summary>
        /// Build a vector that can represent all requested states
        /// </summary>
        /// <returns></returns>
        public Vector<double> Build()
        {
            return new DenseVector<double>(Size);
        }
    }
}
