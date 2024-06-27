using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// Markowitz-count based strategy for finding a pivot. Searches the whole diagonal of the submatrix.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public partial class MarkowitzDiagonal<T> : MarkowitzSearchStrategy<T>
    {
        /// <summary>
        /// Gets or sets a heuristic for speeding up pivot searching.
        /// </summary>
        /// <value>
        /// The multiplier for searching pivots with the same markowitz products.
        /// </value>
        /// <remarks>
        /// Instead of searching the whole matrix for a pivot on the diagonal, the search strategy can
        /// choose to stop searching for more pivot elements with the lowest "Markowitz product", which
        /// scores how many extra unwanted elements a row/column could create as a by-product of factoring
        /// the solver. When this score is tied, this search strategy will keep searching until we have
        /// (MarkowitzProduct * TiesMultiplier) eligible pivots. In other words, pivots with a high
        /// Markowitz product will ask the search strategy for more entries to make sure that we can't do 
        /// better.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is negative.</exception>
        [GreaterThanOrEquals(0)]
        private static int _tiesMultiplier = 5;

        /// <inheritdoc/>
        public override MarkowitzSearchStrategy<T> Clone()
            => (MarkowitzSearchStrategy<T>)MemberwiseClone();

        /// <inheritdoc/>
        public override Pivot<ISparseMatrixElement<T>> FindPivot(Markowitz<T> markowitz, ISparseMatrix<T> matrix, int eliminationStep, int max)
        {
            markowitz.ThrowIfNull(nameof(markowitz));
            matrix.ThrowIfNull(nameof(matrix));
            if (eliminationStep < 1 || eliminationStep > max)
                return Pivot<ISparseMatrixElement<T>>.Empty;

            int minMarkowitzProduct = int.MaxValue;
            ISparseMatrixElement<T> chosen = null;
            double ratioOfAccepted = 0.0;
            int ties = 0;

            /* Used for debugging alongside Spice 3f5
            for (var index = matrix.Size + 1; index > eliminationStep; index--)
            {
                var i = index > matrix.Size ? eliminationStep : index; */
            for (int i = eliminationStep; i <= max; i++)
            {
                // Skip the diagonal if we already have a better one
                if (markowitz.Product(i) > minMarkowitzProduct)
                    continue;

                // Get the diagonal
                var diagonal = matrix.FindDiagonalElement(i);
                if (diagonal == null)
                    continue;

                // Get the magnitude
                double magnitude = markowitz.Magnitude(diagonal.Value);
                if (magnitude <= markowitz.AbsolutePivotThreshold)
                    continue;

                // Check that the pivot is eligible
                double largest = 0.0;
                var element = diagonal.Below;
                while (element != null && element.Row <= max)
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

                // Check markowitz numbers to find the optimal pivot
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
                    double ratio = largest / magnitude;
                    if (ratio < ratioOfAccepted)
                    {
                        chosen = diagonal;
                        ratioOfAccepted = ratio;
                    }
                    if (ties >= minMarkowitzProduct * _tiesMultiplier)
                        return new Pivot<ISparseMatrixElement<T>>(chosen, PivotInfo.Suboptimal);
                }
            }

            // The chosen pivot has already been checked for validity
            return chosen != null ? new Pivot<ISparseMatrixElement<T>>(chosen, PivotInfo.Suboptimal) : Pivot<ISparseMatrixElement<T>>.Empty;
        }
    }
}
