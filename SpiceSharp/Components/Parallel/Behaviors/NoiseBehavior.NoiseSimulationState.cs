using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SpiceSharp.Components.ParallelBehaviors
{
    public partial class NoiseBehavior
    {
        protected class NoiseSimulationState : INoiseSimulationState
        {
            private readonly INoiseSimulationState _parent;
            private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

            /// <summary>
            /// Gets or sets the current frequency.
            /// </summary>
            public double Frequency => _parent.Frequency;

            /// <summary>
            /// Gets or sets the frequency step.
            /// </summary>
            public double DeltaFrequency => _parent.DeltaFrequency;

            /// <summary>
            /// Output referred noise
            /// </summary>
            public double OutputNoise 
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

            public double InputNoise 
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

            /// <summary>
            /// Gets or sets the total output noise density.
            /// </summary>
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

            /// <summary>
            /// Gets or sets the inverse squared gain.
            /// </summary>
            /// <remarks>
            /// This value is used to compute the input noise density from the output noise density.
            /// </remarks>
            public double GainInverseSquared => _parent.GainInverseSquared;

            /// <summary>
            /// Gets the logarithm of the gain squared.
            /// </summary>
            public double LogInverseGain => _parent.LogInverseGain;

            /// <summary>
            /// Initializes a new instance of the <see cref="NoiseSimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public NoiseSimulationState(INoiseSimulationState parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <summary>
            /// This subroutine evaluate the integration of the function
            /// NOISE = a * (FREQUENCY) ^ (EXPONENT)
            /// given two points from the curve. If EXPONENT is relatively close to 0, the noise is simply multiplied
            /// by the change in frequency.
            /// If it isn't, a more complicated expression must be used.
            /// Note that EXPONENT = -1 gives a different equation than EXPONENT != -1.
            /// </summary>
            /// <param name="noiseDensity">The noise density.</param>
            /// <param name="logNoiseDensity">The previous noise density</param>
            /// <param name="lastLogNoiseDensity">The previous log noise density</param>
            /// <returns>
            /// The integrated noise.
            /// </returns>
            public double Integrate(double noiseDensity, double logNoiseDensity, double lastLogNoiseDensity)
                => _parent.Integrate(noiseDensity, logNoiseDensity, lastLogNoiseDensity);
        }
    }
}
