using System;

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
        /// Gets or sets the order of the system that needs to be solved.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        /// <remarks>
        /// This property can be used to limit the number of elimination steps.
        /// </remarks>
        public int Order
        {
            get
            {
                if (_order <= 0)
                    return Size + _order;
                return _order;
            }
            set => _order = value;
        }
        private int _order = 0;

        /// <summary>
        /// Occurs before the solver uses the decomposition to find the solution.
        /// </summary>
        public event EventHandler<SolveEventArgs<T>> BeforeSolve;

        /// <summary>
        /// Occurs after the solver used the decomposition to find a solution.
        /// </summary>
        public event EventHandler<SolveEventArgs<T>> AfterSolve;

        /// <summary>
        /// Occurs before the solver uses the transposed decomposition to find the solution.
        /// </summary>
        public event EventHandler<SolveEventArgs<T>> BeforeSolveTransposed;

        /// <summary>
        /// Occurs after the solver uses the transposed decomposition to find a solution.
        /// </summary>
        public event EventHandler<SolveEventArgs<T>> AfterSolveTransposed;

        /// <summary>
        /// Occurs before the solver is factored.
        /// </summary>
        public event EventHandler<EventArgs> BeforeFactor;

        /// <summary>
        /// Occurs after the solver has been factored.
        /// </summary>
        public event EventHandler<EventArgs> AfterFactor;

        /// <summary>
        /// Occurs before the solver is ordered and factored.
        /// </summary>
        public event EventHandler<EventArgs> BeforeOrderAndFactor;

        /// <summary>
        /// Occurs after the solver has been ordered and factored.
        /// </summary>
        public event EventHandler<EventArgs> AfterOrderAndFactor;

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
        /// Solves the equations using the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="solution">The solution.</param>
        public abstract void Solve(IVector<T> solution);

        /// <summary>
        /// Solves the equations using the transposed Y-matrix.
        /// </summary>
        /// <param name="solution">The solution.</param>
        public abstract void SolveTransposed(IVector<T> solution);

        /// <summary>
        /// Factor the Y-matrix and Rhs-vector.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the factoring was successful; otherwise <c>false</c>.
        /// </returns>
        public bool Factor() => Factor(Size);

        /// <summary>
        /// Factor they Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="size">The size of the matrix that should be factored.</param>
        /// <returns>
        /// <c>true</c> if the factoring was successful; otherwise <c>false</c>.
        /// </returns>
        public bool Factor(int size)
        {
            OnBeforeFactor();

            int order = Math.Min(Order, size);
            for (var step = 1; step <= order; step++)
            {
                if (!Elimination(step, size))
                {
                    IsFactored = false;
                    OnAfterFactor();
                    return false;
                }
            }
            IsFactored = true;
            OnAfterFactor();
            return true;
        }

        /// <summary>
        /// Order and factor the Y-matrix and Rhs-vector.
        /// </summary>
        public void OrderAndFactor()
        {
            OnBeforeOrderAndFactor();

            var size = Size;
            var step = 1;
            if (!NeedsReordering)
            {
                for (step = 1; step <= Order; step++)
                {
                    if (Strategy.IsValidPivot(Matrix, step))
                    {
                        if (!Elimination(step, size))
                        {
                            IsFactored = false;
                            throw new AlgebraException("Elimination failed on accepted pivot");
                        }
                    }
                    else
                    {
                        NeedsReordering = true;
                        break;
                    }
                }

                if (!NeedsReordering)
                {
                    IsFactored = true;
                    OnAfterOrderAndFactor();
                    return;
                }
            }

            // Setup pivoting strategy
            Strategy.Setup(Matrix, Vector, step);

            for (; step <= Order; step++)
            {
                if (!Strategy.FindPivot(Matrix, step, out int row, out int column))
                    throw new SingularException(step);
                SwapRows(row, step);
                SwapColumns(column, step);
                if (!Elimination(step, size))
                {
                    IsFactored = false;
                    throw new SingularException(step);
                }
            }
            IsFactored = true;
            NeedsReordering = false;
            OnAfterOrderAndFactor();
        }

        /// <summary>
        /// Eliminate the submatrix right and below the pivot.
        /// </summary>
        /// <param name="step">The current elimination step.</param>
        /// <param name="size">The maximum row/column to be eliminated.</param>
        /// <returns>
        /// <c>true</c> if the elimination was succesful; otherwise <c>false</c>.
        /// </returns>
        protected abstract bool Elimination(int step, int size);

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
                _ = Row[index]; // Expand the vector
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

        /// <summary>
        /// Should be called before solving the decomposition.
        /// </summary>
        /// <param name="args">The <see cref="SolveEventArgs{T}"/> instance containing the event data.</param>
        protected void OnBeforeSolve(SolveEventArgs<T> args) => BeforeSolve?.Invoke(this, args);

        /// <summary>
        /// Should be called after solving the decomposition.
        /// </summary>
        /// <param name="args">The <see cref="SolveEventArgs{T}"/> instance containing the event data.</param>
        protected void OnAfterSolve(SolveEventArgs<T> args) => AfterSolve?.Invoke(this, args);

        /// <summary>
        /// Should be called before solving the transposed decomposition.
        /// </summary>
        /// <param name="args">The <see cref="SolveEventArgs{T}"/> instance containing the event data.</param>
        protected void OnBeforeSolveTransposed(SolveEventArgs<T> args) => BeforeSolveTransposed?.Invoke(this, args);

        /// <summary>
        /// Should be called after solving the transposed decomposition.
        /// </summary>
        /// <param name="args">The <see cref="SolveEventArgs{T}"/> instance containing the event data.</param>
        protected void OnAfterSolveTransposed(SolveEventArgs<T> args) => AfterSolveTransposed?.Invoke(this, args);

        /// <summary>
        /// Should be called before factoring.
        /// </summary>
        protected void OnBeforeFactor() => BeforeFactor?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Should be called after factoring.
        /// </summary>
        protected void OnAfterFactor() => AfterFactor?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Should be called before ordering and factoring.
        /// </summary>
        protected void OnBeforeOrderAndFactor() => BeforeOrderAndFactor?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Should be called after ordering and factoring.
        /// </summary>
        protected void OnAfterOrderAndFactor() => AfterOrderAndFactor?.Invoke(this, EventArgs.Empty);
    }
}
