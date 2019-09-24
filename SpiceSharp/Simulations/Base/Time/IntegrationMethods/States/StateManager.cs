using SpiceSharp.Algebra;
using System.Threading;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// This class is responsible for managing states.
    /// </summary>
    /// <seealso cref="StateHistory"/>
    /// <seealso cref="StateDerivative"/>
    public class StateManager
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// Gets the number of states in the pool.
        /// </summary>
        public int States
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _states;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }
        private int _states = 0;

        /// <summary>
        /// Gets the number of different values in the pool.
        /// </summary>
        /// <remarks>
        /// A state that also calculates a derivative, will need one more memory for
        /// storing that derivative. This property will return the total size needed
        /// to store all such values.
        /// </remarks>
        public int Size
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _size;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }
        private int _size;

        /// <summary>
        /// Allocates a state.
        /// </summary>
        /// <param name="order">The order of the state.</param>
        /// <returns>
        /// The index of the newly allocated state.
        /// </returns>
        public int AllocateState(int order = 0)
        {
            _lock.EnterWriteLock();
            try
            {
                // Get the assigned index
                var result = _size + 1;

                // Increase amount of states
                _states++;
                _size += order + 1;

                // Increase number of stored values
                return result;
            }
            finally
            {
                _lock.EnterWriteLock();
            }
        }

        /// <summary>
        /// Build a vector that can represent all requested states.
        /// </summary>
        /// <returns>
        /// A vector that can hold all the state values.
        /// </returns>
        public IVector<double> Build() => new DenseVector<double>(Size);

        /// <summary>
        /// Destroys the state manager.
        /// </summary>
        public void Unsetup()
        {
            _lock.EnterWriteLock();
            try
            {
                _states = 0;
                _size = 0;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
