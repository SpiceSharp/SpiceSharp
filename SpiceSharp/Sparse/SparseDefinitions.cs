using System;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Sparse definitions for basic operations
    /// </summary>
    public static class SparseDefinitions
    {
        /// <summary>
        /// Swap integers
        /// </summary>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        internal static void SWAP(ref int first, ref int second)
        {
            int swapx = first;
            first = second;
            second = swapx;
        }

        /// <summary>
        /// Swap long integers
        /// </summary>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        internal static void SWAP(ref long first, ref long second)
        {
            long swapx = first;
            first = second;
            second = swapx;
        }

        /// <summary>
        /// Swap doubles
        /// </summary>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        internal static void SWAP(ref double first, ref double second)
        {
            double swapx = first;
            first = second;
            second = swapx;
        }

        /// <summary>
        /// Swap element values
        /// </summary>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        internal static void SWAP(ref ElementValue first, ref ElementValue second)
        {
            ElementValue swapx = first;
            first = second;
            second = swapx;
        }

        /// <summary>
        /// Swap matrix elements
        /// </summary>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        internal static void SWAP(ref MatrixElement first, ref MatrixElement second)
        {
            MatrixElement swapx = first;
            first = second;
            second = swapx;
        }
    }
}
