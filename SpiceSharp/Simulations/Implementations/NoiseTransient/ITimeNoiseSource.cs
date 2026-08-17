namespace SpiceSharp.Simulations;

/// <summary>
/// Describes a noise source of a <see cref="NoiseTransient"/> analysis.
/// </summary>
public interface ITimeNoiseSource
{
    /// <summary>
    /// Gets the name of the noise source. Must be unique within the current scope.
    /// </summary>
    /// <value>
    /// The name of the noise source.
    /// </value>
    string Name { get; }

    /// <summary>
    /// Gets the one-sided power spectral density at the last accepted operating point, in A^2/Hz.
    /// </summary>
    /// <value>
    /// The noise density.
    /// </value>
    double NoiseDensity { get; }

    /// <summary>
    /// Gets the frozen current realization for the probed timepoint, in A.
    /// </summary>
    /// <value>
    /// The noise current.
    /// </value>
    double Current { get; }
}
