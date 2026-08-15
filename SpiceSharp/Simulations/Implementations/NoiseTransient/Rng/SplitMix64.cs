namespace SpiceSharp.Simulations;

/// <summary>
/// The splitmix64 pseudo-random number generator.
/// </summary>
/// <remarks>
/// Used to avoid differences between different implementations by .NET.
/// </remarks>
public sealed class SplitMix64
{
    private ulong _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitMix64"/> class.
    /// </summary>
    /// <param name="seed">The seed. Any value, including 0, is a valid seed.</param>
    public SplitMix64(ulong seed)
    {
        _state = seed;
    }

    /// <summary>
    /// Generates the next 64 random bits.
    /// </summary>
    /// <returns>
    /// The random bits.
    /// </returns>
    public ulong Next()
    {
        unchecked
        {
            _state += 0x9E3779B97F4A7C15ul;
            ulong z = _state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9ul;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBul;
            return z ^ (z >> 31);
        }
    }
}
