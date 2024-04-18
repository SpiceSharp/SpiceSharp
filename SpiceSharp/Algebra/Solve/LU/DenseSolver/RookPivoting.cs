using SpiceSharp.Algebra.Solve;
using SpiceSharp.ParameterSets;
using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Rook pivoting strategy.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="ParameterSet" />
    public partial class RookPivoting<T> : ParameterSet<RookPivoting<T>>
    {
        /// <summary>
        /// Gets the magnitude.
        /// </summary>
        /// <value>
        /// The magnitude.
        /// </value>
        public Func<T, double> Magnitude { get; }

        /// <summary>
        /// Gets or sets the relative pivot threshold.
        /// </summary>
        /// <value>
        /// The relative pivot threshold.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not greater than 0.</exception>
        [ParameterName("pivrel"), ParameterInfo("The relative threshold for validating pivots")]
        [GreaterThan(0), Finite]
        private double _relativePivotThreshold = 1e-3;

        /// <summary>
        /// Gets or sets the absolute pivot threshold.
        /// </summary>
        /// <value>
        /// The absolute pivot threshold.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is negative.</exception>
        [ParameterName("pivtol"), ParameterInfo("The absolute threshold for validating pivots")]
        [GreaterThanOrEquals(0), Finite]
        private double _absolutePivotThreshold = 1e-13;

        /// <summary>
        /// Initializes a new instance of the <see cref="RookPivoting{T}"/> class.
        /// </summary>
        /// <param name="magnitude">The function for turning elements into a scalar.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="magnitude"/> is <c>null</c>.</exception>
        public RookPivoting(Func<T, double> magnitude)
        {
            Magnitude = magnitude.ThrowIfNull(nameof(magnitude));
        }

        /// <summary>
        /// Finds a pivot in the matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        /// <param name="max">The maximum row/column index.</param>
        /// <returns>The pivot.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="matrix"/> is <c>null</c>.</exception>
        public Pivot<MatrixLocation> FindPivot(IMatrix<T> matrix, int eliminationStep, int max)
        {
            matrix.ThrowIfNull(nameof(matrix));
            if (eliminationStep < 1 || eliminationStep > max)
                return Pivot<MatrixLocation>.Empty;

            // Find the largest element below and right of the pivot
            double largest = Magnitude(matrix[eliminationStep, eliminationStep]);
            var loc = new MatrixLocation(eliminationStep, eliminationStep);

            // We just select the biggest off-diagonal element that we can find!
            for (int i = eliminationStep + 1; i <= max; i++)
            {
                double c = Magnitude(matrix[eliminationStep, i]);
                if (c > largest)
                {
                    largest = c;
                    loc = new MatrixLocation(eliminationStep, i);
                }

                c = Magnitude(matrix[i, eliminationStep]);
                if (c > largest)
                {
                    largest = c;
                    loc = new MatrixLocation(i, eliminationStep);
                }
            }
            return largest > 0.0 ? new Pivot<MatrixLocation>(loc, PivotInfo.Good) : Pivot<MatrixLocation>.Empty;
        }

        /// <summary>
        /// Determines whether the current pivot is valid.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        /// <param name="max">The maximum row/column index.</param>
        /// <returns>
        ///   <c>true</c> if the pivot is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="matrix" /> is <c>null</c>.</exception>
        public bool IsValidPivot(IMatrix<T> matrix, int eliminationStep, int max)
        {
            matrix.ThrowIfNull(nameof(matrix));

            // Get the magnitude of the current pivot
            double magnitude = Magnitude(matrix[eliminationStep, eliminationStep]);
            if (magnitude <= AbsolutePivotThreshold)
                return false;

            // Search for the largest element below the pivot
            double largest = 0.0;
            for (int i = eliminationStep + 1; i <= max; i++)
            {
                largest = Math.Max(largest, Magnitude(matrix[eliminationStep, i]));
                largest = Math.Max(largest, Magnitude(matrix[i, eliminationStep]));
            }

            // Check the validity
            if (magnitude > largest * RelativePivotThreshold)
                return true;
            return false;
        }
    }
}
