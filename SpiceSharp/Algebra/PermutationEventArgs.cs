using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Event arguments that can be used when elements are permutated.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class PermutationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the first index.
        /// </summary>
        /// <value>
        /// The first index.
        /// </value>
        public int Index1 { get; }

        /// <summary>
        /// Gets the second index.
        /// </summary>
        /// <value>
        /// The second index.
        /// </value>
        public int Index2 { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermutationEventArgs" /> class.
        /// </summary>
        /// <param name="index1">The first index.</param>
        /// <param name="index2">The second index.</param>
        public PermutationEventArgs(int index1, int index2)
        {
            Index1 = index1;
            Index2 = index2;
        }
    }
}
