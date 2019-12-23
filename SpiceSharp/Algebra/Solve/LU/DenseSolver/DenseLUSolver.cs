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
    /// <seealso cref="IMatrix{T}" />
    /// <seealso cref="IVector{T}" />
    /// <seealso cref="IFormattable" />
    public abstract partial class DenseLUSolver<M, V, T> : LinearSystem<M, V, T>, ISolver<T>
        where M : IPermutableMatrix<T>
        where V : IPermutableVector<T>
        where T : IFormattable, IEquatable<T>
    {

        /// <summary>
        /// Gets or sets the degeneracy of the matrix. For example, specifying 1 will let the solver know that one equation is
        /// expected to be linearly dependent on the others.
        /// </summary>
        /// <value>
        /// The degeneracy.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the order reduction is negative.</exception>
        public int Degeneracy
        {
            get => _order;
            set
            {
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Algebra_InvalidOrder);
                _order = value;
            }
        }
        private int _order = 0;

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
        /// Gets the pivoting strategy.
        /// </summary>
        /// <value>
        /// The pivoting strategy.
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
            int order = Math.Min(size, Size - _order);
            for (var step = 1; step <= order; step++)
            {
                var pivot = Matrix[step, step];
                if (pivot.Equals(default))
                    return false;
                Eliminate(step, size);
            }
            IsFactored = true;
            return true;
        }

        /// <summary>
        /// Order and factor the Y-matrix and Rhs-vector.
        /// </summary>
        public int OrderAndFactor()
        {
            var size = Size;
            var step = 1;
            var order = Size - _order;
            if (!NeedsReordering)
            {
                for (step = 1; step <= order; step++)
                {
                    if (Strategy.IsValidPivot(Matrix, step))
                        Eliminate(step, size);
                    else
                    {
                        NeedsReordering = true;
                        break;
                    }
                }

                if (!NeedsReordering)
                {
                    IsFactored = true;
                    return order;
                }
            }

            // Setup pivoting strategy
            Strategy.Setup(Matrix, Vector, step);

            for (; step <= order; step++)
            {
                if (!Strategy.FindPivot(Matrix, step, out int row, out int column))
                    return step - 1;
                SwapRows(row, step);
                SwapColumns(column, step);
                Eliminate(step, size);
            }
            IsFactored = true;
            NeedsReordering = false;
            return order;
        }

        /// <summary>
        /// Eliminates the submatrix right and below the pivot.
        /// </summary>
        /// <param name="step">The current elimination step.</param>
        /// <param name="size">The maximum row/column to be eliminated.</param>
        /// <returns>
        /// <c>true</c> if the elimination was succesful; otherwise <c>false</c>.
        /// </returns>
        protected abstract void Eliminate(int step, int size);

        /// <summary>
        /// Resets all elements in the matrix.
        /// </summary>
        public override void ResetMatrix()
        {
            base.ResetMatrix();
            IsFactored = false;
        }

        /// <summary>
        /// Clears the system of any elements. The size of the system becomes 0.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            IsFactored = false;
            NeedsReordering = true;
            _order = 0;
            Strategy.Clear();
        }
    }
}
