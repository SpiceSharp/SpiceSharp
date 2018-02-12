using System;

namespace SpiceSharp.NewSparse.Solve
{
    /// <summary>
    /// A pivot strategy that can be used for any generic type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MarkowitzEntireMatrix<T> : MarkowitzSearchStrategy<T> where T : IFormattable
    {
        /// <summary>
        /// Constants
        /// </summary>
        const int TiesMultiplier = 5;

        /// <summary>
        /// Search the entire matrix for a suitable pivot
        /// In order to preserve sparsity, Markowitz counts are used to find the largest valid
        /// pivot with the smallest number of elements.
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="step">Step</param>
        /// <returns></returns>
        public override Element<T> FindPivot(Markowitz<T> markowitz, Matrix<T> matrix, int step)
        {
            Element<T> chosen = null;
            long minMarkowitzProduct = long.MaxValue;
            double largestMagnitude = 0.0, acceptedRatio = 0.0;
            Element<T> largestElement = null;
            int ties = 0;

            // Start search of matrix on column by column basis
            for (int i = step; i <= matrix.Size; i++)
            {
                var element = matrix.GetFirstInColumn(i);
                while (element != null && element.Row < step)
                    element = element.Below;

                // Are there no elements?
                double largest = LargestInColumn(markowitz, element);
                if (largest.Equals(0.0))
                    continue;

                while (element != null)
                {
                    // Check to see if the element is the largest encountered so far. If so, record its
                    // magnitude and address
                    double magnitude = markowitz.Magnitude(element.Value);
                    if (magnitude > largestMagnitude)
                    {
                        largestElement = element;
                        largestMagnitude = magnitude;
                    }

                    // Calculate element's Markowitz product
                    long product = markowitz.Row[element.Row] * markowitz.Column[element.Column];

                    // test to see if the element is acceptable as a pivot candidate
                    if (product <= minMarkowitzProduct
                        && (magnitude > markowitz.RelativePivotThreshold * largest)
                        && (magnitude > markowitz.AbsolutePivotThreshold))
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
                            double ratio = largest / magnitude;
                            if (ratio < acceptedRatio)
                            {
                                chosen = element;
                                acceptedRatio = ratio;
                            }
                            if (ties >= minMarkowitzProduct * TiesMultiplier)
                                return chosen;
                        }
                    }

                    element = element.Below;
                }
            }

            // Return the chosen pivot
            if (chosen != null)
                return chosen;

            if (largestElement.Equals(0.0))
                throw new Exception("Singular");

            // TODO: Notify that the pivot is too small
            return largestElement;
        }

        /// <summary>
        /// Find the largest magnitude in the column at or below <paramref name="element"/>
        /// </summary>
        /// <param name="element">Element</param>
        /// <returns></returns>
        double LargestInColumn(Markowitz<T> markowitz, Element<T> element)
        {
            double magnitude, largest = 0;

            // Search the column for the largest element beginning at element
            while (element != null)
            {
                magnitude = markowitz.Magnitude(element.Value);
                if (magnitude > largest)
                    largest = magnitude;
                element = element.Below;
            }
            return largest;
        }
    }
}
