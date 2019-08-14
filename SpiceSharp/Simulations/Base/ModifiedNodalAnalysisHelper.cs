using System;
using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A helper class that is specific to Modified Nodal Analysis.
    /// </summary>
    public static class ModifiedNodalAnalysisHelper
    {
        /// <summary>
        /// This method preorders a matrix that is typically constructed using Modified Nodal Analysis (MNA).
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="solver">The solver.</param>
        /// <param name="magnitude">The method that converts the base value type to a scalar.</param>
        public static void PreorderModifiedNodalAnalysis<T>(this Solver<T> solver, Func<T, double> magnitude) where T : IFormattable, IEquatable<T>
        {
            solver.ThrowIfNull(nameof(solver));
            magnitude.ThrowIfNull(nameof(magnitude));

            /*
             * MNA often has patterns that we can already use for pivoting
             * 
             * For example, the following pattern is quite common lone twins:
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
             * We can swap either twin column with to pivot the 1 or -1 
             * to the diagonal. Note that you can also treat this pattern
             * as 2 twins on the ?-diagonal elements. These should be taken
             * care of first.
             */
            MatrixElement<T> twin1 = null, twin2 = null;
            var start = 1;
            bool anotherPassNeeded;

            do
            {
                bool swapped;
                anotherPassNeeded = swapped = false;

                // Search for zero diagonals with lone twins. 
                for (var j = start; j <= solver.Order; j++)
                {
                    if (solver.ReorderedDiagonal(j) == null)
                    {
                        var twins = CountTwins(solver, j, ref twin1, ref twin2, magnitude);
                        if (twins == 1)
                        {
                            // Lone twins found, swap
                            solver.MovePivot(twin2, j);
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
                    for (var j = start; !swapped && j <= solver.Order; j++)
                    {
                        if (solver.ReorderedDiagonal(j) == null)
                        {
                            CountTwins(solver, j, ref twin1, ref twin2, magnitude);
                            solver.MovePivot(twin2, j);
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
        /// <param name="solver">The solver.</param>
        /// <param name="gmin">The conductance to be added.</param>
        public static void ApplyDiagonalGmin(this SparseLinearSystem<double> solver, double gmin)
        {
            solver.ThrowIfNull(nameof(solver));

            // Skip if not necessary
            if (gmin <= 0.0)
                return;

            // Add to the diagonal
            for (var i = 1; i <= solver.Order; i++)
            {
                var diagonal = solver.ReorderedDiagonal(i);
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
        /// <param name="solver">The solver.</param>
        /// <param name="column">The column index.</param>
        /// <param name="twin1">The first twin element.</param>
        /// <param name="twin2">The second twin element.</param>
        /// <param name="magnitude">The method that converts the base value type to a scalar.</param>
        /// <returns>The number of twins found.</returns>
        private static int CountTwins<T>(Solver<T> solver, int column, ref MatrixElement<T> twin1, ref MatrixElement<T> twin2, Func<T, double> magnitude) where T : IFormattable, IEquatable<T>
        {
            var twins = 0;

            // Begin `CountTwins'. 

            var cTwin1 = solver.FirstInReorderedColumn(column);
            while (cTwin1 != null)
            {
                // if (Math.Abs(pTwin1.Element.Magnitude) == 1.0)
                if (magnitude(cTwin1.Value).Equals(1.0))
                {
                    var row = cTwin1.Row;
                    var cTwin2 = solver.FirstInReorderedColumn(row);
                    while (cTwin2 != null && cTwin2.Row != column)
                        cTwin2 = cTwin2.Below;
                    if (cTwin2 != null && magnitude(cTwin2.Value).Equals(1.0))
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
