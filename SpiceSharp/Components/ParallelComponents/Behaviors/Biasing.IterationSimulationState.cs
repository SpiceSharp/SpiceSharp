using SpiceSharp.Simulations;
using System;
using System.Threading;

namespace SpiceSharp.Components.ParallelComponents
{
    public partial class Biasing
    {
        /// <summary>
        /// An <see cref="IIterationSimulationState"/> that allows concurrent access.
        /// </summary>
        /// <seealso cref="IIterationSimulationState" />
        protected class IterationSimulationState : IIterationSimulationState
        {
            private readonly IIterationSimulationState _parent;
            private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

            /// <inheritdoc/>
            IterationModes IIterationSimulationState.Mode => _parent.Mode;

            /// <inheritdoc/>
            double IIterationSimulationState.SourceFactor => _parent.SourceFactor;

            /// <inheritdoc/>
            double IIterationSimulationState.Gmin => _parent.Gmin;

            /// <inheritdoc/>
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
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="parent"/> is <c>null</c>.</exception>
            public IterationSimulationState(IIterationSimulationState parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }
        }
    }
}
