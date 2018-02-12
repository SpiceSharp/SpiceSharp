using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.NewSparse.Solve
{
    public class MarkowitzSingleton<T> : MarkowitzSearchStrategy<T> where T : IFormattable
    {
        /// <summary>
        /// Find a pivot
        /// </summary>
        /// <param name="markowitz">Markowitz object</param>
        /// <param name="matrix">Matrix</param>
        /// <param name="step">Step</param>
        /// <returns></returns>
        public override Element<T> FindPivot(Markowitz<T> markowitz, Matrix<T> matrix, int step)
        {
            int singletons = markowitz.Singletons;
            int index = matrix.Size + 1;
            markowitz.Product[index] = markowitz.Product[step];
            markowitz.Product[step - 1] = 0; // Make sure the loop below ends

            // Try all singletons
            while (singletons-- > 0)
            {
                // Find the last singleton
                while (markowitz.Product[index] != 0)
                    index--;
                int i = index;

                // Ensure that i is valid
                if (index < step)
                    break;
                if (index > matrix.Size)
                    i = step;

                // Singleton has been found in either/both row or/and column i
                var chosen = matrix.GetDiagonalElement(i);
                if (chosen != null)
                {
                    // Singleton lies on the diagonal
                    double magnitude = markowitz.Magnitude(chosen.Value);
                    if (magnitude > markowitz.AbsolutePivotThreshold &&
                        magnitude > markowitz.RelativePivotThreshold * BiggestInColumnExclude(matrix, chosen, step, markowitz.Magnitude))
                        return chosen;
                }

                // Singleton does not lie on the diagonal, find it
                if (markowitz.Row[i] == 0)
                {
                    chosen = matrix.GetFirstInRow(i);
                    while (chosen != null && chosen.Column < step)
                        chosen = chosen.Right;

                    // TODO: Is this right? Spice 3f5 is doing it like this...
                    if (chosen == null)
                        break;

                    double magnitude = markowitz.Magnitude(chosen.Value);
                    if (magnitude > markowitz.AbsolutePivotThreshold &&
                        magnitude > markowitz.RelativePivotThreshold * BiggestInColumnExclude(matrix, chosen, step, markowitz.Magnitude))
                        return chosen;
                }
            }

            // All singletons were unacceptable...
            return null;
        }

        /// <summary>
        /// Find the largest magnitude excluding the current element
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="element"></param>
        /// <param name="step"></param>
        /// <param name="magnitude"></param>
        /// <returns></returns>
        double BiggestInColumnExclude(Matrix<T> matrix, Element<T> element, int step, Func<T, double> magnitude)
        {
            int row = element.Row;
            int column = element.Column;
            element = matrix.GetFirstInColumn(column);

            // Travel down column until reduced submatrix is entered
            while (element != null && element.Row < step)
                element = element.Below;

            // Initialize the variable largest
            double largest = 0.0;
            if (element.Row != row)
                largest = magnitude(element.Value);

            // Search rest of the column for the largest element, avoiding the excluded element
            while ((element = element.Below) != null)
            {
                double mag = magnitude(element.Value);
                if (mag > largest)
                {
                    if (element.Row != row)
                        largest = mag;
                }
            }
            return largest;
        }
    }
}
