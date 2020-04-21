using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// An base class for dense linear systems that can be solved using LU decomposition.
    /// Pivoting is controlled by the 
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IMatrix{T}" />
    /// <seealso cref="IVector{T}" />
    /// <seealso cref="IFormattable" />
    public abstract partial class DenseLUSolver<T> : PivotingSolver<IMatrix<T>, IVector<T>, T>, ISolver<T>, IParameterized<RookPivoting<T>>
        where T : IFormattable
    {
        /// <summary>
        /// Gets the pivoting strategy.
        /// </summary>
        /// <value>
        /// The pivoting strategy.
        /// </value>
        public RookPivoting<T> Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseLUSolver{T}"/> class.
        /// </summary>
        /// <param name="magnitude">The magnitude.</param>
        protected DenseLUSolver(Func<T, double> magnitude)
            : base(new DenseMatrix<T>(), new DenseVector<T>())
        {
            NeedsReordering = true;
            Parameters = new RookPivoting<T>(magnitude);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseLUSolver{T}"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="magnitude">The magnitude.</param>
        protected DenseLUSolver(int size, Func<T, double> magnitude)
            : base(new DenseMatrix<T>(size), new DenseVector<T>(size))
        {
            NeedsReordering = true;
            Parameters = new RookPivoting<T>(magnitude);
        }

        /// <summary>
        /// Preconditions the solver matrix and right hand side vector.
        /// </summary>
        /// <param name="method">The method.</param>
        public override void Precondition(PreconditioningMethod<IMatrix<T>, IVector<T>, T> method)
        {
            var reorderedMatrix = new ReorderedMatrix(this);
            var reorderedVector = new ReorderedVector(this);
            method(reorderedMatrix, reorderedVector);
        }

        /// <summary>
        /// Factor the Y-matrix and Rhs-vector.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the factoring was successful; otherwise <c>false</c>.
        /// </returns>
        public override bool Factor() => Factor(Size);

        /// <summary>
        /// Factor they Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="size">The size of the matrix that should be factored.</param>
        /// <returns>
        /// <c>true</c> if the factoring was successful; otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This factoring method allows reusing matrices if only a small matrix is needed.
        /// </remarks>
        public bool Factor(int size)
        {
            int order = Math.Min(size, Size) - Degeneracy;
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
        public override int OrderAndFactor()
        {
            var size = Size;
            var step = 1;
            var order = Size - Degeneracy;
            if (!NeedsReordering)
            {
                for (step = 1; step <= order; step++)
                {
                    if (Parameters.IsValidPivot(Matrix, step))
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

            for (; step <= order; step++)
            {
                if (!Parameters.FindPivot(Matrix, step, out int row, out int column))
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
        /// Clears the system of any elements. The size of the system becomes 0.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            PivotSearchReduction = 0;
        }
    }
}
