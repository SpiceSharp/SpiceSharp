using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a linear system of equations. It tracks permutations of
    /// the equations and the variables.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface ISolver<T> : IParameterized where T : IFormattable
    {
        /// <summary>
        /// Gets or sets the degeneracy of the matrix. For example, specifying 1 will let the solver know that one equation is
        /// expected to be linearly dependent on the others.
        /// </summary>
        /// <value>
        /// The degeneracy.
        /// </value>
        int Degeneracy { get; set; }

        /// <summary>
        /// Gets the size of the matrix and right-hand side vector.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        /// <remarks>
        /// This property is only redefined here to avoid ambiguity 
        /// issues between <see cref="IMatrix{T}"/> and <see cref="IVector{T}"/>.
        /// </remarks>
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
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>The value.</returns>
        T this[int row, int column] { get; set; }

        /// <summary>
        /// Gets or sets the value of the matrix at the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>The value.</returns>
        T this[MatrixLocation location] { get; set; }

        /// <summary>
        /// Gets or sets the value of the vector at the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>The value.</returns>
        T this[int row] { get; set; }

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
