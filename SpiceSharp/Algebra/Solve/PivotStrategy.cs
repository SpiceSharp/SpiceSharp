using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// A pivot strategy used by a <see cref="Solver{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PivotStrategy<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Check that the current pivot is valid
        /// It checks for the submatrix right/below of the pivot.
        /// </summary>
        /// <param name="pivot">Pivot</param>
        /// <returns></returns>
        public abstract bool IsValidPivot(MatrixElement<T> pivot);

        /// <summary>
        /// Setup the pivot strategy
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Rhs</param>
        /// <param name="eliminationStep">Step</param>
        /// <param name="magnitude">Magnitude method</param>
        public abstract void Setup(SparseMatrix<T> matrix, SparseVector<T> rhs, int eliminationStep, Func<T, double> magnitude);

        /// <summary>
        /// Update the strategy before the pivot is moved
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Right-hand side</param>
        /// <param name="pivot">Pivot</param>
        /// <param name="eliminationStep">Step</param>
        public abstract void MovePivot(SparseMatrix<T> matrix, SparseVector<T> rhs, MatrixElement<T> pivot, int eliminationStep);

        /// <summary>
        /// Update the strategy after the pivot is moved
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="pivot">Pivot</param>
        /// <param name="eliminationStep">Step</param>
        public abstract void Update(SparseMatrix<T> matrix, MatrixElement<T> pivot, int eliminationStep);

        /// <summary>
        /// Find a pivot
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="eliminationStep">Step</param>
        /// <returns></returns>
        public abstract MatrixElement<T> FindPivot(SparseMatrix<T> matrix, int eliminationStep);
    }
}
