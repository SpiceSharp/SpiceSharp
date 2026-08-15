using System;

namespace SpiceSharp.Simulations;

/// <summary>
/// The band-limit shaping coefficients for a single probed timestep of a transient noise analysis.
/// </summary>
/// <remarks>
/// Contains pre-computed shared values that are used for noise transient simulations.
/// </remarks>
/// <seealso cref="IEquatable{T}"/>
public readonly struct TimeNoisePoint : IEquatable<TimeNoisePoint>
{
    /// <summary>
    /// The maximum number of shaping poles that is supported.
    /// </summary>
    public const int MaximumOrder = 2;

    /// <summary>
    /// The value of <see cref="Z"/> below which the conditional covariance is evaluated from a
    /// series expansion instead of from its closed form.
    /// </summary>
    /// <remarks>
    /// The closed form is a difference of nearly equal quantities for small steps. Both branches are
    /// accurate to better than 1e-9 relative at this crossover.
    /// </remarks>
    public const double SeriesLimit = 1e-2;

    /// <summary>
    /// Gets the number of shaping poles.
    /// </summary>
    /// <value>
    /// The number of shaping poles.
    /// </value>
    public int Order { get; }

    /// <summary>
    /// Gets the normalized timestep <c>z = 2*pi*fmax*dt</c>.
    /// </summary>
    /// <value>
    /// The normalized timestep.
    /// </value>
    public double Z { get; }

    /// <summary>
    /// Gets the decay of the shaping filter over the timestep, <c>exp(-z)</c>.
    /// </summary>
    /// <value>
    /// The decay.
    /// </value>
    public double Decay { get; }

    /// <summary>
    /// Gets the off-diagonal element of the propagator, <c>z*exp(-z)</c>. It is 0 at order 1.
    /// </summary>
    /// <value>
    /// The coupling from the first shaping state to the second one.
    /// </value>
    public double Coupling { get; }

    /// <summary>
    /// Gets the first diagonal element of the Cholesky factor of the conditional covariance.
    /// </summary>
    /// <value>
    /// The Cholesky factor element (1,1).
    /// </value>
    public double L11 { get; }

    /// <summary>
    /// Gets the off-diagonal element of the Cholesky factor of the conditional covariance. It is 0
    /// at order 1.
    /// </summary>
    /// <value>
    /// The Cholesky factor element (2,1).
    /// </value>
    public double L21 { get; }

    /// <summary>
    /// Gets the second diagonal element of the Cholesky factor of the conditional covariance. It is
    /// 0 at order 1.
    /// </summary>
    /// <value>
    /// The Cholesky factor element (2,2).
    /// </value>
    public double L22 { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeNoisePoint"/> struct.
    /// </summary>
    /// <param name="z">The normalized timestep <c>2*pi*fmax*dt</c>. May be infinite, which describes
    /// the stationary distribution of the shaping filter.</param>
    /// <param name="order">The number of shaping poles.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="z"/> is negative or not a number.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="order"/> is not between
    /// 1 and <see cref="MaximumOrder"/>.</exception>
    public TimeNoisePoint(double z, int order)
    {
        z.GreaterThanOrEquals(nameof(z), 0.0);
        if (order < 1 || order > MaximumOrder)
            throw new ArgumentOutOfRangeException(nameof(order));
        Z = z;
        Order = order;
        Decay = Math.Exp(-z);
        Coupling = (order > 1) && (Decay > 0.0) ? z * Decay : 0.0;

        // The conditional covariance Q(z) of the two-pole shaping filter. Order 1 is order 2 with the
        // second row dropped, so Q11 is the same expression for both.
        double q11, q12, residual;
        if (z < SeriesLimit)
        {
            // The closed form below cancels for small steps. The residual is the dangerous one: it is
            // a cancellation between two terms of order z^3.
            q11 = 2.0 * z * (1.0 + (z * (-1.0 + (z * ((2.0 / 3.0) + (z * ((-1.0 / 3.0) + (z * ((2.0 / 15.0) + (z * (-2.0 / 45.0)))))))))));
            q12 = z * z * (1.0 + (z * ((-4.0 / 3.0) + (z * (1.0 + (z * ((-8.0 / 15.0) + (z * ((2.0 / 9.0) + (z * (-8.0 / 105.0)))))))))));
            residual = z * z * z / 6.0 * (1.0 + (z * (-1.0 + (z * ((7.0 / 15.0) + (z * ((-2.0 / 15.0) + (z * (2.0 / 63.0)))))))));
        }
        else
        {
            double m = Decay * Decay;
            if (m > 0.0)
            {
                q11 = 1.0 - m;
                q12 = 0.5 - (m * (z + 0.5));
                double q22 = 0.5 - (m * ((z * z) + z + 0.5));
                residual = q22 - (q12 * q12 / q11);
            }
            else
            {
                // The step is long enough for the shaping filter to have reached its stationary
                // distribution, which is [[1, 1/2], [1/2, 1/2]].
                q11 = 1.0;
                q12 = 0.5;
                residual = 0.25;
            }
        }

        L11 = Math.Sqrt(q11);
        if (order > 1)
        {
            L21 = L11 > 0.0 ? q12 / L11 : 0.0;
            L22 = Math.Sqrt(Math.Max(residual, 0.0));
        }
        else
        {
            L21 = 0.0;
            L22 = 0.0;
        }
    }

    /// <summary>
    /// Creates a point that describes the stationary distribution of the shaping filter. Propagating
    /// a zero state through it initializes that state without a startup transient.
    /// </summary>
    /// <param name="order">The number of shaping poles.</param>
    /// <returns>
    /// The stationary point.
    /// </returns>
    public static TimeNoisePoint Stationary(int order) => new(double.PositiveInfinity, order);

    /// <summary>
    /// Advances a shaping state over the timestep described by this point, and returns the
    /// unit-variance output of the shaping filter.
    /// </summary>
    /// <param name="state1">The first shaping state.</param>
    /// <param name="state2">The second shaping state. It is left untouched at order 1.</param>
    /// <param name="normal1">The first standard normal variate.</param>
    /// <param name="normal2">The second standard normal variate. It is unused at order 1.</param>
    /// <returns>
    /// The unit-variance output of the shaping filter. Multiply by the stationary standard deviation
    /// of a noise source to obtain its current.
    /// </returns>
    /// <remarks>
    /// The update is exact rather than a discretization of a stochastic differential equation, so an
    /// irregular or arbitrarily varying timestep costs nothing in accuracy.
    /// </remarks>
    public readonly double Propagate(ref double state1, ref double state2, double normal1, double normal2)
    {
        if (Order < 2)
        {
            state1 = (Decay * state1) + (L11 * normal1);
            return state1;
        }

        // The second state is advanced using the first state before it is itself advanced.
        state2 = (Coupling * state1) + (Decay * state2) + (L21 * normal1) + (L22 * normal2);
        state1 = (Decay * state1) + (L11 * normal1);

        // The second state has a stationary variance of 1/2.
        return Constants.Root2 * state2;
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public readonly bool Equals(TimeNoisePoint other)
    {
        if (Order != other.Order)
            return false;
        if (!Z.Equals(other.Z))
            return false;
        return true;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override readonly bool Equals(object obj)
    {
        if (obj is TimeNoisePoint tnp)
            return Equals(tnp);
        return false;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    public override readonly int GetHashCode()
    {
        return (Z.GetHashCode() * 13) ^ Order.GetHashCode();
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator ==(TimeNoisePoint left, TimeNoisePoint right) => left.Equals(right);

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(TimeNoisePoint left, TimeNoisePoint right) => !left.Equals(right);
}
