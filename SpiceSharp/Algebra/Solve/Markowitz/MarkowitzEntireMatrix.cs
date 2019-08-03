using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// Markowitz-count based strategy for finding a pivot. Search the complete submatrix.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public class MarkowitzEntireMatrix<T> : MarkowitzSearchStrategy<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Constants
        /// </summary>
        private const int TiesMultiplier = 5;

        /// <summary>
        /// Find a pivot in a matrix.
        /// </summary>
        /// <param name="markowitz">The Markowitz pivot strategy.</param>
        /// <param name="matrix">The matrix</param>
        /// <param name="eliminationStep">The current elimination step.</param>
        /// <returns>
        /// The pivot element, or null if no pivot was found.
        /// </returns>
        public override MatrixElement<T> FindPivot(Markowitz<T> markowitz, SparseMatrix<T> matrix, int eliminationStep)
        {
            markowitz.ThrowIfNull(nameof(markowitz));
            matrix.ThrowIfNull(nameof(matrix));
            if (eliminationStep < 1)
                throw new ArgumentException("Invalid elimination step");

            MatrixElement<T> chosen = null;
            var minMarkowitzProduct = long.MaxValue;
            double largestMagnitude = 0.0, acceptedRatio = 0.0;
            MatrixElement<T> largestElement = null;
            var ties = 0;

            // Start search of matrix on column by column basis
            for (var i = eliminationStep; i <= matrix.Size; i++)
            {
                // Find the biggest magnitude in the column for checking valid pivots later
                var largest = 0.0;
                var element = matrix.GetLastInColumn(i);
                while (element != null && element.Row >= eliminationStep)
                {
                    largest = Math.Max(largest, markowitz.Magnitude(element.Value));
                    element = element.Above;
                }
                if (largest.Equals(0.0))
                    continue;

                // Restart search for a pivot
                element = matrix.GetLastInColumn(i);
                while (element != null && element.Row >= eliminationStep)
                {
                    // Find the magnitude and Markowitz product
                    var magnitude = markowitz.Magnitude(element.Value);
                    var product = markowitz.RowCount(element.Row) * markowitz.ColumnCount(element.Column);

                    // In the case no valid pivot is available, at least return the largest element
                    if (magnitude > largestMagnitude)
                    {
                        largestElement = element;
                        largestMagnitude = magnitude;
                    }

                    // test to see if the element is acceptable as a pivot candidate
                    if (product <= minMarkowitzProduct
                        && magnitude > markowitz.RelativePivotThreshold * largest
                        && magnitude > markowitz.AbsolutePivotThreshold)
                    {
                        // Test to see if the element has the lowest Markowitz product yet found,
                        // or whether it is tied with an element found earlier
                        if (product < minMarkowitzProduct)
                        {
                            // Notice strict inequality
                            // This is a new smallest Markowitz product
                            chosen = element;
                            minMarkowitzProduct = product;
                            acceptedRatio = largest / magnitude;
                            ties = 0;
                        }
                        else
                        {
                            // This case handles Markowitz ties
                            ties++;
                            var ratio = largest / magnitude;
                            if (ratio < acceptedRatio)
                            {
                                chosen = element;
                                acceptedRatio = ratio;
                            }
                            if (ties >= minMarkowitzProduct * TiesMultiplier)
                                return chosen;
                        }
                    }

                    element = element.Above;
                }
            }

            // If a valid pivot was found, return it
            if (chosen != null)
                return chosen;

            // Singular matrix
            // If we can't find it while searching the entire matrix, then we definitely have a singular matrix...
            if (largestElement == null || largestElement.Value.Equals(0.0))
                throw new SingularException(eliminationStep);
            return largestElement;
        }
    }
}
