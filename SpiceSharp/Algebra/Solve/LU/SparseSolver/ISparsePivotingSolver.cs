namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A sparse solver that can use pivoting to solve equations.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="ISparseSolver{T}" />
    /// <seealso cref="ISparseMatrix{T}"/>
    /// <seealso cref="ISparseVector{T}"/>
    /// <seealso cref="IPivotingSolver{M, V, T}" />
    public interface ISparsePivotingSolver<T> :
        ISparseSolver<T>,
        IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>
    {
    }
}
