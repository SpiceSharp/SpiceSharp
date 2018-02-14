using System;

namespace SpiceSharp.NewSparse.Solve
{
    /// <summary>
    /// A pivot strategy used by a <see cref="Solver{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PivotStrategy<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Setup the pivot strategy
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Rhs</param>
        /// <param name="step">Step</param>
        public abstract void Setup(SparseMatrix<T> matrix, SparseVector<T> rhs, int step, Func<T, double> magnitude);

        /// <summary>
        /// Update the strategy before the pivot is moved
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Right-hand side</param>
        /// <param name="pivot">Pivot</param>
        /// <param name="step">Step</param>
        public abstract void MovePivot(SparseMatrix<T> matrix, SparseVector<T> rhs, MatrixElement<T> pivot, int step);

        /// <summary>
        /// Update the strategy after the pivot is moved
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="pivot">Pivot</param>
        /// <param name="step">Step</param>
        public abstract void Update(SparseMatrix<T> matrix, MatrixElement<T> pivot, int step);

        /// <summary>
        /// Find a pivot
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="step">Step</param>
        /// <returns></returns>
        public abstract MatrixElement<T> FindPivot(SparseMatrix<T> matrix, int step);
    }
}
