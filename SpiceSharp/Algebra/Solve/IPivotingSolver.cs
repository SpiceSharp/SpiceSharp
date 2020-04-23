using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// An <see cref="ISolver{T}"/> that implements mechanisms for pivoting. This means
    /// that the solver may reorder the internal matrix and right hand side vector.
    /// </summary>
    /// <typeparam name="M">The matrix type.</typeparam>
    /// <typeparam name="V">The vector type.</typeparam>
    /// <typeparam name="T">The base value type.</typeparam>
    public interface IPivotingSolver<M, V, T> : ISolver<T>
        where M : IMatrix<T>
        where V : IVector<T>
    {
        /// <summary>
        /// Gets or sets the pivot search reduction. This makes sure that pivots cannot
        /// be chosen from the last N rows. The default, 0, lets the pivot strategy to
        /// choose from the whole matrix.
        /// </summary>
        /// <value>
        /// The pivot search reduction.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is negative.</exception>
        int PivotSearchReduction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the solver needs to be reordered all the way from the start.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the solver needs reordering; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// If this flag is false, the solver will still reorder when using <see cref="OrderAndFactor" />, but
        /// it will try to stay away from reordering as long as possible. This flag will force the solver to
        /// immediately start reordering.
        /// </remarks>
        bool NeedsReordering { get; set; }

        /// <summary>
        /// Preconditions the solver matrix and right hand side vector.
        /// </summary>
        /// <param name="method">The method.</param>
        void Precondition(PreconditioningMethod<M, V, T> method);

        /// <summary>
        /// Order and factor the equation matrix and right hand side vector.
        /// This method will reorder the matrix as it sees fit.
        /// </summary>
        /// <returns>
        /// The number of rows that were successfully eliminated.
        /// </returns>
        int OrderAndFactor();

        /// <summary>
        /// Maps an internal matrix location to an external one.
        /// </summary>
        /// <param name="indices">The internal matrix location.</param>
        /// <returns>
        /// The external matrix location.
        /// </returns>
        MatrixLocation InternalToExternal(MatrixLocation indices);

        /// <summary>
        /// Maps an external matrix location to an internal one.
        /// </summary>
        /// <param name="indices">The external matrix location.</param>
        /// <returns>
        /// The internal matrix location.
        /// </returns>
        MatrixLocation ExternalToInternal(MatrixLocation indices);
    }

    /// <summary>
    /// Describes a method for preconditioning an <see cref="IPivotingSolver{M, V, T}"/>.
    /// </summary>
    /// <typeparam name="M">The matrix.</typeparam>
    /// <typeparam name="V">The vector.</typeparam>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <param name="matrix">The matrix.</param>
    /// <param name="vector">The vector.</param>
    public delegate void PreconditioningMethod<M, V, T>(M matrix, V vector)
            where M : IMatrix<T>
            where V : IVector<T>;
}
