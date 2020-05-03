using SpiceSharp.Simulations;
using System.Threading;

namespace SpiceSharp.Components.ParallelBehaviors
{
    public partial class NoiseBehavior
    {
        /// <summary>
        /// An <see cref="INoiseSimulationState"/> for a <see cref="ParallelComponents"/>.
        /// </summary>
        /// <seealso cref="INoiseSimulationState" />
        protected class NoiseSimulationState : INoiseSimulationState
        {
            private readonly INoiseSimulationState _parent;
            private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

            /// <inheritdoc/>
            public double OutputNoiseDensity
            {
                get
                {
                    _lock.EnterReadLock();
                    try
                    {
                        return _parent.OutputNoiseDensity;
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                }
            }

            /// <inheritdoc/>
            public double TotalOutputNoise
            {
                get
                {
                    _lock.EnterReadLock();
                    try
                    {
                        return _parent.TotalOutputNoise;
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                }
            }

            /// <inheritdoc/>
            public double TotalInputNoise
            {
                get
                {
                    _lock.EnterReadLock();
                    try
                    {
                        return _parent.TotalInputNoise;
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                }
            }

            /// <inheritdoc/>
            public IHistory<NoisePoint> Point
            {
                get
                {
                    _lock.EnterReadLock();
                    try
                    {
                        return _parent.Point;
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="NoiseSimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public NoiseSimulationState(INoiseSimulationState parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }
        }
    }
}
