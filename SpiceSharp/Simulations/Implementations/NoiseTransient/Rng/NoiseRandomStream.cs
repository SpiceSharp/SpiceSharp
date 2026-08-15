using System;

namespace SpiceSharp.Simulations;

/// <summary>
/// A reproducible stream of standard normal variates for a single transient noise source.
/// </summary>
/// <remarks>
/// Uses Box-Muller transform for normal distribution variables.
/// </remarks>
public sealed class NoiseRandomStream
{
    private const ulong FnvOffsetBasis = 0xCBF29CE484222325ul;
    private const ulong FnvPrime = 0x100000001B3ul;
    private readonly Xoshiro256StarStar _rng;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoiseRandomStream"/> class.
    /// </summary>
    /// <param name="seed">The seed of the stream.</param>
    public NoiseRandomStream(ulong seed)
    {
        _rng = new Xoshiro256StarStar(seed);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoiseRandomStream"/> class for a named noise
    /// source.
    /// </summary>
    /// <param name="seed">The master seed of the simulation.</param>
    /// <param name="name">The name of the noise source.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
    public NoiseRandomStream(int seed, string name)
        : this(CreateSeed(seed, name))
    {
    }

    /// <summary>
    /// Combines a master seed and the name of a noise source into a stream seed.
    /// </summary>
    /// <param name="seed">The master seed of the simulation.</param>
    /// <param name="name">The name of the noise source.</param>
    /// <returns>
    /// The seed of the stream of the noise source.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
    /// <remarks>
    /// A FNV-1a hash over the code units of the name, avalanched by <see cref="SplitMix64"/>. The
    /// hash of <see cref="string"/> is deliberately not used: it is randomized per process, which
    /// would make a result irreproducible even within one Spice# version.
    /// </remarks>
    public static ulong CreateSeed(int seed, string name)
    {
        name.ThrowIfNull(nameof(name));
        unchecked
        {
            ulong hash = FnvOffsetBasis ^ (ulong)seed;
            foreach (char c in name)
            {
                hash = (hash ^ ((ulong)c & 0xFFul)) * FnvPrime;
                hash = (hash ^ (((ulong)c >> 8) & 0xFFul)) * FnvPrime;
            }
            return new SplitMix64(hash).Next();
        }
    }

    /// <summary>
    /// Generates the next random number in the open interval (0, 1).
    /// </summary>
    /// <returns>
    /// The random number.
    /// </returns>
    public double NextDouble() => _rng.NextDouble();

    /// <summary>
    /// Generates the next pair of independent standard normal variates.
    /// </summary>
    /// <param name="first">The first variate.</param>
    /// <param name="second">The second variate.</param>
    public void NextNormals(out double first, out double second)
    {
        double radius = Math.Sqrt(-2.0 * Math.Log(_rng.NextDouble()));
        double angle = 2.0 * Math.PI * _rng.NextDouble();
        first = radius * Math.Cos(angle);
        second = radius * Math.Sin(angle);
    }
}
