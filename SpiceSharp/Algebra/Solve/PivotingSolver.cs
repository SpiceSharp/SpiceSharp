using System;
using System.Globalization;
using System.Text;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// A standard implementation for pivoting solvers.
    /// </summary>
    /// <typeparam name="M">The matrix type.</typeparam>
    /// <typeparam name="V">The vector type.</typeparam>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IPivotingSolver{M, V, T}" />
    public abstract class PivotingSolver<M, V, T> : Parameterized, IPivotingSolver<M, V, T>
        where M : IMatrix<T>
        where V : IVector<T>
        where T : IFormattable
    {
        private int _degeneracy = 0;
        private int _pivotLimit = 0;

        /// <summary>
        /// Gets or sets the degeneracy of the matrix. For example, specifying 1 will let the solver know that one equation is
        /// expected to be linearly dependent on the others.
        /// </summary>
        /// <value>
        /// The degeneracy.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the degeneracy is negative.</exception>
        public int Degeneracy
        {
            get => _degeneracy;
            set
            {
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Degeneracy), value, 0));
                _degeneracy = value;
            }
        }

        /// <summary>
        /// Gets or sets the pivot search reduction. This makes sure that pivots cannot
        /// be chosen from the last N rows. The default, 0, lets the pivot strategy to
        /// choose from the whole matrix.
        /// </summary>
        /// <value>
        /// The pivot search reduction.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the pivot search reduction is negative.</exception>
        public int PivotSearchReduction
        {
            get => _pivotLimit;
            set
            {
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(PivotSearchReduction), value, 0));
                _pivotLimit = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this solver has been factored.
        /// A solver needs to be factored becore it can solve for a solution.
        /// </summary>
        /// <value>
        /// <c>true</c> if this solver is factored; otherwise, <c>false</c>.
        /// </value>
        public bool IsFactored { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the solver needs to be reordered all the way from the start.
        /// </summary>
        /// <value>
        /// <c>true</c> if the solver needs reordering; otherwise, <c>false</c>.
        /// </value>
        public bool NeedsReordering { get; set; }

        /// <summary>
        /// Gets the order of the matrix (matrix size).
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Size => Math.Max(Matrix.Size, Vector.Length);

        /// <summary>
        /// Gets or sets the matrix value at the specified row and column.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The value.</returns>
        public T this[int row, int column]
        {
            get => this[new MatrixLocation(row, column)];
            set => this[new MatrixLocation(row, column)] = value;
        }

        /// <summary>
        /// Gets or sets the value of the matrix at the specified location.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="location">The location.</param>
        /// <returns>The value.</returns>
        public T this[MatrixLocation location]
        {
            get => Matrix[ExternalToInternal(location)];
            set => Matrix[ExternalToInternal(location)] = value;
        }

        /// <summary>
        /// Gets or sets the vector value at the specified index.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>Value</returns>
        public T this[int index]
        {
            get
            {
                index = Row[index];
                return Vector[index];
            }
            set
            {
                index = Row[index];
                Vector[index] = value;
            }
        }

        /// <summary>
        /// Gets the row translation.
        /// </summary>
        protected Translation Row { get; } = new Translation();

        /// <summary>
        /// Gets the column translation.
        /// </summary>
        protected Translation Column { get; } = new Translation();

        /// <summary>
        /// Gets the matrix to work on.
        /// </summary>
        protected M Matrix { get; }

        /// <summary>
        /// Gets the right-hand side vector.
        /// </summary>
        protected V Vector { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PivotingSolver{M, V, T}"/> class.
        /// </summary>
        protected PivotingSolver(M matrix, V vector)
        {
            Matrix = matrix;
            Vector = vector;
            NeedsReordering = true;
            IsFactored = false;
        }

        /// <summary>
        /// Preconditions the solver matrix and right hand side vector.
        /// </summary>
        /// <param name="method">The method.</param>
        public abstract void Precondition(PreconditioningMethod<M, V, T> method);

        /// <summary>
        /// Swap two (internal) rows in the linear system. This method keeps
        /// the matrix and vector synchronized.
        /// </summary>
        /// <param name="row1">The first row index.</param>
        /// <param name="row2">The second row index.</param>
        public void SwapRows(int row1, int row2)
        {
            Matrix.SwapRows(row1, row2);
            Vector.SwapElements(row1, row2);
            Row.Swap(row1, row2);
        }

        /// <summary>
        /// Swap two (internal) columns in the system.
        /// </summary>
        /// <param name="column1">The first column index.</param>
        /// <param name="column2">The second column index.</param>
        public void SwapColumns(int column1, int column2)
        {
            Matrix.SwapColumns(column1, column2);
            Column.Swap(column1, column2);
        }

        /// <summary>
        /// Resets all elements in the matrix and vector.
        /// </summary>
        public virtual void Reset()
        {
            Matrix.Reset();
            Vector.Reset();
            IsFactored = false;
        }

        /// <summary>
        /// Clears the system of any elements. The size of the system becomes 0.
        /// </summary>
        public virtual void Clear()
        {
            Matrix.Clear();
            Vector.Clear();
            Row.Clear();
            Column.Clear();
            IsFactored = false;
            NeedsReordering = true;
            Degeneracy = 0;
        }

        /// <summary>
        /// Maps an external row/column tuple to an internal one.
        /// </summary>
        /// <param name="indices">The external row/column indices.</param>
        /// <returns>
        /// The internal row/column indices.
        /// </returns>
        public MatrixLocation ExternalToInternal(MatrixLocation indices)
        {
            return new MatrixLocation(Row[indices.Row], Column[indices.Column]);
        }

        /// <summary>
        /// Maps an internal row/column tuple to an external one.
        /// </summary>
        /// <param name="indices">The internal row/column indices.</param>
        /// <returns>
        /// The external row/column indices.
        /// </returns>
        public MatrixLocation InternalToExternal(MatrixLocation indices)
        {
            return new MatrixLocation(Row.Reverse(indices.Row), Column.Reverse(indices.Column));
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder sb = new StringBuilder();
            for (var r = 1; r <= Size; r++)
            {
                sb.Append("[ ");
                for (var c = 1; c <= Size; c++)
                {
                    sb.Append(this[r, c].ToString(format, formatProvider));
                    sb.Append(" ");
                }
                sb.Append(this[r].ToString(format, formatProvider));
                sb.Append(" ]");
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
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
        /// This method can save time when factoring similar matrices in succession.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the factoring was successful; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool Factor();

        /// <summary>
        /// Order and factor the Y-matrix and Rhs-vector.
        /// This method will reorder the matrix as it sees fit.
        /// </summary>
        /// <returns>
        /// The number of rows that were successfully eliminated.
        /// </returns>
        public abstract int OrderAndFactor();
    }
}
