using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Rook pivoting strategy.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="SpiceSharp.Algebra.DensePivotStrategy{T}" />
    public class RookPivoting<T> : DensePivotStrategy<T> where T : IFormattable
    {
        /// <summary>
        /// Gets the relative pivot threshold.
        /// </summary>
        /// <value>
        /// The relative pivot threshold.
        /// </value>
        public double RelativePivotThreshold { get; private set; } = 1e-3;

        /// <summary>
        /// Initializes a new instance of the <see cref="RookPivoting{T}"/> class.
        /// </summary>
        /// <param name="magnitude">The magnitude.</param>
        public RookPivoting(Func<T, double> magnitude)
            : base(magnitude)
        {
        }

        /// <summary>
        /// Finds a pivot in the matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        /// <param name="row">The row of the found pivot.</param>
        /// <param name="column">The column of the found pivot.</param>
        /// <returns></returns>
        public override bool FindPivot(IMatrix<T> matrix, int eliminationStep, out int row, out int column)
        {
            // Find the largest element below and right of the pivot
            var largest = Magnitude(matrix[eliminationStep, eliminationStep]);
            row = eliminationStep;
            column = eliminationStep;
            for (var i = eliminationStep + 1; i <= matrix.Size; i++)
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
        public override bool IsValidPivot(IMatrix<T> matrix, int eliminationStep)
        {
            // Get the magnitude of the current pivot
            var magnitude = Magnitude(matrix[eliminationStep, eliminationStep]);

            // Search for the largest element below the pivot
            var largest = 0.0;
            for (var i = eliminationStep + 1; i <= matrix.Size; i++)
            {
                largest = Math.Max(largest, Magnitude(matrix[eliminationStep, i]));
                largest = Math.Max(largest, Magnitude(matrix[i, eliminationStep]));
            }

            // Check the validity
            if (largest * RelativePivotThreshold < magnitude)
                return true;
            return false;
        }

        /// <summary>
        /// Indicates that a pivot has been chosen and that it will be move to the diagonal.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <param name="row">The row of the pivot.</param>
        /// <param name="column">The column of the pivot.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        public override void MovePivot(IMatrix<T> matrix, IVector<T> rhs, int row, int column, int eliminationStep)
        {
        }

        /// <summary>
        /// Sets up the pivot strategy.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <param name="eliminationStep">The current elimination step.</param>
        public override void Setup(IMatrix<T> matrix, IVector<T> rhs, int eliminationStep)
        {
        }

        /// <summary>
        /// Updates the strategy after the pivot was moved to the diagonal.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        public override void Update(IMatrix<T> matrix, int row, int column, int eliminationStep)
        {
        }
    }
}
