using SpiceSharp.Simulations;
using System.Threading;

namespace SpiceSharp.Components.ParallelBehaviors
{
    public partial class ConvergenceBehavior
    {
        /// <summary>
        /// An <see cref="IIterationSimulationState"/> that allows concurrent access.
        /// </summary>
        /// <seealso cref="IIterationSimulationState" />
        protected class IterationSimulationState : IIterationSimulationState
        {
            private readonly IIterationSimulationState _parent;
            private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

            IterationModes IIterationSimulationState.Mode => _parent.Mode;
            double IIterationSimulationState.SourceFactor => _parent.SourceFactor;
            double IIterationSimulationState.Gmin => _parent.Gmin;
            bool IIterationSimulationState.IsConvergent
            {
                get
                {
                    _lock.EnterReadLock();
                    try
                    {
                        return _parent.IsConvergent;
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                }
                set
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        _parent.IsConvergent = value;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="IterationSimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public IterationSimulationState(IIterationSimulationState parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }
        }
    }
}
