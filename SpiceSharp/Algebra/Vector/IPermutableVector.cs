using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a vector that is also permutable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="IVector{T}" />
    public interface IPermutableVector<T> : IVector<T> where T : IFormattable
    {
        /// <summary>
        /// Occurs when two elements have swapped.
        /// </summary>
        event EventHandler<PermutationEventArgs> ElementsSwapped;

        /// <summary>
        /// Swaps two elements in the vector.
        /// </summary>
        /// <param name="index1">The first index.</param>
        /// <param name="index2">The second index.</param>
        void SwapElements(int index1, int index2);
    }
}
