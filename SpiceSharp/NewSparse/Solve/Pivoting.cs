using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.NewSparse
{
    /// <summary>
    /// Class for finding pivots in a matrix
    /// </summary>
    public class Pivoting
    {
        /// <summary>
        /// Constants
        /// </summary>
        const int TiesMultiplier = 5;

        /// <summary>
        /// Absolute threshold for pivoting
        /// </summary>
        public double AbsoluteThreshold { get; set; } = 1e-13;

        /// <summary>
        /// Relative threshold for pivoting
        /// </summary>
        public double RelativeThreshold { get; set; } = 1e-3;

        /// <summary>
        /// Markowitz properties
        /// </summary>
        int[] markowitzRow;
        int[] markowitzCol;
        long[] markowitzProd;
        bool[] DoComplexDirect;
        bool[] DoRealDirect;

        /// <summary>
        /// Find the largest element in the column not looking at the pivot
        /// </summary>
        /// <typeparam name="T">Base type</typeparam>
        /// <param name="pivot">Pivot</param>
        /// <returns></returns>
        public double LargestInColumn<T>(MatrixIterator<T> pivot)
        {
            double magnitude, largest = 0;

            // Search the column for the largest element beginning at element
            var element = pivot.BranchDown();
            while (element.Element != null)
            {
                magnitude = element.Element.Magnitude;
                if (magnitude > largest)
                    largest = magnitude;
                element.MoveDown();
            }
            return largest;
        }
    }
}
