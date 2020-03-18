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

            double INoiseSimulationState.Frequency => _parent.Frequency;
            double INoiseSimulationState.DeltaFrequency => _parent.DeltaFrequency;
            double INoiseSimulationState.OutputNoise 
            {
                get
                {
                    _lock.EnterReadLock();
                    try
                    {
                        return _parent.OutputNoise;
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
                        _parent.OutputNoise = value;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
            double INoiseSimulationState.InputNoise 
            {
                get
                {
                    _lock.EnterReadLock();
                    try
                    {
                        return _parent.InputNoise;
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
                        _parent.InputNoise = value;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
            double INoiseSimulationState.OutputNoiseDensity 
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
                set
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        _parent.OutputNoiseDensity = value;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
            double INoiseSimulationState.GainInverseSquared => _parent.GainInverseSquared;
            double INoiseSimulationState.LogInverseGain => _parent.LogInverseGain;

            /// <summary>
            /// Initializes a new instance of the <see cref="NoiseSimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public NoiseSimulationState(INoiseSimulationState parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            double INoiseSimulationState.Integrate(double noiseDensity, double logNoiseDensity, double lastLogNoiseDensity)
                => _parent.Integrate(noiseDensity, logNoiseDensity, lastLogNoiseDensity);
        }
    }
}
