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
    /// Gets or sets the number of sections per decade of the ladder that flicker noise sources are
    /// built from. More sections make the density follow its <c>1/f^beta</c> target more closely, at
    /// the cost of one shaping section per source per timepoint.
    /// </summary>
    /// <value>
    /// The number of ladder sections per decade.
    /// </value>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the value is not positive.</exception>
    [ParameterName("flickersections"), ParameterInfo("The number of flicker noise ladder sections per decade.")]
    [GreaterThan(0), Finite]
    private double _flickerSectionsPerDecade = 2.0;

    /// <summary>
    /// Gets or sets the number of decades that the flicker noise ladder extends below the reciprocal
    /// of the run length.
    /// </summary>
    /// <value>
    /// The number of guard decades.
    /// </value>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the value is negative.</exception>
    [ParameterName("flickerguard"), ParameterInfo("The number of decades the flicker noise ladder extends below 1/StopTime.")]
    [GreaterThanOrEquals(0), Finite]
    private double _flickerGuardDecades = 1.0;

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
