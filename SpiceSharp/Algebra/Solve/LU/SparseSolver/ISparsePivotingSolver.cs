using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A sparse solver that can use pivoting.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="ISparseSolver{T}" />
    /// <seealso cref="IPivotingSolver{M, V, T}" />
    public interface ISparsePivotingSolver<T> : ISparseSolver<T>, IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>
        where T : IFormattable
    {
    }
}
