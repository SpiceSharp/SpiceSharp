using SpiceSharp.Simulations.Histories;
using System;

namespace SpiceSharp.Simulations
{
    public partial class Noise
    {
        /// <summary>
        /// A class that represents the state of a <see cref="Noise" /> analysis.
        /// </summary>
        /// <seealso cref="ISimulationState" />
        protected class NoiseSimulationState : INoiseSimulationState
        {
            /// <inheritdoc/>
            public string Name { get; }

            /// <inheritdoc/>
            public double OutputNoiseDensity { get; private set; }

            /// <inheritdoc/>
            public double TotalOutputNoise { get; private set; }

            /// <inheritdoc/>
            public double TotalInputNoise { get; private set; }

            /// <inheritdoc/>
            public IHistory<NoisePoint> Point { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="NoiseSimulationState"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
            public NoiseSimulationState(string name)
            {
                Name = name.ThrowIfNull(nameof(name));
                Point = new ArrayHistory<NoisePoint>(2);
            }

            /// <summary>
            /// Reset the frequency, and resets all noise contributions as well as the total
            /// integrated noise.
            /// </summary>
            /// <param name="point">The point.</param>
            public void Reset(NoisePoint point)
            {
                Point.Set(point);
                OutputNoiseDensity = 0;
                TotalOutputNoise = 0;
                TotalInputNoise = 0;
            }

            /// <summary>
            /// Sets the current noise data point.
            /// </summary>
            /// <param name="point">The noise data point.</param>
            public void SetCurrentPoint(NoisePoint point)
            {
                Point.Accept();
                Point.Value = point;

                // Reset the total noise density for our new point
                OutputNoiseDensity = 0;
                TotalOutputNoise = 0;
                TotalInputNoise = 0;
            }

            /// <summary>
            /// Adds the contributions of the specified noise source.
            /// </summary>
            /// <param name="source">The noise source.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <c>null</c>.</exception>
            public void Add(INoiseSource source)
            {
                source.ThrowIfNull(nameof(source));
                OutputNoiseDensity += source.OutputNoiseDensity;
                TotalInputNoise += source.TotalInputNoise;
                TotalOutputNoise += source.TotalOutputNoise;
            }

            /// <summary>
            /// Adds the contributions of the specified noise sources.
            /// </summary>
            /// <param name="sources">The noise sources.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="sources"/> or any of the noise sources is <c>null</c>.</exception>
            public void Add(params INoiseSource[] sources)
            {
                sources.ThrowIfNull(nameof(sources));
                for (int i = 0; i < sources.Length; i++)
                    Add(sources[i]);
            }
        }
    }
}
