using NUnit.Framework;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharpTest.Simulations;

/// <summary>
/// Tests for the random streams that drive the noise sources of a transient noise analysis.
/// </summary>
/// <remarks>
/// The golden vectors are what enforce the reproducibility contract of the analysis: the same seed,
/// the same netlist and the same Spice# version give the same result. They are the published
/// reference vectors of the two generators, so a change to them is a change of algorithm and not a
/// refactoring.
/// </remarks>
[TestFixture]
public class NoiseRandomStreamTests
{
    [Test]
    public void When_SplitMix64_Expect_ReferenceVectors()
    {
        var rng = new SplitMix64(0ul);
        ulong[] expected = [0xE220A8397B1DCDAFul, 0x6E789E6AA1B965F4ul, 0x06C45D188009454Ful, 0xF88BB8A8724C81ECul];
        foreach (ulong e in expected)
            Assert.That(rng.Next(), Is.EqualTo(e));
    }

    [Test]
    public void When_Xoshiro256StarStar_Expect_ReferenceVectors()
    {
        var rng = new Xoshiro256StarStar(0ul);
        ulong[] expected = [0x99EC5F36CB75F2B4ul, 0xBF6E1F784956452Aul, 0x1A5F849D4933E6E0ul, 0x6AA594F1262D2D2Cul];
        foreach (ulong e in expected)
            Assert.That(rng.Next(), Is.EqualTo(e));
    }

    [Test]
    public void When_SameSeed_Expect_SameSequence()
    {
        var a = new NoiseRandomStream(0xDECAFul);
        var b = new NoiseRandomStream(0xDECAFul);
        var c = new NoiseRandomStream(0xDECAEul);
        bool anyDifferent = false;
        for (int i = 0; i < 100; i++)
        {
            a.NextNormals(out double a1, out double a2);
            b.NextNormals(out double b1, out double b2);
            c.NextNormals(out double c1, out double c2);
            Assert.Multiple(() =>
            {
                Assert.That(b1, Is.EqualTo(a1));
                Assert.That(b2, Is.EqualTo(a2));
            });
            anyDifferent |= c1 != a1;
        }
        Assert.That(anyDifferent, Is.True);
    }

    [Test]
    public void When_SeededByName_Expect_IndependentOfOtherNames()
    {
        // Hashing the name rather than a registration index is what keeps a realization stable when
        // an unrelated device is added to the netlist.
        ulong first = NoiseRandomStream.CreateSeed(0, "R1/r");
        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(0x0936BC891DC4D24Cul));
            Assert.That(NoiseRandomStream.CreateSeed(0, "R2/r"), Is.EqualTo(0x075E3E3C8B42B0D0ul));
            Assert.That(NoiseRandomStream.CreateSeed(0, "R2/r"), Is.Not.EqualTo(first));
            Assert.That(NoiseRandomStream.CreateSeed(1, "R1/r"), Is.Not.EqualTo(first));
        });
        Assert.Throws<ArgumentNullException>(() => NoiseRandomStream.CreateSeed(0, null));
    }

    [Test]
    public void When_SeededByName_Expect_DistinctStreams()
    {
        // Two sources of the same kind on two identical devices must not share a realization.
        var a = new NoiseRandomStream(0, "R1/r");
        var b = new NoiseRandomStream(0, "R2/r");
        double correlation = 0.0, variance = 0.0;
        const int n = 20000;
        for (int i = 0; i < n; i++)
        {
            a.NextNormals(out double a1, out _);
            b.NextNormals(out double b1, out _);
            correlation += a1 * b1;
            variance += a1 * a1;
        }
        Assert.Multiple(() =>
        {
            Assert.That(variance / n, Is.EqualTo(1.0).Within(0.05));
            Assert.That(correlation / n, Is.EqualTo(0.0).Within(0.05));
        });
    }

    [Test]
    public void When_NextDouble_Expect_OpenUnitInterval()
    {
        var rng = new Xoshiro256StarStar(7ul);
        double min = double.MaxValue, max = double.MinValue, sum = 0.0;
        const int n = 200000;
        for (int i = 0; i < n; i++)
        {
            double value = rng.NextDouble();
            Assert.That(value, Is.GreaterThan(0.0).And.LessThan(1.0));
            min = Math.Min(min, value);
            max = Math.Max(max, value);
            sum += value;
        }
        Assert.Multiple(() =>
        {
            Assert.That(sum / n, Is.EqualTo(0.5).Within(0.01));
            Assert.That(min, Is.LessThan(0.01));
            Assert.That(max, Is.GreaterThan(0.99));
        });
    }

    [Test]
    public void When_NextNormals_Expect_StandardNormalMoments()
    {
        var rng = new NoiseRandomStream(42ul);
        const int n = 500000;
        var values = new List<double>(2 * n);
        double correlation = 0.0;
        for (int i = 0; i < n; i++)
        {
            rng.NextNormals(out double first, out double second);
            values.Add(first);
            values.Add(second);
            correlation += first * second;
        }

        double mean = 0.0, m2 = 0.0, m4 = 0.0;
        foreach (double value in values)
            mean += value;
        mean /= values.Count;
        foreach (double value in values)
        {
            double d = value - mean;
            m2 += d * d;
            m4 += d * d * d * d;
        }
        m2 /= values.Count;
        m4 /= values.Count;

        Assert.Multiple(() =>
        {
            Assert.That(mean, Is.EqualTo(0.0).Within(0.01), "mean");
            Assert.That(m2, Is.EqualTo(1.0).Within(0.01), "variance");
            Assert.That(m4 / (m2 * m2), Is.EqualTo(3.0).Within(0.05), "kurtosis");

            // The two variates of a Box-Muller pair must be independent.
            Assert.That(correlation / n, Is.EqualTo(0.0).Within(0.01), "pair correlation");
        });
    }
}
