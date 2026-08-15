using NUnit.Framework;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpTest.Simulations;

/// <summary>
/// Tests for <see cref="TimeNoisePoint"/>, the band-limit shaping coefficients of a transient noise
/// analysis. The point is a pure function of the normalized timestep and the shaping order, so these
/// are plain unit tests without a circuit or a simulation in them.
/// </summary>
[TestFixture]
public class TimeNoisePointTests
{
    /// <summary>
    /// The conditional covariance of the two-pole shaping filter, in closed form. Accurate as long as
    /// the timestep is not so small that the terms cancel.
    /// </summary>
    private static void ConditionalCovariance(double z, out double q11, out double q12, out double q22)
    {
        double m = Math.Exp(-2.0 * z);
        q11 = 1.0 - m;
        q12 = 0.5 - (m * (z + 0.5));
        q22 = 0.5 - (m * ((z * z) + z + 0.5));
    }

    /// <summary>
    /// Reconstructs the conditional covariance from the Cholesky factor of a point.
    /// </summary>
    private static void Reconstruct(TimeNoisePoint point, out double q11, out double q12, out double q22)
    {
        q11 = point.L11 * point.L11;
        q12 = point.L11 * point.L21;
        q22 = (point.L21 * point.L21) + (point.L22 * point.L22);
    }

    /// <summary>
    /// Advances a covariance through a point: P = Phi * P * Phi' + L * L'.
    /// </summary>
    private static void Advance(TimeNoisePoint point, ref double p11, ref double p12, ref double p22)
    {
        double a = point.Decay, b = point.Coupling;
        Reconstruct(point, out double q11, out double q12, out double q22);
        double n11 = a * a * p11;
        double n12 = a * ((b * p11) + (a * p12));
        double n22 = (b * b * p11) + (2.0 * a * b * p12) + (a * a * p22);
        p11 = n11 + q11;
        p12 = n12 + q12;
        p22 = n22 + q22;
    }

    [Test]
    public void When_Order1_Expect_ExactTransitionDensity()
    {
        for (double z = 1e-6; z < 1e3; z *= 1.7)
        {
            var point = new TimeNoisePoint(z, 1);
            Assert.Multiple(() =>
            {
                Assert.That(point.Decay, Is.EqualTo(Math.Exp(-z)).Within(1e-10).Percent, $"decay at z={z}");
                Assert.That(point.L11 * point.L11, Is.EqualTo(1.0 - Math.Exp(-2.0 * z)).Within(1e-6).Percent, $"variance at z={z}");

                // The second row does not exist at order 1.
                Assert.That(point.Coupling, Is.Zero, $"coupling at z={z}");
                Assert.That(point.L21, Is.Zero, $"L21 at z={z}");
                Assert.That(point.L22, Is.Zero, $"L22 at z={z}");
            });
        }
    }

    [Test]
    public void When_Order2_Expect_ExactPropagator()
    {
        // exp(A*dt) = exp(-z) * [[1, 0], [z, 1]]
        for (double z = 1e-6; z < 1e2; z *= 1.7)
        {
            var point = new TimeNoisePoint(z, 2);
            Assert.Multiple(() =>
            {
                Assert.That(point.Decay, Is.EqualTo(Math.Exp(-z)).Within(1e-10).Percent, $"decay at z={z}");
                Assert.That(point.Coupling, Is.EqualTo(z * Math.Exp(-z)).Within(1e-10).Percent, $"coupling at z={z}");
            });
        }
    }

    [Test]
    public void When_Order2_Expect_CholeskyOfConditionalCovariance()
    {
        // Only from the series crossover upward, where the closed form is trustworthy in double
        // precision. Below it, the series is checked by the crossover and Chapman-Kolmogorov tests.
        for (double z = TimeNoisePoint.SeriesLimit; z < 1e2; z *= 1.3)
        {
            var point = new TimeNoisePoint(z, 2);
            ConditionalCovariance(z, out double q11, out double q12, out double q22);
            Reconstruct(point, out double r11, out double r12, out double r22);
            Assert.Multiple(() =>
            {
                Assert.That(r11, Is.EqualTo(q11).Within(1e-7).Percent, $"Q11 at z={z}");
                Assert.That(r12, Is.EqualTo(q12).Within(1e-7).Percent, $"Q12 at z={z}");
                Assert.That(r22, Is.EqualTo(q22).Within(1e-7).Percent, $"Q22 at z={z}");
            });
        }
    }

