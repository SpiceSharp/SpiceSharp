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

            /// <summary>
            /// Gets the iteration mode.
            /// </summary>
            /// <value>
            /// The mode.
            /// </value>
            public IterationModes Mode => _parent.Mode;

            /// <summary>
            /// The current source factor.
            /// This parameter is changed when doing source stepping for aiding convergence.
            /// </summary>
            /// <remarks>
            /// In source stepping, all sources are considered to be at 0 which has typically only one single solution (all nodes and
            /// currents are 0V and 0A). By increasing the source factor in small steps, it is possible to progressively reach a solution
            /// without having non-convergence.
            /// </remarks>
            public double SourceFactor => _parent.SourceFactor;

            /// <summary>
            /// Gets or sets the a conductance that is shunted with PN junctions to aid convergence.
            /// </summary>
            public double Gmin => _parent.Gmin;

            /// <summary>
            /// Is the current iteration convergent?
            /// This parameter is used to communicate convergence.
            /// </summary>
            public bool IsConvergent
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
