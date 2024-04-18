using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A system description at a data point in a <see cref="Noise"/> simulation.
    /// </summary>
    /// <seealso cref="IEquatable{T}" />
    public readonly struct NoisePoint : IEquatable<NoisePoint>
    {
        /// <summary>
        /// Gets the frequency.
        /// </summary>
        /// <value>
        /// The frequency.
        /// </value>
        public double Frequency { get; }

        /// <summary>
        /// Gets the natural logarithm of the frequency.
        /// </summary>
        /// <value>
        /// The natural logarithm of the frequency.
        /// </value>
        public double LogFrequency { get; }

        /// <summary>
        /// Gets the inverse gain squared from input to output.
        /// </summary>
        /// <value>
        /// The inverse gain squared.
        /// </value>
        /// <remarks>
        /// You can use this scaling factor to scale an output noise density
        /// to an input noise density.
        /// </remarks>
        public double InverseGainSquared { get; }

        /// <summary>
        /// Gets the natural logarithm of the inverse gain squared.
        /// </summary>
        /// <value>
        /// The natural logarithm of the inverse gain squared.
        /// </value>
        public double LogInverseGainSquared { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoisePoint"/> struct.
        /// </summary>
        /// <param name="frequency">The frequency.</param>
        /// <param name="inverseGainSquared">The inverse gain squared from input to output.</param>
        public NoisePoint(double frequency, double inverseGainSquared)
        {
            Frequency = frequency;
            LogFrequency = Math.Log(Frequency);
            InverseGainSquared = inverseGainSquared;
            LogInverseGainSquared = Math.Log(InverseGainSquared);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public readonly bool Equals(NoisePoint other)
        {
            if (!Frequency.Equals(other.Frequency))
                return false;
            if (!InverseGainSquared.Equals(other.InverseGainSquared))
                return false;
            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is NoisePoint np)
                return Equals(np);
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
            return (Frequency.GetHashCode() * 13) ^ InverseGainSquared.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(NoisePoint left, NoisePoint right) => left.Equals(right);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(NoisePoint left, NoisePoint right) => !left.Equals(right);
    }
}
