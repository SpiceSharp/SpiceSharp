using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.CommonBehaviors
{
    /// <summary>
    /// A structure containing the unknowns for a one-port.
    /// </summary>
    /// <remarks>
    /// A one-port is a device with two pins, where the current into one pin
    /// is equal to the current out of the other.
    /// </remarks>
    /// <typeparam name="T">The base value type.</typeparam>
    public readonly struct OnePort<T>
    {
        /// <summary>
        /// The positive node.
        /// </summary>
        public readonly IVariable<T> Positive;

        /// <summary>
        /// The negative node.
        /// </summary>
        public readonly IVariable<T> Negative;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnePort{T}"/> struct.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="NodeMismatchException">Thrown if <paramref name="context"/> does not define exactly 2 nodes.</exception>
        public OnePort(IVariableFactory<IVariable<T>> factory, IComponentBindingContext context)
        {
            context
                .ThrowIfNull(nameof(context))
                .Nodes.CheckNodes(2);
            Positive = factory.GetSharedVariable(context.Nodes[0]);
            Negative = factory.GetSharedVariable(context.Nodes[1]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnePort{T}"/> struct.
        /// </summary>
        /// <param name="positive">The positive.</param>
        /// <param name="negative">The negative.</param>
        public OnePort(IVariable<T> positive, IVariable<T> negative)
        {
            Positive = positive.ThrowIfNull(nameof(positive));
            Negative = negative.ThrowIfNull(nameof(negative));
        }

        /// <summary>
        /// Gets the matrix locations in the order (Positive, Positive), (Positive, Negative), (Negative, Positive), (Negative, Negative).
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>An array of matrix locations.</returns>
        public readonly MatrixLocation[] GetMatrixLocations(IVariableMap map)
        {
            int pos = map[Positive];
            int neg = map[Negative];
            return
            [
                new MatrixLocation(pos, pos),
                new MatrixLocation(pos, neg),
                new MatrixLocation(neg, pos),
                new MatrixLocation(neg, neg)
            ];
        }

        /// <summary>
        /// Gets the right-hand-side indices, in the order Positive, then Negative.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>An array with indices.</returns>
        public readonly int[] GetRhsIndices(IVariableMap map)
        {
            int pos = map[Positive];
            int neg = map[Negative];
            return [pos, neg];
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object obj)
        {
            if (obj is OnePort<T> op)
            {
                if (!Positive.Equals(op.Positive))
                    return false;
                if (!Negative.Equals(op.Negative))
                    return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override readonly int GetHashCode() => (Positive.GetHashCode() * 13) ^ Negative.GetHashCode();

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(OnePort<T> left, OnePort<T> right) => left.Equals(right);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(OnePort<T> left, OnePort<T> right) => !left.Equals(right);
    }
}
