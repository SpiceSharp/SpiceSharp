using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// Markowitz-based pivot search. Quickly search the diagonal for valid pivots.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public partial class MarkowitzQuickDiagonal<T> : MarkowitzSearchStrategy<T>
    {
        private readonly ISparseMatrixElement<T>[] _tiedElements = new ISparseMatrixElement<T>[MaxMarkowitzTies];

        /// <summary>
        /// Gets or sets the maximum number of diagonals that are considered for choosing the pivot.
        /// </summary>
        /// <value>
        /// The maximum number of searched pivots with the same markowitz product.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not greater than 1.</exception>
        /// <remarks>
        /// The pivot search strategy will try to find pivots with the lowest "Markowitz product", which
        /// scores how many extra unwanted elements a row/column could create as a by-product of factoring
        /// the solver. When this score is tied, this search strategy will keep a list of them with a
        /// maximum of this amount of elements.
        /// </remarks>
        [GreaterThan(1)]
        private static int _maxMarkowitzTies = 100;

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
        /// the solver. When this score is tied, this search strategy will keep a list of them with a
        /// maximum of (MarkowitzProduct * TiesMultiplier) elements. In other words, pivots with a high
        /// Markowitz product will ask the search strategy for more entries to make sure that we can do 
        /// better. Set this value to <see cref="MaxMarkowitzTies"/> to always search for the maximum
        /// number of eligible pivots.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is negative.</exception>
        [GreaterThanOrEquals(0)]
        private static int _tiesMultiplier = 5;

        /// <inheritdoc/>
        public override Pivot<ISparseMatrixElement<T>> FindPivot(Markowitz<T> markowitz, ISparseMatrix<T> matrix, int eliminationStep, int max)
        {
            markowitz.ThrowIfNull(nameof(markowitz));
            matrix.ThrowIfNull(nameof(matrix));
            if (eliminationStep < 1 || eliminationStep > max)
                return Pivot<ISparseMatrixElement<T>>.Empty;

            int minMarkowitzProduct = int.MaxValue;
            int numberOfTies = -1;

            /* Used for debugging along Spice 3f5
            for (var index = matrix.Size + 1; index > eliminationStep; index--)
            {
                int i = index > matrix.Size ? eliminationStep : index; */
            for (int i = eliminationStep; i <= max; i++)
            {
                // Skip diagonal elements with a Markowitz product worse than already found
                int product = markowitz.Product(i);
                if (product >= minMarkowitzProduct)
                    continue;

                // Get the diagonal item
                var diagonal = matrix.FindDiagonalElement(i);
                if (diagonal == null)
                    continue;

                // Get the magnitude
                double magnitude = markowitz.Magnitude(diagonal.Value);
                if (magnitude <= markowitz.AbsolutePivotThreshold)
                    continue;

                // Well, can't do much better than this can we? (Assuming all the singletons are taken)
                // Note that a singleton can still appear depending on the allowed tolerances!
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
                                return new Pivot<ISparseMatrixElement<T>>(diagonal, PivotInfo.Good);
                        }
                    }
                }

                if (product < minMarkowitzProduct)
                {
                    // We found a diagonal that beats all the previous ones!
                    numberOfTies = 0;
                    _tiedElements[0] = diagonal;
                    minMarkowitzProduct = product;
                }
                else
                {
                    if (numberOfTies < _tiedElements.Length - 1)
                    {
                        // Keep track of this diagonal too
                        _tiedElements[++numberOfTies] = diagonal;

                        // This is our heuristic for speeding up pivot searching
                        if (numberOfTies >= minMarkowitzProduct * TiesMultiplier)
                            break;
                    }
                }
            }

            // Not even one eligible pivot on the diagonal...
            if (numberOfTies < 0)
                return Pivot<ISparseMatrixElement<T>>.Empty;

            // Determine which of the tied elements is the best numerical choise
            ISparseMatrixElement<T> chosen = null;
            double maxRatio = 1.0 / markowitz.RelativePivotThreshold;
            for (int i = 0; i <= numberOfTies; i++)
            {
                var diag = _tiedElements[i];
                double mag = markowitz.Magnitude(diag.Value);
                double largest = LargestOtherElementInColumn(markowitz, diag, eliminationStep, max);
                double ratio = largest / mag;
                if (ratio < maxRatio)
                {
                    maxRatio = ratio;
                    chosen = diag;
                }
            }

            // We don't actually know if the pivot is sub-optimal, but we take the worst case scenario.
            return chosen != null ? new Pivot<ISparseMatrixElement<T>>(chosen, PivotInfo.Suboptimal) : Pivot<ISparseMatrixElement<T>>.Empty;
        }

        private double LargestOtherElementInColumn(Markowitz<T> markowitz, ISparseMatrixElement<T> chosen, int eliminationStep, int max)
        {
            // Find the biggest element above and below the pivot
            var element = chosen.Below;
            double largest = 0.0;
            while (element != null && element.Row <= max)
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
            return largest;
        }

        /// <inheritdoc/>
        public override MarkowitzSearchStrategy<T> Clone()
            => new MarkowitzQuickDiagonal<T>();
    }
}
