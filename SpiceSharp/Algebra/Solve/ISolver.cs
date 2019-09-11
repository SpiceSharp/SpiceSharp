using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a linear system of equations. It tracks permutations of
    /// the equations and the variables.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="SpiceSharp.Algebra.IMatrix{T}" />
    /// <seealso cref="SpiceSharp.Algebra.IVector{T}" />
    public interface ISolver<T> : IMatrix<T>, IVector<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Preconditions the current matrix and vector.
        /// </summary>
        /// <param name="method">The preconditioning method.</param>
        void Precondition(PreconditionMethod<T> method);

        /// <summary>
        /// Solves the equations using the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="solution">The solution.</param>
        void Solve(IVector<T> solution);

        /// <summary>
        /// Solves the equations using the transposed Y-matrix.
        /// </summary>
        /// <param name="solution">The solution.</param>
        void SolveTransposed(IVector<T> solution);

        /// <summary>
        /// Factor the Y-matrix and Rhs-vector.
        /// </summary>
        /// <returns></returns>
        bool Factor();

        /// <summary>
        /// Order and factor the Y-matrix and Rhs-vector.
        /// </summary>
        void OrderAndFactor();

        /// <summary>
        /// Clears all matrix and vector elements.
        /// </summary>
        void Reset();

        /// <summary>
        /// Maps an internal row/column tuple to an external one.
        /// </summary>
        /// <param name="indices">The internal row/column indices.</param>
        /// <returns>The external row/column indices.</returns>
        Tuple<int, int> InternalToExternal(Tuple<int, int> indices);

        /// <summary>
        /// Maps an external row/column tuple to an internal one.
        /// </summary>
        /// <param name="indices">The external row/column indices.</param>
        /// <returns>The internal row/column indices.</returns>
        Tuple<int, int> ExternalToInternal(Tuple<int, int> indices);
    }

    /// <summary>
    /// A delegate for preconditioning a solver matrix and vector.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <param name="matrix">The matrix to be preconditioned.</param>
    /// <param name="vector">The vector to be preconditioned.</param>
    public delegate void PreconditionMethod<T>(IPermutableMatrix<T> matrix, IPermutableVector<T> vector) where T : IFormattable;
}
