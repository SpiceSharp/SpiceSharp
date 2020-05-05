using SpiceSharp.ParameterSets;
using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// Markowitz-count based strategy for finding a pivot. Search the complete submatrix.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    [GeneratedParameters]
    public class MarkowitzEntireMatrix<T> : MarkowitzSearchStrategy<T>
    {
        private static int _tiesMultiplier = 5;

        /// <summary>
        /// Gets or sets a heuristic for speeding up pivot searching.
        /// </summary>
        /// <value>
        /// The multiplier for searching pivots with the same markowitz products.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is negative.</exception>
        /// <remarks>
        /// Instead of searching the whole matrix for a pivot on the diagonal, the search strategy can
        /// choose to stop searching for more pivot elements with the lowest "Markowitz product", which
        /// scores how many extra unwanted elements a row/column could create as a by-product of factoring
        /// the solver. When this score is tied, this search strategy will keep a list of them with a
        /// maximum of (MarkowitzProduct * TiesMultiplier) elements. In other words, pivots with a high
        /// Markowitz product will ask the search strategy for more entries to make sure that we can do
        /// better.
        /// </remarks>
        [GreaterThanOrEquals(0)]
        public static int TiesMultiplier
        {
            get => _tiesMultiplier;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(TiesMultiplier), 0);
                _tiesMultiplier = value;
            }
        }

        /// <inheritdoc/>
        public override Pivot<ISparseMatrixElement<T>> FindPivot(Markowitz<T> markowitz, ISparseMatrix<T> matrix, int eliminationStep, int max)
        {
            markowitz.ThrowIfNull(nameof(markowitz));
            matrix.ThrowIfNull(nameof(matrix));
            if (eliminationStep < 1 || eliminationStep > max)
                return Pivot<ISparseMatrixElement<T>>.Empty;

            ISparseMatrixElement<T> chosen = null;
            var minMarkowitzProduct = long.MaxValue;
            double largestMagnitude = 0.0, acceptedRatio = 0.0;
            ISparseMatrixElement<T> largestElement = null;
            var ties = 0;

            // Start search of matrix on column by column basis
            for (var i = eliminationStep; i <= max; i++)
            {
                // Find the biggest magnitude in the column for checking valid pivots later
                var largest = 0.0;
                var diagonal = matrix.FindDiagonalElement(i);
                var element = diagonal;
                while (element != null && element.Row >= eliminationStep)
                {
                    largest = Math.Max(largest, markowitz.Magnitude(element.Value));
                    element = element.Above;
                }
                element = diagonal?.Below;
                while (element != null && element.Row <= max)
                {
                    largest = Math.Max(largest, markowitz.Magnitude(element.Value));
                    element = element.Below;
                }
                if (largest.Equals(0.0))
                    continue;

                // Restart search for a pivot
                element = element?.Above ?? matrix.GetLastInColumn(i);
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
                            if (ties >= minMarkowitzProduct * _tiesMultiplier)
                                return new Pivot<ISparseMatrixElement<T>>(chosen, PivotInfo.Suboptimal);
                        }
                    }

                    element = element.Above;
                }
            }

            // If a valid pivot was found, return it
            if (chosen != null)
                return new Pivot<ISparseMatrixElement<T>>(chosen, PivotInfo.Suboptimal);

            // Else just return the largest element
            if (largestElement == null || largestElement.Value.Equals(default))
                return Pivot<ISparseMatrixElement<T>>.Empty;
            return new Pivot<ISparseMatrixElement<T>>(largestElement, PivotInfo.Bad);
        }
    }
}
