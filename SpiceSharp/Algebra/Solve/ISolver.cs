using SpiceSharp.ParameterSets;
using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a linear system of equations. It tracks permutations of
    /// the equations and the variables.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface ISolver<T> : IParameterSetCollection
    {
        /// <summary>
        /// Gets or sets the degeneracy of the matrix. For example, specifying 1 will let the solver know that one equation is
        /// expected to be linearly dependent on the others.
        /// </summary>
        /// <value>
        /// The degeneracy.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is negative.</exception>
        int Degeneracy { get; set; }

        /// <summary>
        /// Gets the size of the solver. This is the total number of equations.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        int Size { get; }

        /// <summary>
        /// Gets a value indicating whether this solver has been factored.
        /// A solver needs to be factored becore it can solve for a solution.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this solver is factored; otherwise, <c>false</c>.
        /// </value>
        bool IsFactored { get; }

        /// <summary>
        /// Gets or sets the value of the matrix at the specified row and column.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>
        /// The value.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="row"/> or <paramref name="column"/> is negative.</exception>
        T this[int row, int column] { get; set; }

        /// <summary>
        /// Gets or sets the value of the matrix at the specified location.
        /// </summary>
        /// <value>
        /// The value of the matrix element.
        /// </value>
        /// <param name="location">The location.</param>
        /// <returns>
        /// The value.
        /// </returns>
        T this[MatrixLocation location] { get; set; }

        /// <summary>
        /// Gets or sets the value of the right hand side vector at the specified row.
        /// </summary>
        /// <value>
        /// The value of the right hand side vector.
        /// </value>
        /// <param name="row">The row.</param>
        /// <returns>
        /// The value.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="row"/> is negative.</exception>
        T this[int row] { get; set; }

        /// <summary>
        /// Applies forward substitution on a factored matrix and right-hand side vector.
        /// </summary>
        /// <param name="solution">The solution vector.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="solution" /> is <c>null</c>.</exception>
        /// <exception cref="AlgebraException">Thrown if the solver is not factored yet.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="solution" /> does not have <see cref="Size" /> elements.</exception>
        void ForwardSubstitute(IVector<T> solution);

        /// <summary>
        /// Applies backward substitution on a factored matrix and the intermediate vector.
        /// </summary>
        /// <param name="solution">The solution vector.</param>
        void BackwardSubstitute(IVector<T> solution);

        /// <summary>
        /// Computes a contribution for degenerate solvers (<see cref="Degeneracy"/> is larger than 0).
        /// Used when solving submatrices separately.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Returns the contribution.</returns>
        T ComputeDegenerateContribution(int index);

        /// <summary>
        /// Applies forward substitution on the adjoint matrix and right-hand side vector.
        /// </summary>
        /// <param name="solution">The solution vector.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="solution" /> is <c>null</c>.</exception>
        /// <exception cref="AlgebraException">Thrown if the solver is not factored yet.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="solution" /> does not have <see cref="Size" /> elements.</exception>
        void ForwardSubstituteTransposed(IVector<T> solution);

        /// <summary>
        /// Applies backward substitution on the adjoint matrix and the intermediate vector.
        /// </summary>
        /// <param name="solution">The solution vector.</param>
        void BackwardSubstituteTransposed(IVector<T> solution);

        /// <summary>
        /// Computes a contribution of the transposed solving for degenerate solvers (<see cref="Degeneracy"/> is larger than 0).
        /// Used when solving submatrices separately.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Returns the contribution.</returns>
        T ComputeDegenerateContributionTransposed(int index);

        /// <summary>
        /// Factor the equation matrix and right hand side vector.
        /// This method can save time when factoring similar matrices in succession.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the factoring was successful; otherwise, <c>false</c>.
        /// </returns>
        bool Factor();

        /// <summary>
        /// Clears all matrix and vector elements.
        /// </summary>
        /// <remarks>
        /// This method is only redefined here to avoid ambiguity
        /// issues between <see cref="IMatrix{T}"/> and <see cref="IVector{T}"/>.
        /// </remarks>
        void Reset();

        /// <summary>
        /// Clears the solver of any elements. The size of the solver becomes 0.
        /// </summary>
        /// <remarks>
        /// The method is only redefined here to avoid ambiguity issues between
        /// <see cref="IMatrix{T}"/> and <see cref="IVector{T}"/>.
        /// </remarks>
        void Clear();
    }
}
