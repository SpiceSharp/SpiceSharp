using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// A pivot strategy used by a <see cref="SparseLUSolver{M, V, T}" />
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public abstract class SparsePivotStrategy<T> where T : IFormattable
    {
        /// <summary>
        /// Gets the magnitude method.
        /// </summary>
        public Func<T, double> Magnitude { get; private set; }

        /// <summary>
        /// Gets or sets the region for reordering the matrix. For example, specifying 1 will avoid a pivot from being chosen from
        /// the last row or column.
        /// </summary>
        /// <value>
        /// The pivot search reduction.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the pivot search reduction is negative.</exception>
        public int PivotSearchReduction
        {
            get => _search;
            set
            {
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Algebra_InvalidPivotSearchReduction);
                _search = value;
            }
        }
        private int _search = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparsePivotStrategy{T}"/> class.
        /// </summary>
        /// <param name="magnitude">The magnitude function.</param>
        protected SparsePivotStrategy(Func<T, double> magnitude)
        {
            Magnitude = magnitude.ThrowIfNull(nameof(magnitude));
        }

        /// <summary>
        /// This method will check whether or not a pivot element is valid or not.
        /// It checks for the submatrix right/below of the pivot.
        /// </summary>
        /// <param name="pivot">The pivot candidate.</param>
        /// <returns>True if the pivot can be used.</returns>
        public abstract bool IsValidPivot(ISparseMatrixElement<T> pivot);

        /// <summary>
        /// Setup the pivot strategy.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <param name="eliminationStep">The current elimination step.</param>
        public abstract void Setup(ISparseMatrix<T> matrix, ISparseVector<T> rhs, int eliminationStep);

        /// <summary>
        /// Move the pivot to the diagonal for this elimination step.
        /// </summary>
        /// <remarks>
        /// This is done by swapping the rows and columns of the diagonal and that of the pivot.
        /// </remarks>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <param name="pivot">The pivot element.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        public abstract void MovePivot(ISparseMatrix<T> matrix, ISparseVector<T> rhs, ISparseMatrixElement<T> pivot, int eliminationStep);

        /// <summary>
        /// Update the strategy after the pivot was moved.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="pivot">The pivot element.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        public abstract void Update(ISparseMatrix<T> matrix, ISparseMatrixElement<T> pivot, int eliminationStep);

        /// <summary>
        /// Notifies the strategy that a fill-in has been created
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="fillin">The fill-in.</param>
        public abstract void CreateFillin(ISparseMatrix<T> matrix, ISparseMatrixElement<T> fillin);

        /// <summary>
        /// Finds a pivot in the matrix.
        /// </summary>
        /// <remarks>
        /// The pivot should be searched for in the submatrix towards the right and down of the
        /// current diagonal at row/column <paramref name="eliminationStep"/>. This pivot element
        /// will be moved to the diagonal for this elimination step.
        /// </remarks>
        /// <param name="matrix">The matrix.</param>
        /// <param name="eliminationStep">The current elimination step.</param>
        /// <returns>The chosen pivot.</returns>
        public abstract ISparseMatrixElement<T> FindPivot(ISparseMatrix<T> matrix, int eliminationStep);

        /// <summary>
        /// Clears the pivot strategy.
        /// </summary>
        public virtual void Clear()
        {
        }
    }
}
