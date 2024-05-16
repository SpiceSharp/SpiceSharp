using SpiceSharp.Simulations;
using System;
using System.Threading;

namespace SpiceSharp.Components.ParallelComponents
{
    public partial class Noise
    {
        /// <summary>
        /// An <see cref="INoiseSimulationState"/> for a <see cref="ParallelComponents"/>.
        /// </summary>
        /// <seealso cref="INoiseSimulationState" />
        protected class NoiseSimulationState : INoiseSimulationState
        {
            private readonly INoiseSimulationState _parent;
            private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

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
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="parent"/> is <c>null</c>.</exception>
            public NoiseSimulationState(INoiseSimulationState parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }
        }
    }
}
