using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A pivot strategy used by a <see cref="DenseLUSolver{M, V, T}"/>
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public abstract class DensePivotStrategy<T> where T : IFormattable
    {
        /// <summary>
        /// Gets the magnitude.
        /// </summary>
        /// <value>
        /// The magnitude.
        /// </value>
        protected Func<T, double> Magnitude { get; private set; }

        /// <summary>
        /// Gets or sets the row/column limit for searching a pivot.
        /// </summary>
        /// <value>
        /// The maximum row/column to search.
        /// </value>
        public int SearchLimit { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DensePivotStrategy{T}"/> class.
        /// </summary>
        /// <param name="magnitude">The magnitude.</param>
        protected DensePivotStrategy(Func<T, double> magnitude)
        {
            Magnitude = magnitude.ThrowIfNull(nameof(magnitude));
        }

        /// <summary>
        /// Determines whether the current pivot is valid.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        /// <returns>
        ///   <c>true</c> if the pivot is valid; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool IsValidPivot(IMatrix<T> matrix, int eliminationStep);

        /// <summary>
        /// Sets up the pivot strategy.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <param name="eliminationStep">The current elimination step.</param>
        public abstract void Setup(IMatrix<T> matrix, IVector<T> rhs, int eliminationStep);

        /// <summary>
        /// Indicates that a pivot has been chosen and that it will be move to the diagonal.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <param name="row">The row of the pivot.</param>
        /// <param name="column">The column of the pivot.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        public abstract void MovePivot(IMatrix<T> matrix, IVector<T> rhs, int row, int column, int eliminationStep);

        /// <summary>
        /// Updates the strategy after the pivot was moved to the diagonal.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        public abstract void Update(IMatrix<T> matrix, int row, int column, int eliminationStep);

        /// <summary>
        /// Finds a pivot in the matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        /// <param name="row">The row of the found pivot.</param>
        /// <param name="column">The column of the found pivot.</param>
        public abstract bool FindPivot(IMatrix<T> matrix, int eliminationStep, out int row, out int column);
    }
}
