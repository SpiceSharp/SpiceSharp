using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// Markowitz-based pivot strategy: diagonal search
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MarkowitzDiagonal<T> : MarkowitzSearchStrategy<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Constants
        /// </summary>
        private const int TiesMultiplier = 5;

        /// <summary>
        /// Find the pivot on the diagonal
        /// </summary>
        /// <param name="markowitz">Markowitz</param>
        /// <param name="matrix">Matrix</param>
        /// <param name="eliminationStep">Step</param>
        /// <returns></returns>
        public override MatrixElement<T> FindPivot(Markowitz<T> markowitz, SparseMatrix<T> matrix, int eliminationStep)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (markowitz == null)
                throw new ArgumentNullException(nameof(markowitz));
            if (eliminationStep < 1)
                throw new ArgumentException("Invalid elimination step");

            int minMarkowitzProduct = int.MaxValue;
            MatrixElement<T> chosen = null;
            double ratioOfAccepted = 0.0;
            int ties = 0;

            for (int i = eliminationStep; i <= matrix.Size; i++)
            {
                // Skip the diagonal if we already have a better one
                if (markowitz.Product(i) > minMarkowitzProduct)
                    continue;

                // Get the diagonal
                var diagonal = matrix.GetDiagonalElement(i);
                if (diagonal == null)
                    continue;

                // Get the magnitude
                double magnitude = markowitz.Magnitude(diagonal.Value);
                if (magnitude <= markowitz.AbsolutePivotThreshold)
                    continue;

                // Check that the pivot is eligible
                double largest = 0.0;
                var element = diagonal.Below;
                while (element != null)
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

                // Check markowitz numbers to find the optimal
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
                    if (ties >= minMarkowitzProduct * TiesMultiplier)
                        return chosen;
                }
            }

            // The chosen pivot has already been checked for validity
            return chosen;
        }
    }
}
