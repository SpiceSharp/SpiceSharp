using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a linear system of equations. It tracks permutations of
    /// the equations and the variables.
    /// </summary>
    /// <remarks>
    /// The solver readily implements <see cref="IElementMatrix{T}"/> and <see cref="IElementVector{T}"/>.
    /// This makes it easier for other objects to contribute to the solver Y-matrix or Rhs-vector without
    /// the need to store a reference to the solver.
    /// </remarks>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="SpiceSharp.Algebra.IElementMatrix{T}" />
    /// <seealso cref="SpiceSharp.Algebra.IElementVector{T}" />
    public interface ISolver<T> : IElementMatrix<T>, IElementVector<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Preconditions the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
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
    /// A method that can be used to precondition a solver.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <param name="matrix">The (permutated) matrix.</param>
    /// <param name="vector">The (permutated) vector.</param>
    public delegate void PreconditionMethod<T>(IMatrix<T> matrix, IVector<T> vector) where T : IFormattable;
}
