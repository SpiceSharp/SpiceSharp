using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations;

/// <summary>
/// The per-section amplitude weights that make the ladder of a
/// <see cref="ITimeNoiseSimulationState.FlickerLadder"/> realize a <c>1/f^beta</c> power spectral
/// density.
/// </summary>
/// <remarks>
/// Based on exponent in 1/f^beta, and weights are constant for the whole run.
/// </remarks>
/// <seealso cref="ITimeNoiseSimulationState"/>
public sealed class FlickerWeights
{
    /// <summary>
    /// Smallest possible exponent above which convergence can still happen.
    /// </summary>
    public const double MinimumExponent = 0.0;

    /// <summary>
    /// Largest possible exponent below which convergence can still happen.
    /// </summary>
    public const double MaximumExponent = 2.0;

    /// <summary>
    /// Gets the exponent that these weights realize.
    /// </summary>
    /// <value>
    /// The exponent beta of <c>1/f^beta</c>.
    /// </value>
    public double Exponent { get; }

    /// <summary>
    /// Gets the amplitude of every section, <c>sqrt(w_i)</c>. It is indexed in lockstep with
    /// <see cref="ITimeNoiseSimulationState.FlickerLadder"/> and
    /// <see cref="ITimeNoiseSimulationState.FlickerRates"/>.
    /// </summary>
    /// <value>
    /// The amplitude of every section.
    /// </value>
    public IReadOnlyList<double> Amplitudes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlickerWeights"/> class.
    /// </summary>
    /// <param name="exponent">The roll-off exponent of <c>1/f^beta</c>.</param>
    /// <param name="rates">The rates of the ladder sections, in rad/s. They have to be spaced logarithmically.</param>
    /// <param name="order">The number of poles of a single ladder section.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rates"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="rates"/> holds fewer than two rates, or a rate that is not positive.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="exponent"/> is outside <see cref="MinimumExponent"/> and <see cref="MaximumExponent"/>, or if
    /// <paramref name="order"/> is not between 1 and <see cref="TimeNoisePoint.MaximumOrder"/>.</exception>
    public FlickerWeights(double exponent, IReadOnlyList<double> rates, int order)
    {
        rates.ThrowIfNull(nameof(rates));
        if (!(exponent > MinimumExponent) || !(exponent < MaximumExponent))
            throw new ArgumentOutOfRangeException(nameof(exponent));
        if (order < 1 || order > TimeNoisePoint.MaximumOrder)
            throw new ArgumentOutOfRangeException(nameof(order));
        if (rates.Count < 2)
            throw new ArgumentException(Properties.Resources.Simulations_NoiseTransient_FlickerLadderTooShort, nameof(rates));
        Exponent = exponent;

        // The spacing of the ladder. It is recovered from the rates themselves rather than passed
        // separately, so that the weights cannot disagree with the poles they belong to.
        double ratio = Math.Log(rates[rates.Count - 1] / rates[0]) / (rates.Count - 1);

        double c = Math.Sin(Math.PI * exponent / 2.0) * Math.Pow(2.0 * Math.PI, exponent - 1.0);
        if (order > 1)
            c /= MaximumExponent - exponent;

        double scale = ratio * c;
        double[] amplitudes = new double[rates.Count];
        for (int i = 0; i < amplitudes.Length; i++)
        {
            if (!(rates[i] > 0.0))
                throw new ArgumentException(Properties.Resources.Simulations_NoiseTransient_FlickerLadderInvalidRate, nameof(rates));

            // Math.Pow(x, 0.0) is exactly 1, so a 1/f density gets the equal-weight ladder without
            // any rounding of its own.
            amplitudes[i] = Math.Sqrt(scale * Math.Pow(rates[i], 1.0 - exponent));
        }
        Amplitudes = amplitudes;
    }
}
