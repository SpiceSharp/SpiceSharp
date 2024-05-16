using SpiceSharp.Algebra;
using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A helper class that is specific to Modified Nodal Analysis.
    /// </summary>
    public static class ModifiedNodalAnalysisHelper<T>
    {
        /// <summary>
        /// Gets or sets the magnitude method.
        /// </summary>
        /// <value>
        /// The magnitude.
        /// </value>
        public static Func<T, double> Magnitude { get; set; }

        /// <summary>
        /// Preorders the modified nodal analysis.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="size">The submatrix size to be preordered.</param>
        public static void PreorderModifiedNodalAnalysis(ISparseMatrix<T> matrix, int size)
        {
            /*
             * MNA often has patterns that we can already use for pivoting
             * 
             * For example, the following pattern is quite common (lone twins):
             * ? ... 1
             * .  \  .
             * 1 ... 0
             * We can swap columns to pivot the 1's to the diagonal. This
             * makes searching for pivots faster.
             * 
             * We also often have the pattern of double twins:
             * ? ...  ? ...  1
             * .  \   . ...  .
             * ? ...  ? ... -1
             * . ...  .  \   .
             * 1 ... -1 ...  0
             * We can swap either of the columns to pivot the 1 or -1
             * to the diagonal. Note that you can also treat this pattern
             * as 2 twins on the ?-diagonal elements. These should be taken
             * care of first.
             */
            ISparseMatrixElement<T> twin1 = null, twin2 = null;
            int start = 1;
            bool anotherPassNeeded;

            do
            {
                bool swapped;
                anotherPassNeeded = swapped = false;

                // Search for zero diagonals with lone twins. 
                for (int j = start; j <= size; j++)
                {
                    if (matrix.FindDiagonalElement(j) == null)
                    {
                        int twins = CountTwins(matrix, j, ref twin1, ref twin2, size);
                        if (twins == 1)
                        {
                            // Lone twins found, swap columns
                            matrix.SwapColumns(twin2.Column, j);
                            swapped = true;
                        }
                        else if (twins > 1 && !anotherPassNeeded)
                        {
                            anotherPassNeeded = true;
                            start = j;
                        }
                    }
                }

                // All lone twins are gone, look for zero diagonals with multiple twins. 
                if (anotherPassNeeded)
                {
                    for (int j = start; !swapped && j <= size; j++)
                    {
                        if (matrix.FindDiagonalElement(j) == null)
                        {
                            CountTwins(matrix, j, ref twin1, ref twin2, size);
                            matrix.SwapColumns(twin2.Column, j);
                            swapped = true;
                        }
                    }
                }
            }
            while (anotherPassNeeded);
        }

        /// <summary>
        /// Apply an additional conductance to the diagonal elements of a matrix that is typically constructed using Modified Nodal Analysis (MNA).
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="gmin">The conductance to be added to the diagonal.</param>
        public static void ApplyDiagonalGmin(IMatrix<double> matrix, double gmin)
        {
            matrix.ThrowIfNull(nameof(matrix));

            // Skip if not necessary
            if (gmin <= 0.0)
                return;

            if (matrix is ISparseMatrix<double> m)
            {
                // Add to the diagonal
                for (int i = 1; i <= matrix.Size; i++)
                {
                    var diagonal = m.FindDiagonalElement(i);
                    if (diagonal != null)
                        diagonal.Value += gmin;
                }
            }
            else
            {
                for (int i = 1; i <= matrix.Size; i++)
                    matrix[i, i] += gmin;
            }
        }

        /// <summary>
        /// Count the number of twins in a matrix that is typically constructed using Modified Nodal Analysis (MNA).
        /// </summary>
        /// <remarks>
        /// A twin is a matrix element that is equal to one, and also has a one on the transposed position. MNA formulation
        /// often leads to many twins, allowing us to save some time by searching for them beforehand.
        /// </remarks>
        /// <param name="matrix">The matrix.</param>
        /// <param name="column">The column index.</param>
        /// <param name="twin1">The first twin element.</param>
        /// <param name="twin2">The second twin element.</param>
        /// <param name="size">The size of the submatrix to search.</param>
        /// <returns>The number of twins found.</returns>
        private static int CountTwins<M>(M matrix, int column, ref ISparseMatrixElement<T> twin1, ref ISparseMatrixElement<T> twin2, int size)
            where M : ISparseMatrix<T>
        {
            int twins = 0;

            // Begin `CountTwins'.
            var cTwin1 = matrix.GetFirstInColumn(column);
            while (cTwin1 != null && cTwin1.Row <= size)
            {
                // if (Math.Abs(pTwin1.Element.Magnitude) == 1.0)
                if (Magnitude(cTwin1.Value).Equals(1.0))
                {
                    int row = cTwin1.Row;
                    var cTwin2 = matrix.GetFirstInColumn(row);
                    while (cTwin2 != null && cTwin2.Row != column)
                        cTwin2 = cTwin2.Below;
                    if (cTwin2 != null && Magnitude(cTwin2.Value).Equals(1.0))
                    {
                        // Found symmetric twins. 
                        if (++twins >= 2) return twins;
                        twin1 = cTwin1;
                        twin2 = cTwin2;
                    }
                }
                cTwin1 = cTwin1.Below;
            }
            return twins;
        }
    }
}
