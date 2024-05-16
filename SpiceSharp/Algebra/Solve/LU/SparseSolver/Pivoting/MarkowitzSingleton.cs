using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// Markowitz-count based strategy for finding a pivot. This strategy will search for 
    /// singletons (rows or columns with only one element), these can be found rather cheaply.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public class MarkowitzSingleton<T> : MarkowitzSearchStrategy<T>
    {
        /// <inheritdoc/>
        public override MarkowitzSearchStrategy<T> Clone()
            => new MarkowitzSingleton<T>();

        /// <inheritdoc/>
        public override Pivot<ISparseMatrixElement<T>> FindPivot(Markowitz<T> markowitz, ISparseMatrix<T> matrix, int eliminationStep, int max)
        {
            markowitz.ThrowIfNull(nameof(markowitz));
            matrix.ThrowIfNull(nameof(matrix));
            if (eliminationStep < 1 || eliminationStep > max)
                return Pivot<ISparseMatrixElement<T>>.Empty;

            // No singletons left, so don't bother
            if (markowitz.Singletons == 0)
                return Pivot<ISparseMatrixElement<T>>.Empty;

            // Find the first valid singleton we can use
            int singletons = 0, index;
            for (int i = max + 1; i >= eliminationStep; i--)
            {
                // First check the current pivot, else
                // search from last to first as this tends to push the higher markowitz
                // products downwards.
                index = i > max ? eliminationStep : i;

                // Not a singleton, let's skip this one...
                if (markowitz.Product(index) != 0)
                    continue;

                // Keep track of how many singletons we have found
                singletons++;

                /*
                 * NOTE: In the sparse library of Spice 3f5 first the diagonal is checked,
                 * then the column is checked (if no success go to row) and finally the
                 * row is checked for a valid singleton pivot.
                 * The diagonal should actually not be checked, as the checking algorithm is the same
                 * as for checking the column (FindBiggestInColExclude will not find anything in the column
                 * for singletons). The original author did not find this.
                 * Also, the original algorithm has a bug in there that renders the whole code invalid...
                 * if (ChosenPivot != NULL) { break; } will throw away the pivot even if it was found!
                 * (ref. Spice 3f5 Libraries/Sparse/spfactor.c, line 1286)
                 */

                // Find the singleton element
                ISparseMatrixElement<T> chosen;
                if (markowitz.ColumnCount(index) == 0)
                {
                    // The last element in the column is the singleton element!
                    chosen = matrix.GetLastInColumn(index);
                    if (chosen.Row <= max && chosen.Column <= max)
                    {
                        // Check if it is a valid pivot
                        double magnitude = markowitz.Magnitude(chosen.Value);
                        if (magnitude > markowitz.AbsolutePivotThreshold)
                            return new Pivot<ISparseMatrixElement<T>>(chosen, PivotInfo.Good);
                    }
                }

                // Check if we can still use a row here
                if (markowitz.RowCount(index) == 0)
                {
                    // The last element in the row is the singleton element
                    chosen = matrix.GetLastInRow(index);

                    // When the matrix has an empty row, and an RHS element, it is possible
                    // that the singleton is not a singleton
                    if (chosen == null || chosen.Column < eliminationStep)
                    {
                        // The last element is not valid, singleton failed!
                        singletons--;
                        continue;
                    }

                    // First find the biggest magnitude in the column, not counting the pivot candidate
                    var element = chosen.Above;
                    double largest = 0.0;
                    while (element != null && element.Row >= eliminationStep)
                    {
                        largest = Math.Max(largest, markowitz.Magnitude(element.Value));
                        element = element.Above;
                    }
                    element = chosen.Below;
                    while (element != null && element.Row <= max)
                    {
                        largest = Math.Max(largest, markowitz.Magnitude(element.Value));
                        element = element.Below;
                    }

                    // Check if the pivot is valid
                    if (chosen.Row <= max && chosen.Column <= max)
                    {
                        double magnitude = markowitz.Magnitude(chosen.Value);
                        if (magnitude > markowitz.AbsolutePivotThreshold &&
                            magnitude > markowitz.RelativePivotThreshold * largest)
                            return new Pivot<ISparseMatrixElement<T>>(chosen, PivotInfo.Good);
                    }
                }

                // Don't continue if no more singletons are available
                if (singletons >= markowitz.Singletons)
                    break;
            }

            // All singletons were unacceptable...
            return Pivot<ISparseMatrixElement<T>>.Empty;
        }
    }
}
