using System;
using System.Numerics;
using SpiceSharp.NewSparse;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Helper class
    /// </summary>
    public static class Helper<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Method for preordering a matrix used in Modified Nodal Analysis (MNA)
        /// </summary>
        /// <typeparam name="T">Base type</typeparam>
        /// <param name="solver">Solver</param>
        public static void PreorderModifiedNodalAnalysis(Solver<T> solver)
        {
            /*
             * MNA often has patterns that we can already use for pivoting
             * 
             * For example, the following pattern is quite common:
             * Lone twins
             * ? ... 1
             * .  \  .
             * 1 ... 0
             * We can swap columns to pivot the 1's to the diagonal. This
             * makes searching for pivots faster.
             * 
             * We also often have the pattern:
             * Double twins
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
            MatrixElement<T> pTwin1 = null, pTwin2 = null;
            int start = 1;
            bool swapped, anotherPassNeeded;

            do
            {
                anotherPassNeeded = swapped = false;

                // Search for zero diagonals with lone twins. 
                for (int j = start; j <= solver.Order; j++)
                {
                    if (solver.ReorderedDiagonal(j) == null)
                    {
                        int twins = CountTwins(solver, j, ref pTwin1, ref pTwin2);
                        if (twins == 1)
                        {
                            // Lone twins found, swap
                            solver.MovePivot(pTwin2, j);
                            swapped = true;
                        }
                        else if ((twins > 1) && !anotherPassNeeded)
                        {
                            anotherPassNeeded = true;
                            start = j;
                        }
                    }
                }

                // All lone twins are gone, look for zero diagonals with multiple twins. 
                if (anotherPassNeeded)
                {
                    for (int j = start; !swapped && (j <= solver.Order); j++)
                    {
                        if (solver.ReorderedDiagonal(j) == null)
                        {
                            CountTwins(solver, j, ref pTwin1, ref pTwin2);
                            solver.MovePivot(pTwin2, j);
                            swapped = true;
                        }
                    }
                }
            }
            while (anotherPassNeeded);
        }

        /// <summary>
        /// Count the number of twins
        /// </summary>
        /// <param name="solver">Solver</param>
        /// <param name="Col">Column</param>
        /// <param name="ppTwin1">First twin element</param>
        /// <param name="ppTwin2">Second twin element</param>
        /// <returns></returns>
        static int CountTwins(Solver<T> solver, int Col, ref MatrixElement<T> ppTwin1, ref MatrixElement<T> ppTwin2)
        {
            int Row, Twins = 0;
            MatrixElement<T> pTwin1, pTwin2;

            // Begin `CountTwins'. 

            pTwin1 = solver.FirstInReorderedColumn(Col);
            while (pTwin1 != null)
            {
                // if (Math.Abs(pTwin1.Element.Magnitude) == 1.0)
                if (Magnitude(pTwin1.Value).Equals(1.0))
                {
                    Row = pTwin1.Row;
                    pTwin2 = solver.FirstInReorderedColumn(Row);
                    while ((pTwin2 != null) && (pTwin2.Row != Col))
                        pTwin2 = pTwin2.Below;
                    if ((pTwin2 != null) && Magnitude(pTwin2.Value).Equals(1.0))
                    {
                        // Found symmetric twins. 
                        if (++Twins >= 2) return Twins;
                        ppTwin1 = pTwin1;
                        ppTwin2 = pTwin2;
                    }
                }
                pTwin1 = pTwin1.Below;
            }
            return Twins;
        }
    }
}
