using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Rook pivoting strategy.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="ParameterSet" />
    public class RookPivoting<T> : ParameterSet where T : IFormattable
    {
        private int _searchReduction;

        /// <summary>
        /// Gets the magnitude.
        /// </summary>
        /// <value>
        /// The magnitude.
        /// </value>
        public Func<T, double> Magnitude { get; }

        /// <summary>
        /// Gets or sets the relative threshold for choosing a pivot.
        /// </summary>
        [ParameterName("pivrel"), ParameterInfo("The relative threshold for validating pivots")]
        public double RelativePivotThreshold { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the absolute threshold for choosing a pivot.
        /// </summary>
        [ParameterName("pivtol"), ParameterInfo("The absolute threshold for validating pivots")]
        public double AbsolutePivotThreshold { get; set; } = 1e-13;

        /// <summary>
        /// Gets or sets the search reduction.
        /// </summary>
        /// <value>
        /// The search reduction.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the reduction is negative.</exception>
        public int SearchReduction
        {
            get => _searchReduction;
            set
            {
                if (_searchReduction < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(SearchReduction), value, 0));
                _searchReduction = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RookPivoting{T}"/> class.
        /// </summary>
        /// <param name="magnitude">The magnitude.</param>
        public RookPivoting(Func<T, double> magnitude)
        {
            Magnitude = magnitude.ThrowIfNull(nameof(magnitude));
        }

        /// <summary>
        /// Finds a pivot in the matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        /// <param name="row">The row of the found pivot.</param>
        /// <param name="column">The column of the found pivot.</param>
        /// <returns></returns>
        public bool FindPivot(IMatrix<T> matrix, int eliminationStep, out int row, out int column)
        {
            var limit = matrix.Size - SearchReduction;

            // Find the largest element below and right of the pivot
            var largest = Magnitude(matrix[eliminationStep, eliminationStep]);
            row = eliminationStep;
            column = eliminationStep;
            for (var i = eliminationStep + 1; i <= limit; i++)
            {
                var current = Magnitude(matrix[eliminationStep, i]);
                if (current > largest)
                {
                    largest = current;
                    row = eliminationStep;
                    column = i;
                }

                current = Magnitude(matrix[i, eliminationStep]);
                if (current > largest)
                {
                    largest = current;
                    row = i;
                    column = eliminationStep;
                }
            }
            return largest > 0.0;
        }

        /// <summary>
        /// Determines whether the current pivot is valid.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        /// <returns>
        /// <c>true</c> if the pivot is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidPivot(IMatrix<T> matrix, int eliminationStep)
        {
            var limit = matrix.Size - SearchReduction;

            // Get the magnitude of the current pivot
            var magnitude = Magnitude(matrix[eliminationStep, eliminationStep]);

            // Search for the largest element below the pivot
            var largest = 0.0;
            for (var i = eliminationStep + 1; i <= limit; i++)
            {
                largest = Math.Max(largest, Magnitude(matrix[eliminationStep, i]));
                largest = Math.Max(largest, Magnitude(matrix[i, eliminationStep]));
            }

            // Check the validity
            if (largest * RelativePivotThreshold < magnitude)
                return true;
            return false;
        }
    }
}
