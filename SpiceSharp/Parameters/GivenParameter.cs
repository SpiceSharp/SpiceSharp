using System;

namespace SpiceSharp
{
    /// <summary>
    /// This class describes a parameter that is optional. Whether or not it was specified can be
    /// found using the Given variable. It also has a default value when not specified.
    /// </summary>
    /// <remarks>
    /// This class is related to nullable types, but instead of assigning/returning null, we still
    /// want these parameters to return a default value.
    /// </remarks>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IEquatable{T}"/>
    public readonly struct GivenParameter<T> : IEquatable<T>, IEquatable<GivenParameter<T>>
    {
        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <value>
        /// The value of the parameter.
        /// </value>
        public T Value { get; }

        /// <summary>
        /// Gets whether or not the parameter was specified by the user.
        /// </summary>
        /// <value>
        /// Whether or not the parameter is given.
        /// </value>
        public bool Given { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GivenParameter{T}"/> class.
        /// </summary>
        public GivenParameter(T value, bool given = true)
        {
            Value = value;
            Given = given;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override readonly string ToString()
        {
            if (Given)
                return "{0} (set)".FormatString(Value);
            return "{0} (not set)".FormatString(Value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="GivenParameter{T}"/> to the base value type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator T(GivenParameter<T> parameter) => parameter.Value;

        /// <summary>
        /// Performs an implicit conversion from the base value type to <see cref="GivenParameter{T}"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator GivenParameter<T>(T value) => new(value);

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object obj)
        {
            if (obj is GivenParameter<T> gp)
            {
                if (!gp.Value.Equals(Value))
                    return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified value is equal to this instance.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the specified value is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public readonly bool Equals(T value) => Value.Equals(value);

        /// <summary>
        /// Determines whether the specified value is equal to this instance.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the specified value is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public readonly bool Equals(GivenParameter<T> value)
        {
            if (!Value.Equals(value.Value))
                return false;
            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        /// <remarks>
        /// The hash code is not based on whether or not the value is given.
        /// </remarks>
        public override readonly int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(GivenParameter<T> left, GivenParameter<T> right) => left.Equals(right);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(GivenParameter<T> left, GivenParameter<T> right) => !(left == right);
    }
}