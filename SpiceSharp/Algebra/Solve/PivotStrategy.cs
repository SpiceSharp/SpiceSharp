using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// A pivot strategy used by a <see cref="Solver{T}" />
    /// </summary>
    /// <typeparam name="T">The base value type</typeparam>
    public abstract class PivotStrategy<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Gets or sets the relative threshold for choosing a pivot.
        /// </summary>
        public double RelativePivotThreshold { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the absolute threshold for choosing a pivot.
        /// </summary>
        public double AbsolutePivotThreshold { get; set; } = 1e-13;

        /// <summary>
        /// This method will check whether or not a pivot element is valid or not.
        /// It checks for the submatrix right/below of the pivot.
        /// </summary>
        /// <param name="pivot">The pivot candidate.</param>
        /// <returns>True if the pivot can be used.</returns>
        public abstract bool IsValidPivot(MatrixElement<T> pivot);

        /// <summary>
        /// Setup the pivot strategy.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <param name="eliminationStep">The current elimination step.</param>
        /// <param name="magnitude">The method used to determine the magnitude of an element.</param>
        public abstract void Setup(SparseMatrix<T> matrix, SparseVector<T> rhs, int eliminationStep, Func<T, double> magnitude);

        /// <summary>
        /// Move the pivot to the diagonal for this elimination step.
        /// </summary>
        /// <remarks>
        /// This is done by swapping the rows and columns of the diagonal and that of the pivot.
        /// </remarks>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <param name="pivot">The pivot element.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        public abstract void MovePivot(SparseMatrix<T> matrix, SparseVector<T> rhs, MatrixElement<T> pivot, int eliminationStep);

        /// <summary>
        /// Update the strategy after the pivot was moved.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="pivot">The pivot element.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        public abstract void Update(SparseMatrix<T> matrix, MatrixElement<T> pivot, int eliminationStep);

        /// <summary>
        /// Notifies the strategy that a fill-in has been created
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="fillin">The fill-in.</param>
        public abstract void CreateFillin(SparseMatrix<T> matrix, MatrixElement<T> fillin);

        /// <summary>
        /// Find a pivot in the matrix.
        /// </summary>
        /// <remarks>
        /// The pivot should be searched for in the submatrix towards the right and down of the
        /// current diagonal at row/column <paramref name="eliminationStep"/>. This pivot element
        /// will be moved to the diagonal for this elimination step.
        /// </remarks>
        /// <param name="matrix">The matrix.</param>
        /// <param name="eliminationStep">The current elimination step.</param>
        /// <returns></returns>
        public abstract MatrixElement<T> FindPivot(SparseMatrix<T> matrix, int eliminationStep);
    }
}
