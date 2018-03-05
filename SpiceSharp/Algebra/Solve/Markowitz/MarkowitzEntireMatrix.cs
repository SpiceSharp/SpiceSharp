using System;

namespace SpiceSharp.Algebra.Solve.Markowitz
{
    /// <summary>
    /// A pivot strategy that can be used for any generic type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MarkowitzEntireMatrix<T> : MarkowitzSearchStrategy<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Constants
        /// </summary>
        private const int TiesMultiplier = 5;

        /// <summary>
        /// Search the entire matrix for a suitable pivot
        /// In order to preserve sparsity, Markowitz counts are used to find the largest valid
        /// pivot with the smallest number of elements.
        /// </summary>
        /// <param name="markowitz">Markowitz object</param>
        /// <param name="matrix">Matrix</param>
        /// <param name="step">Step</param>
        /// <returns></returns>
        public override MatrixElement<T> FindPivot(Markowitz<T> markowitz, SparseMatrix<T> matrix, int step)
        {
            if (markowitz == null)
                throw new ArgumentNullException(nameof(markowitz));
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (step < 1)
                throw new ArgumentException("Invalid elimination step");

            MatrixElement<T> chosen = null;
            long minMarkowitzProduct = long.MaxValue;
            double largestMagnitude = 0.0, acceptedRatio = 0.0;
            MatrixElement<T> largestElement = null;
            int ties = 0;

            // Start search of matrix on column by column basis
            for (int i = step; i <= matrix.Size; i++)
            {
                // Find the biggest magnitude in the column for checking valid pivots later
                double largest = 0.0;
                var element = matrix.GetLastInColumn(i);
                while (element != null && element.Row >= step)
                {
                    largest = Math.Max(largest, markowitz.Magnitude(element.Value));
                    element = element.Above;
                }
                if (largest.Equals(0.0))
                    continue;

                // Restart search for a pivot
                element = matrix.GetLastInColumn(i);
                while (element != null && element.Row >= step)
                {
                    // Find the magnitude and Markowitz product
                    double magnitude = markowitz.Magnitude(element.Value);
                    int product = markowitz.RowCount(element.Row) * markowitz.ColumnCount(element.Column);

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
                            double ratio = largest / magnitude;
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
                throw new SingularException(step);
            return largestElement;
        }
    }
}
