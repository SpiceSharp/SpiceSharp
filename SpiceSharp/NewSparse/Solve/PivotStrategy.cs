using System;

namespace SpiceSharp.NewSparse.Solve
{
    /// <summary>
    /// A pivot strategy used by a <see cref="Solver{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PivotStrategy<T> where T : IFormattable
    {

        /// <summary>
        /// Setup the pivot strategy
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Rhs</param>
        /// <param name="step">Step</param>
        public abstract void Setup(Matrix<T> matrix, Vector<T> rhs, int step);

        /// <summary>
        /// Update the strategy before the pivot is moved
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Right-hand side</param>
        /// <param name="pivot">Pivot</param>
        /// <param name="step">Step</param>
        public abstract void MovePivot(Matrix<T> matrix, Vector<T> rhs, Element<T> pivot, int step);

        /// <summary>
        /// Update the strategy after the pivot is moved
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="pivot">Pivot</param>
        /// <param name="step">Step</param>
        public abstract void Update(Matrix<T> matrix,Element<T> pivot, int step);

        /// <summary>
        /// Find a pivot
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="step">Step</param>
        /// <returns></returns>
        public abstract Element<T> FindPivot(Matrix<T> matrix, int step);
    }
}