    [Test]
    public void When_Order2_AtSeriesCrossover_Expect_ContinuousFromBothSides()
    {
        // Straddle the crossover so that the two branches are compared against each other. This is
        // what pins the series of the residual Q22 - Q12^2/Q11, a cancellation between two terms of
        // order z^3 that neither branch can be checked against on its own.
        var series = new TimeNoisePoint(TimeNoisePoint.SeriesLimit * (1.0 - 1e-12), 2);
        var closedForm = new TimeNoisePoint(TimeNoisePoint.SeriesLimit * (1.0 + 1e-12), 2);
        Assert.Multiple(() =>
        {
            Assert.That(series.L11, Is.EqualTo(closedForm.L11).Within(1e-6).Percent);
            Assert.That(series.L21, Is.EqualTo(closedForm.L21).Within(1e-6).Percent);
            Assert.That(series.L22, Is.EqualTo(closedForm.L22).Within(1e-6).Percent);
        });
    }

    [Test]
    public void When_Order2_BelowSeriesCrossover_Expect_ClosedForm()
    {
        // One decade below the crossover the closed form still carries about eight significant digits
        // on the residual, so it is a usable independent reference for the series branch.
        const double z = 1e-3;
        var point = new TimeNoisePoint(z, 2);
        Assert.That(point.Z, Is.LessThan(TimeNoisePoint.SeriesLimit));

        ConditionalCovariance(z, out double q11, out double q12, out double q22);
        Reconstruct(point, out double r11, out double r12, out double r22);
        Assert.Multiple(() =>
        {
            Assert.That(r11, Is.EqualTo(q11).Within(1e-6).Percent);
            Assert.That(r12, Is.EqualTo(q12).Within(1e-6).Percent);
            Assert.That(r22, Is.EqualTo(q22).Within(1e-4).Percent);
        });
    }

    [Test]
    public void When_Order2_SmallStep_Expect_LeadingOrderCovariance()
    {
        // Q11 -> 2z, Q12 -> z^2, and the Cholesky residual -> z^3/6.
        foreach (double z in new[] { 1e-4, 1e-6, 1e-8 })
        {
            var point = new TimeNoisePoint(z, 2);
            Assert.Multiple(() =>
            {
                Assert.That(point.L11 * point.L11 / (2.0 * z), Is.EqualTo(1.0).Within(2.0 * z), $"Q11 at z={z}");
                Assert.That(point.L11 * point.L21 / (z * z), Is.EqualTo(1.0).Within(2.0 * z), $"Q12 at z={z}");
                Assert.That(point.L22 * point.L22 / (z * z * z / 6.0), Is.EqualTo(1.0).Within(2.0 * z), $"residual at z={z}");
            });
        }
    }

