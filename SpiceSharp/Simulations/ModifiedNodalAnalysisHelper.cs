using System;
using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Helper class
    /// </summary>
    public static class ModifiedNodalAnalysisHelper
    {
        /// <summary>
        /// Method for preordering a matrix used in Modified Nodal Analysis (MNA)
        /// </summary>
        /// <typeparam name="T">Base type</typeparam>
        /// <param name="solver">Solver</param>
        /// <param name="magnitude">Magnitude method</param>
        public static void PreorderModifiedNodalAnalysis<T>(this Solver<T> solver, Func<T, double> magnitude) where T : IFormattable, IEquatable<T>
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

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
        /// Apply a gmin to the diagonal elements
        /// </summary>
        /// <param name="solver">Solver</param>
        /// <param name="gmin">Conductance</param>
        public static void ApplyDiagonalGmin(this SparseLinearSystem<double> solver, double gmin)
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

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
        /// Count the number of twins
        /// </summary>
        /// <param name="solver">Solver</param>
        /// <param name="column">Column</param>
        /// <param name="twin1">First twin element</param>
        /// <param name="twin2">Second twin element</param>
        /// <param name="magnitude">Magnitude method</param>
        /// <returns></returns>
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
