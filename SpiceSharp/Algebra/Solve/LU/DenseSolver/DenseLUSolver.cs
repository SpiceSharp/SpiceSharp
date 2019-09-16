using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// An base class for dense linear systems that can be solved using LU decomposition.
    /// Pivoting is controlled by the 
    /// </summary>
    /// <typeparam name="M">The matrix type.</typeparam>
    /// <typeparam name="V">The vector type.</typeparam>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="SpiceSharp.Algebra.IMatrix{T}" />
    /// <seealso cref="SpiceSharp.Algebra.IVector{T}" />
    /// <seealso cref="System.IFormattable" />
    public abstract partial class DenseLUSolver<M, V, T> : LinearSystem<M, V, T>, ISolver<T>
        where M : IPermutableMatrix<T>
        where V : IPermutableVector<T>
        where T : IFormattable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the matrix needs to be reordered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the matrix needs reordering; otherwise, <c>false</c>.
        /// </value>
        public bool NeedsReordering { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the solver is factored.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the solver is factored; otherwise, <c>false</c>.
        /// </value>
        public bool IsFactored { get; protected set; }

        /// <summary>
        /// Gets the strategy.
        /// </summary>
        /// <value>
        /// The strategy.
        /// </value>
        public DensePivotStrategy<T> Strategy { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseLUSolver{M, V, T}"/> class.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="vector">The vector.</param>
        /// <param name="strategy">The pivoting strategy that needs to be used.</param>
        protected DenseLUSolver(M matrix, V vector, DensePivotStrategy<T> strategy)
            : base(matrix, vector)
        {
            NeedsReordering = true;
            Strategy = strategy.ThrowIfNull(nameof(strategy));
        }

        /// <summary>
        /// Preconditions the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        public virtual void Precondition(PreconditionMethod<T> method)
        {
            bool _isFirstSwap = true;
            void OnMatrixRowsSwapped(object sender, PermutationEventArgs args)
            {
                // Reflect the swapped vector elements in the row translation
                if (_isFirstSwap)
                {
                    _isFirstSwap = false;
                    Row.Swap(args.Index1, args.Index2);
                    Vector.SwapElements(args.Index1, args.Index2);
                    _isFirstSwap = true;
                }
            }
            void OnMatrixColumnsSwapped(object sender, PermutationEventArgs args)
            {
                // Reflect the swapped matrix column in the column translation
                Column.Swap(args.Index1, args.Index2);
            }
            void OnVectorElementsSwapped(object sender, PermutationEventArgs args)
            {
                // Reflect the swapped vector elements in the row translation
                if (_isFirstSwap)
                {
                    _isFirstSwap = false;
                    Row.Swap(args.Index1, args.Index2);
                    Matrix.SwapRows(args.Index1, args.Index2);
                    _isFirstSwap = true;
                }
            }

            Matrix.RowsSwapped += OnMatrixRowsSwapped;
            Matrix.ColumnsSwapped += OnMatrixColumnsSwapped;
            Vector.ElementsSwapped += OnVectorElementsSwapped;
            method(Matrix, Vector);
            Matrix.RowsSwapped -= OnMatrixRowsSwapped;
            Matrix.ColumnsSwapped -= OnMatrixColumnsSwapped;
            Vector.ElementsSwapped -= OnVectorElementsSwapped;
        }

        /// <summary>
        /// Solves the system of equations.
        /// </summary>
        /// <param name="solution">The solution vector that will hold the solution to the set of equations.</param>
        public abstract void Solve(IVector<T> solution);

        /// <summary>
        /// Solves the equations using the transposed Y-matrix.
        /// </summary>
        /// <param name="solution">The solution.</param>
        public abstract void SolveTransposed(IVector<T> solution);

        /// <summary>
        /// Factors the matrix.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the matrix was succesfully factored; otherwise <c>false</c>.
        /// </returns>
        public abstract bool Factor();

        /// <summary>
        /// Orders and factors the matrix.
        /// </summary>
        public abstract void OrderAndFactor();

        /// <summary>
        /// Finds the diagonal element at the specified row/column.
        /// </summary>
        /// <param name="index">The row/column index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        public override IMatrixElement<T> FindDiagonalElement(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index > Size)
                return null;
            return new MatrixElement(this, index, index);
        }

        /// <summary>
        /// Gets a pointer to the matrix element at the specified row and column. A
        /// non-zero element is always guaranteed with this method. The matrix is expanded
        /// if necessary.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        public override IMatrixElement<T> GetMatrixElement(int row, int column)
        {
            if (row < 0)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (column < 0)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row > Size || column > Size)
                _ = Matrix[row, column];
            return new MatrixElement(this, row, column);
        }

        /// <summary>
        /// Finds a pointer to the matrix element at the specified row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The matrix element; otherwise <c>null</c>.
        /// </returns>
        public override IMatrixElement<T> FindMatrixElement(int row, int column)
        {
            if (row < 0)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (column < 0)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row > Size || column > Size)
                return null;
            return new MatrixElement(this, row, column);
        }

        /// <summary>
        /// Gets a vector element at the specified index. A non-zero element is
        /// always guaranteed with this method. The vector is expanded if
        /// necessary.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The vector element.
        /// </returns>
        public override IVectorElement<T> GetVectorElement(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index > Size)
                _ = Row[index];
            return new VectorElement(this, index);
        }

        /// <summary>
        /// Finds a vector element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The vector element; otherwise <c>null</c>.
        /// </returns>
        public override IVectorElement<T> FindVectorElement(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index > Size)
                return null;
            return new VectorElement(this, index);
        }

        /// <summary>
        /// Resets all elements in the matrix.
        /// </summary>
        public override void ResetMatrix()
        {
            base.ResetMatrix();
            IsFactored = false;
        }
    }
}
