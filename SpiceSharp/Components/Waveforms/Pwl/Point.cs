﻿namespace SpiceSharp.Components
{
    /// <summary>
    /// Represents a point for a <see cref="Pwl"/>.
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// The point time.
        /// </summary>
        public readonly double Time;

        /// <summary>
        /// The point value.
        /// </summary>
        public readonly double Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="value">The value.</param>
        public Point(double time, double value)
        {
            Time = time;
            Value = value;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return (Time.GetHashCode() * 13) ^ Value.GetHashCode();
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
            if (obj is Point pt)
            {
                // This is probably only relevant for dictionaries &
                // sorted sets, so let's not work with tolerances.
                if (!Time.Equals(pt.Time))
                    return false;
                if (!Value.Equals(pt.Value))
                    return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "({0}, {1})".FormatString(Time, Value);
        }
    }
}
