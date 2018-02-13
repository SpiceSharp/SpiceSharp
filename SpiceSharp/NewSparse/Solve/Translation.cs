using System;

namespace SpiceSharp.NewSparse.Solve
{
    /// <summary>
    /// Can map external to internal indices and vice-versa
    /// </summary>
    public class Translation
    {
        /// <summary>
        /// Maps
        /// </summary>
        int[] extToInt;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">Number</param>
        public Translation(int size)
        {
            extToInt = new int[size];
            for (int i = 0; i < size; i++)
                extToInt[i] = i;
        }

        /// <summary>
        /// Indexer will map external indices to internal indices
        /// </summary>
        /// <param name="index">External index</param>
        /// <returns></returns>
        public int this[int index]
        {
            get
            {
                // Zero is mapped to zero
                if (index == 0)
                    return 0;
                if (index >= extToInt.Length)
                    throw new ArgumentException("Invalid index {0}".FormatString(index));
                return extToInt[index];
            }
        }

        /// <summary>
        /// Swap two (internal) indices
        /// </summary>
        /// <param name="index1">Index 1</param>
        /// <param name="index2">Index 2</param>
        public void Swap(int index1, int index2)
        {
            // The extToInt indices need to be swapped
            var tmp = extToInt[index1];
            extToInt[index1] = extToInt[index2];
            extToInt[index2] = tmp;
        }

        /// <summary>
        /// Scramble a vector
        /// </summary>
        /// <typeparam name="T">Base type</typeparam>
        /// <param name="source">Source</param>
        /// <param name="target">Target</param>
        public void Scramble<T>(DenseVector<T> source, DenseVector<T> target) where T : IFormattable
        {
            for (int i = 1; i < extToInt.Length; i++)
                target[extToInt[i]] = source[i];
        }

        /// <summary>
        /// Unscramble a vector
        /// </summary>
        /// <typeparam name="T">Base type</typeparam>
        /// <param name="source">Source</param>
        /// <param name="target">Target</param>
        public void Unscramble<T>(T[] source, DenseVector<T> target) where T : IFormattable
        {
            for (int i = 1; i < extToInt.Length; i++)
                target[i] = source[extToInt[i]];
        }
    }
}
