using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// Markowitz-count based strategy for finding a pivot. Searches the whole diagonal of the submatrix.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public class MarkowitzDiagonal<T> : MarkowitzSearchStrategy<T> where T : IFormattable, IEquatable<T>
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

            var minMarkowitzProduct = int.MaxValue;
            MatrixElement<T> chosen = null;
            var ratioOfAccepted = 0.0;
            var ties = 0;

            /* Used for debugging alongside Spice 3f5
            for (var index = matrix.Size + 1; index > eliminationStep; index--)
            {
                var i = index > matrix.Size ? eliminationStep : index; */
            for (var i = eliminationStep; i <= matrix.Size; i++)
            {
                // Skip the diagonal if we already have a better one
                if (markowitz.Product(i) > minMarkowitzProduct)
                    continue;

                // Get the diagonal
                var diagonal = matrix.GetDiagonalElement(i);
                if (diagonal == null)
                    continue;

                // Get the magnitude
                var magnitude = markowitz.Magnitude(diagonal.Value);
                if (magnitude <= markowitz.AbsolutePivotThreshold)
                    continue;

                // Check that the pivot is eligible
                var largest = 0.0;
                var element = diagonal.Below;
                while (element != null)
                {
                    largest = Math.Max(largest, markowitz.Magnitude(element.Value));
                    element = element.Below;
                }
                element = diagonal.Above;
                while (element != null && element.Row >= eliminationStep)
                {
                    largest = Math.Max(largest, markowitz.Magnitude(element.Value));
                    element = element.Above;
                }
                if (magnitude <= markowitz.RelativePivotThreshold * largest)
                    continue;

                // Check markowitz numbers to find the optimal
                if (markowitz.Product(i) < minMarkowitzProduct)
                {
                    // Notice strict inequality, this is a new smallest product
                    chosen = diagonal;
                    minMarkowitzProduct = markowitz.Product(i);
                    ratioOfAccepted = largest / magnitude;
                    ties = 0;
                }
                else
                {
                    // If we have enough elements with the same (minimum) number of ties, stop searching
                    ties++;
                    var ratio = largest / magnitude;
                    if (ratio < ratioOfAccepted)
                    {
                        chosen = diagonal;
                        ratioOfAccepted = ratio;
                    }
                    if (ties >= minMarkowitzProduct * TiesMultiplier)
                        return chosen;
                }
            }

            // The chosen pivot has already been checked for validity
            return chosen;
        }
    }
}
