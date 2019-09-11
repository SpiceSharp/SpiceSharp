using System;
using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A helper class that is specific to Modified Nodal Analysis.
    /// </summary>
    public class ModifiedNodalAnalysisHelper<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// A delegate for measuring the magnitude of elements.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The magnitude.</returns>
        public delegate double MagnitudeMethod(T value);

        /// <summary>
        /// Gets or sets the magnitude.
        /// </summary>
        /// <value>
        /// The magnitude.
        /// </value>
        public static MagnitudeMethod Magnitude
        {
            get => _magnitude;
            set => _magnitude = value.ThrowIfNull("magnitude");
        }
        private static MagnitudeMethod _magnitude = null;

        /// <summary>
        /// Preorders the modified nodal analysis.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        public static void PreorderModifiedNodalAnalysis(IPermutableMatrix<T> matrix)
        {
            matrix.ThrowIfNull(nameof(matrix));

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
            IMatrixElement<T> twin1 = null, twin2 = null;
            var start = 1;
            bool anotherPassNeeded;

            do
            {
                bool swapped;
                anotherPassNeeded = swapped = false;

                // Search for zero diagonals with lone twins. 
                for (var j = start; j <= matrix.Size; j++)
                {
                    if (matrix.FindDiagonalElement(j) == null)
                    {
                        var twins = CountTwins(matrix, j, ref twin1, ref twin2);
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
                    for (var j = start; !swapped && j <= matrix.Size; j++)
                    {
                        if (matrix.FindDiagonalElement(j) == null)
                        {
                            CountTwins(matrix, j, ref twin1, ref twin2);
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
        public static void ApplyDiagonalGmin(IPermutableMatrix<double> matrix, double gmin)
        {
            matrix.ThrowIfNull(nameof(matrix));

            // Skip if not necessary
            if (gmin <= 0.0)
                return;

            // Add to the diagonal
            for (var i = 1; i <= matrix.Size; i++)
            {
                var diagonal = matrix.FindDiagonalElement(i);
                if (diagonal != null)
                    diagonal.Value += gmin;
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
        /// <returns>The number of twins found.</returns>
        private static int CountTwins(IPermutableMatrix<T> matrix, int column, ref IMatrixElement<T> twin1, ref IMatrixElement<T> twin2)
        {
            var twins = 0;

            // Begin `CountTwins'.
            var cTwin1 = matrix.GetFirstInColumn(column);
            while (cTwin1 != null)
            {
                // if (Math.Abs(pTwin1.Element.Magnitude) == 1.0)
                if (Magnitude(cTwin1.Value).Equals(1.0))
                {
                    var row = cTwin1.Row;
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
