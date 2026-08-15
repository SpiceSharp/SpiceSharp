namespace SpiceSharp.Simulations;

/// <summary>
/// The xoshiro256** pseudo-random number generator.
/// </summary>
/// <remarks>
/// Random generator implemented here to avoid differences between different .NET frameworks.
/// </remarks>
public sealed class Xoshiro256StarStar
{
    private ulong _s0, _s1, _s2, _s3;

    /// <summary>
    /// Initializes a new instance of the <see cref="Xoshiro256StarStar"/> class.
    /// </summary>
    /// <param name="seed">The seed. Any value, including 0, is a valid seed.</param>
    public Xoshiro256StarStar(ulong seed)
    {
        var seeder = new SplitMix64(seed);
        _s0 = seeder.Next();
        _s1 = seeder.Next();
        _s2 = seeder.Next();
        _s3 = seeder.Next();
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
            ulong result = RotateLeft(_s1 * 5ul, 7) * 9ul;
            ulong t = _s1 << 17;
            _s2 ^= _s0;
            _s3 ^= _s1;
            _s1 ^= _s2;
            _s0 ^= _s3;
            _s2 ^= t;
            _s3 = RotateLeft(_s3, 45);
            return result;
        }
    }

    /// <summary>
    /// Generates the next random number in the open interval (0, 1).
    /// </summary>
    /// <returns>
    /// The random number.
    /// </returns>
    /// <remarks>
    /// Both endpoints are excluded, so the result can be fed to a logarithm without a guard.
    /// </remarks>
    public double NextDouble()
    {
        // 53 bits of mantissa, offset by half a unit in the last place to exclude both endpoints.
        return ((Next() >> 11) + 0.5) * (1.0 / 9007199254740992.0);
    }

    private static ulong RotateLeft(ulong value, int count)
        => (value << count) | (value >> (64 - count));
}
