using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// Markowitz-based pivot search: Quickly search the diagonal
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MarkowitzQuickDiagonal<T> : MarkowitzSearchStrategy<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Find a pivot by quickly searching the diagonal
        /// </summary>
        /// <param name="markowitz">Markowitz</param>
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

            int minMarkowitzProduct = int.MaxValue;
            MatrixElement<T> chosen = null;

            for (int i = step; i <= matrix.Size; i++)
            {
                // Skip diagonal elements with a Markowitz product worse than already found
                int product = markowitz.Product(i);
                if (product >= minMarkowitzProduct)
                    continue;

                // Get the diagonal item
                var diagonal = matrix.GetDiagonalElement(i);
                if (diagonal == null)
                    continue;

                // Get the magnitude
                double magnitude = markowitz.Magnitude(diagonal.Value);
                if (magnitude <= markowitz.AbsolutePivotThreshold)
                    continue;

                if (product == 1)
                {
                    // Find the off-diagonal elements
                    var otherInRow = diagonal.Right ?? diagonal.Left;
                    var otherInColumn = diagonal.Below ?? diagonal.Above;

                    // Accept diagonal as pivot if diagonal is larger than off-diagonals and
                    // the off-diagonals are placed symmetrically
                    if (otherInRow != null && otherInColumn != null)
                    {
                        if (otherInRow.Column == otherInColumn.Row)
                        {
                            double largest = Math.Max(
                                markowitz.Magnitude(otherInRow.Value),
                                markowitz.Magnitude(otherInColumn.Value));
                            if (magnitude >= largest)
                                return diagonal;
                        }
                    }
                }

                minMarkowitzProduct = markowitz.Product(i);
                chosen = diagonal;
            }

            // No decision was made yet, so check again here
            if (chosen != null)
            {
                // Find the biggest element above and below the pivot
                var element = chosen.Below;
                double largest = 0.0;
                while (element != null)
                {
                    largest = Math.Max(largest, markowitz.Magnitude(element.Value));
                    element = element.Below;
                }
                element = chosen.Above;
                while (element != null && element.Row >= step)
                {
                    largest = Math.Max(largest, markowitz.Magnitude(element.Value));
                    element = element.Above;
                }

                // If we can't have stability, then drop the pivot
                if (markowitz.Magnitude(chosen.Value) <= markowitz.RelativePivotThreshold * largest)
                    chosen = null;
            }

            return chosen;
        }
    }
}
