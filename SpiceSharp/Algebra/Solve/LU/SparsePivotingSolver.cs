using System;

namespace SpiceSharp.Algebra.Solve;

/// <summary>
/// A base class for solvers that keep their equations in a sparse matrix and a sparse
/// right hand side vector, and that are allowed to reorder them. It takes care of handing
/// out element references, of translating between the indices the outside world uses and
/// the ones the reordered system uses, and of exposing the system for preconditioning.
/// How the system is actually factored is left to the derived class.
/// </summary>
/// <typeparam name="T">The base value type.</typeparam>
/// <seealso cref="PivotingSolver{M, V, T}"/>
/// <seealso cref="ISparsePivotingSolver{T}"/>
public abstract partial class SparsePivotingSolver<T> : PivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>,
    ISparsePivotingSolver<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SparsePivotingSolver{T}"/> class.
    /// </summary>
    protected SparsePivotingSolver()
        : base(new SparseMatrix<T>(), new SparseVector<T>())
    {
    }

    /// <inheritdoc/>
    public override void Precondition(PreconditioningMethod<ISparseMatrix<T>, ISparseVector<T>, T> method)
    {
        var reorderedMatrix = new ReorderedMatrix(this);
        var reorderedVector = new ReorderedVector(this);
        method(reorderedMatrix, reorderedVector);
    }

    /// <summary>
    /// Finds the diagonal element at the specified row/column.
    /// </summary>
    /// <param name="index">The row/column index.</param>
    /// <returns>
    /// The matrix element.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative.</exception>
    public Element<T> FindDiagonalElement(int index)
    {
        index.GreaterThanOrEquals(nameof(index), 0);
        if (index > Size)
            return null;
        int row = Row[index];
        int column = Column[index];
        return Matrix.FindElement(new MatrixLocation(row, column));
    }

    /// <inheritdoc/>
    public Element<T> FindElement(MatrixLocation location) => Matrix.FindElement(ExternalToInternal(location));

    /// <inheritdoc/>
    public Element<T> FindElement(int row) => Vector.FindElement(Row[row]);

    /// <summary>
    /// Gets a matrix element by its position in the reordered system, creating it if it does
    /// not exist yet. Every route that can add a matrix element passes through here, so a
    /// derived class that caches anything about the sparsity pattern can find out about the
    /// change by overriding this.
    /// </summary>
    /// <param name="location">The location in the reordered system.</param>
    /// <returns>The matrix element.</returns>
    protected virtual Element<T> GetInternalElement(MatrixLocation location) => Matrix.GetElement(location);

    /// <summary>
    /// Removes a matrix element by its position in the reordered system. Every route that can
    /// remove a matrix element passes through here.
    /// </summary>
    /// <param name="location">The location in the reordered system.</param>
    /// <returns><c>true</c> if the element was removed; otherwise, <c>false</c>.</returns>
    protected virtual bool RemoveInternalElement(MatrixLocation location) => Matrix.RemoveElement(location);

    /// <summary>
    /// Gets a right hand side element by its position in the reordered system, creating it if
    /// it does not exist yet.
    /// </summary>
    /// <param name="row">The row in the reordered system.</param>
    /// <returns>The vector element.</returns>
    protected virtual Element<T> GetInternalElement(int row) => Vector.GetElement(row);

    /// <summary>
    /// Removes a right hand side element by its position in the reordered system.
    /// </summary>
    /// <param name="row">The row in the reordered system.</param>
    /// <returns><c>true</c> if the element was removed; otherwise, <c>false</c>.</returns>
    protected virtual bool RemoveInternalElement(int row) => Vector.RemoveElement(row);

    /// <inheritdoc/>
    public Element<T> GetElement(MatrixLocation location)
    {
        location = ExternalToInternal(location);
        var elt = GetInternalElement(location);

        // If we created a new row or column, let's move to the front
        // to keep the same equations that are linearly dependent
        if (Degeneracy > 0 && Size - Degeneracy > 0)
        {
            if (location.Row == Size)
                SwapRows(Size, Size - Degeneracy);
            if (location.Column == Size)
                SwapColumns(Size, Size - Degeneracy);
        }
        return elt;
    }

    /// <inheritdoc/>
    public bool RemoveElement(MatrixLocation location)
    {
        location = ExternalToInternal(location);
        return RemoveInternalElement(location);
    }

    /// <inheritdoc/>
    public Element<T> GetElement(int row)
    {
        if (row < 0)
            throw new ArgumentOutOfRangeException(nameof(row));
        row = Row[row];
        var elt = GetInternalElement(row);

        // If we created a new row, let's move it back to still have the same equations that are considered linearly dependent
        if (Degeneracy > 0)
        {
            if (row == Size)
                SwapRows(Size, Size - Degeneracy);
        }
        return elt;
    }

    /// <inheritdoc/>
    public bool RemoveElement(int row)
    {
        row = Row[row];
        return RemoveInternalElement(row);
    }
}
