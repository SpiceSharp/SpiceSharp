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
    public struct OnePort<T> where T : IFormattable
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
        public OnePort(IVariableFactory<IVariable<T>> factory, IComponentBindingContext context)
        {
            context.Nodes.CheckNodes(2);
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
        public MatrixLocation[] GetMatrixLocations(IVariableMap map)
        {
            int pos = map[Positive];
            int neg = map[Negative];
            return new[]
            {
                new MatrixLocation(pos, pos),
                new MatrixLocation(pos, neg),
                new MatrixLocation(neg, pos),
                new MatrixLocation(neg, neg)
            };
        }

        /// <summary>
        /// Gets the right-hand-side indices, in the order Positive, then Negative.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>An array with indices.</returns>
        public int[] GetRhsIndices(IVariableMap map)
        {
            int pos = map[Positive];
            int neg = map[Negative];
            return new[] { pos, neg };
        }
    }
}
