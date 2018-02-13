using System;

namespace SpiceSharp.NewSparse.Solve
{
    /// <summary>
    /// A search strategy for finding pivots
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    public abstract class MarkowitzSearchStrategy<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Find a pivot
        /// </summary>
        /// <param name="markowitz">Markowitz</param>
        /// <param name="matrix">Matrix</param>
        /// <param name="step">Step</param>
        /// <returns></returns>
        public abstract MatrixElement<T> FindPivot(Markowitz<T> markowitz, SparseMatrix<T> matrix, int step);
    }
}
