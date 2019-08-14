using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// Markowitz-based pivot search. Quickly search the diagonal for valid pivots.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public class MarkowitzQuickDiagonal<T> : MarkowitzSearchStrategy<T> where T : IFormattable, IEquatable<T>
    {
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

            var minMarkowitzProduct = int.MaxValue;
            MatrixElement<T> chosen = null;

            /* Used for debugging along Spice 3f5
            for (var index = matrix.Size + 1; index > eliminationStep; index--)
            {
                int i = index > matrix.Size ? eliminationStep : index; */
            for (var i = eliminationStep; i <= matrix.Size; i++)
            {
                // Skip diagonal elements with a Markowitz product worse than already found
                var product = markowitz.Product(i);
                if (product >= minMarkowitzProduct)
                    continue;

                // Get the diagonal item
                var diagonal = matrix.GetDiagonalElement(i);
                if (diagonal == null)
                    continue;

                // Get the magnitude
                var magnitude = markowitz.Magnitude(diagonal.Value);
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
                            var largest = Math.Max(
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
                var largest = 0.0;
                while (element != null)
                {
                    largest = Math.Max(largest, markowitz.Magnitude(element.Value));
                    element = element.Below;
                }
                element = chosen.Above;
                while (element != null && element.Row >= eliminationStep)
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