    [Test]
    [TestCase(1, 110)]
    [TestCase(1, 10000)]
    [TestCase(2, 110)]
    [TestCase(2, 10000)]
    public void When_CovarianceAccumulatedOverSmallSteps_Expect_SingleStepCovariance(int order, int steps)
    {
        // Chapman-Kolmogorov: the covariance accumulated from zero over many small steps must equal
        // the conditional covariance of the single large step. The small steps are inside the series
        // branch while the reference is in the closed-form branch, which is what makes this the
        // sharpest check on the series.
        const double total = 1.0;
        var small = new TimeNoisePoint(total / steps, order);
        Assert.That(small.Z, Is.LessThan(TimeNoisePoint.SeriesLimit));

        double p11 = 0.0, p12 = 0.0, p22 = 0.0;
        for (int i = 0; i < steps; i++)
            Advance(small, ref p11, ref p12, ref p22);

        ConditionalCovariance(total, out double q11, out double q12, out double q22);
        Assert.Multiple(() =>
        {
            Assert.That(p11, Is.EqualTo(q11).Within(1e-8).Percent);
            if (order > 1)
            {
                Assert.That(p12, Is.EqualTo(q12).Within(1e-8).Percent);
                Assert.That(p22, Is.EqualTo(q22).Within(1e-8).Percent);
            }
        });
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void When_CovariancePropagatedOverIrregularSteps_Expect_Stationary(int order)
    {
        // The stationary covariance [[1, 1/2], [1/2, 1/2]] must be a fixed point of the update for
        // *any* step. A wrong Cholesky factor, propagator or series makes it drift.
        double p11 = 1.0, p12 = 0.5, p22 = 0.5;
        double worst = 0.0;
        var rng = new NoiseRandomStream(1234ul);
        for (int i = 0; i < 20000; i++)
        {
            // Log-uniform over eight decades of normalized timestep, straddling the series crossover.
            double z = Math.Pow(10.0, 2.0 - (8.0 * rng.NextDouble()));
            Advance(new TimeNoisePoint(z, order), ref p11, ref p12, ref p22);
            worst = Math.Max(worst, Math.Abs(p11 - 1.0));
            if (order > 1)
            {
                worst = Math.Max(worst, Math.Abs(p12 - 0.5));
                worst = Math.Max(worst, Math.Abs(p22 - 0.5));
            }
        }
        Assert.That(worst, Is.LessThan(1e-9));
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void When_ZeroStep_Expect_Identity(int order)
    {
        var point = new TimeNoisePoint(0.0, order);
        Assert.Multiple(() =>
        {
            Assert.That(point.Decay, Is.EqualTo(1.0));
            Assert.That(point.Coupling, Is.Zero);
            Assert.That(point.L11, Is.Zero);
            Assert.That(point.L21, Is.Zero);
            Assert.That(point.L22, Is.Zero);
        });

        // A zero step must leave the state exactly where it was.
        double state1 = 0.3, state2 = -0.7;
        double output = point.Propagate(ref state1, ref state2, 1.0, -2.0);
        Assert.Multiple(() =>
        {
            Assert.That(state1, Is.EqualTo(0.3));
            Assert.That(state2, Is.EqualTo(-0.7));
            Assert.That(output, Is.EqualTo(order == 1 ? 0.3 : Math.Sqrt(2.0) * -0.7).Within(1e-15));
        });
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void When_InfiniteStep_Expect_Stationary(int order)
    {
        var stationary = TimeNoisePoint.Stationary(order);
        Assert.Multiple(() =>
        {
            Assert.That(stationary.Decay, Is.Zero);
            Assert.That(stationary.Coupling, Is.Zero);
            Assert.That(stationary.L11, Is.EqualTo(1.0));
            Assert.That(stationary.L21, Is.EqualTo(order == 1 ? 0.0 : 0.5));
            Assert.That(stationary.L22, Is.EqualTo(order == 1 ? 0.0 : 0.5));
        });

        // A very long but finite step must give the same thing.
        var far = new TimeNoisePoint(1e3, order);
        Assert.Multiple(() =>
        {
            Assert.That(far.Decay, Is.Zero);
            Assert.That(far.Coupling, Is.Zero);
            Assert.That(far.L11, Is.EqualTo(stationary.L11));
            Assert.That(far.L21, Is.EqualTo(stationary.L21));
            Assert.That(far.L22, Is.EqualTo(stationary.L22));
        });

        // Propagating a zero state through it lands exactly on the stationary covariance, which is
        // how a shaping state is initialized without a startup transient.
        double p11 = 0.0, p12 = 0.0, p22 = 0.0;
        Advance(stationary, ref p11, ref p12, ref p22);
        Assert.Multiple(() =>
        {
            Assert.That(p11, Is.EqualTo(1.0).Within(1e-15));
            Assert.That(p12, Is.EqualTo(order == 1 ? 0.0 : 0.5).Within(1e-15));
            Assert.That(p22, Is.EqualTo(order == 1 ? 0.0 : 0.5).Within(1e-15));
        });
    }

    [Test]
    public void When_ExtremeStep_Expect_Finite()
    {
        foreach (double z in new[] { 0.0, 1e-300, 1e-100, 1e-16, 1.0, 700.0, 800.0, double.PositiveInfinity })
        {
            for (int order = 1; order <= TimeNoisePoint.MaximumOrder; order++)
            {
                var point = new TimeNoisePoint(z, order);
                Assert.Multiple(() =>
                {
                    Assert.That(point.Decay, Is.InRange(0.0, 1.0), $"decay at z={z}, order={order}");
                    Assert.That(point.Coupling, Is.GreaterThanOrEqualTo(0.0), $"coupling at z={z}, order={order}");
                    Assert.That(point.L11, Is.InRange(0.0, 1.0), $"L11 at z={z}, order={order}");
                    Assert.That(point.L21, Is.InRange(0.0, 1.0), $"L21 at z={z}, order={order}");
                    Assert.That(point.L22, Is.InRange(0.0, 1.0), $"L22 at z={z}, order={order}");
                });
            }
        }
    }

    [Test]
    public void When_InvalidArguments_Expect_Exception()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new TimeNoisePoint(-1e-30, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new TimeNoisePoint(1.0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new TimeNoisePoint(1.0, TimeNoisePoint.MaximumOrder + 1));
        });
    }

    [Test]
    public void When_Compared_Expect_EqualityByStepAndOrder()
    {
        var a = new TimeNoisePoint(0.25, 2);
        var b = new TimeNoisePoint(0.25, 2);
        var c = new TimeNoisePoint(0.25, 1);
        var d = new TimeNoisePoint(0.5, 2);
        Assert.Multiple(() =>
        {
            Assert.That(a == b, Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
            Assert.That(a != c, Is.True);
            Assert.That(a != d, Is.True);
            Assert.That(a.Equals((object)b), Is.True);
            Assert.That(a.Equals("not a point"), Is.False);
        });
    }
}
