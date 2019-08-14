using SpiceSharp.Algebra;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// This class is responsible for managing states.
    /// </summary>
    /// <seealso cref="StateHistory"/>
    /// <seealso cref="StateDerivative"/>
    public class StateManager
    {
        /// <summary>
        /// Gets the number of states in the pool.
        /// </summary>
        public int States { get; private set; }

        /// <summary>
        /// Gets the number of different values in the pool.
        /// </summary>
        /// <remarks>
        /// A state that also calculates a derivative, will need one more memory for
        /// storing that derivative. This property will return the total size needed
        /// to store all such values.
        /// </remarks>
        public int Size { get; private set; }

        /// <summary>
        /// Allocates a state.
        /// </summary>
        /// <param name="order">The order of the state.</param>
        /// <returns>
        /// The index of the newly allocated state.
        /// </returns>
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
        /// Build a vector that can represent all requested states.
        /// </summary>
        /// <returns>
        /// A vector that can hold all the state values.
        /// </returns>
        public Vector<double> Build() => new DenseVector<double>(Size);

        /// <summary>
        /// Destroys the state manager.
        /// </summary>
        public void Unsetup()
        {
            States = 0;
            Size = 0;
        }
    }
}
