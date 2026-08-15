using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Simulations;

/// <summary>
/// A configuration for a <see cref="NoiseTransient"/> analysis.
/// </summary>
/// <seealso cref="ParameterSet"/>
[GeneratedParameters]
public partial class NoiseTransientParameters : ParameterSet, ICloneable<NoiseTransientParameters>
{
    /// <summary>
    /// Gets or sets the maximum noise frequency, in Hz. Required for noise transients.
    /// Choose this value 10x higher of the signal bandwidth of interest for ~90% of the noise power. Use
    /// 100x higher for ~99% noise power.
    /// </summary>
    /// <value>
    /// The maximum noise frequency.
    /// </value>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the value is not positive.</exception>
    [ParameterName("fmax"), ParameterName("maxfreq"), ParameterInfo("The maximum noise frequency.")]
    [GreaterThan(0), Finite]
    private double _maximumNoiseFrequency;

    /// <summary>
    /// Gets or sets the number of poles of the shaping filter that band-limits every noise source.
    /// Higher order will allow a more favorable timestep truncation, at the expense of some loss
    /// in noise power.
    /// </summary>
    /// <value>
    /// The number of shaping poles.
    /// </value>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the value is not between 1 and
    /// <see cref="TimeNoisePoint.MaximumOrder"/>.</exception>
    [ParameterName("bandorder"), ParameterInfo("The number of poles of the band-limiting shaping filter.")]
    [GreaterThanOrEquals(1), LessThanOrEquals(TimeNoisePoint.MaximumOrder)]
    private int _bandLimitOrder = 2;

    /// <summary>
    /// Gets or sets the master seed of the analysis.
    /// </summary>
    /// <value>
    /// The seed.
    /// </value>
    [ParameterName("seed"), ParameterInfo("The master seed of the random streams.")]
    public int Seed { get; set; }

    /// <inheritdoc/>
    public NoiseTransientParameters Clone()
        => (NoiseTransientParameters)MemberwiseClone();
}
