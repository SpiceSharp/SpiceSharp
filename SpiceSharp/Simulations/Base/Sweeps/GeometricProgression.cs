using SpiceSharp.ParameterSets;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Simulations.Sweeps
{
    /// <summary>
    /// A geometric progression.
    /// </summary>
    public abstract class GeometricProgression : ParameterSet, IEnumerable<double>
    {
        /// <summary>
        /// Gets the Initial factor A.
        /// </summary>
        protected abstract double A { get; }

        /// <summary>
        /// Gets the progression factor R.
        /// </summary>
        protected abstract double R { get; }

        /// <summary>
        /// Gets the number of steps.
        /// </summary>
        protected abstract int N { get; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<double> GetEnumerator()
        {
            // Trivial case: The initial value and final value are the same
            int n = N;
            double current = A;
            yield return current;

            // Just the starting point
            if (n == 0)
                yield break;

            // Geometric decaying progressions
            double r = R;
            if (n < 0)
            {
                r = 1.0 / r;
                n = -n;
            }

            // Output the geometric progression
            for (int i = 0; i < n; i++)
            {
                current *= r;
                yield return current;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
