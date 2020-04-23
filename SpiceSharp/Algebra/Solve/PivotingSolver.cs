using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// A standard implementation for pivoting solvers.
    /// </summary>
    /// <typeparam name="M">The matrix type.</typeparam>
    /// <typeparam name="V">The vector type.</typeparam>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IPivotingSolver{M, V, T}" />
    [GeneratedParameters]
    public abstract class PivotingSolver<M, V, T> : Parameterized, IPivotingSolver<M, V, T>
        where M : IMatrix<T>
        where V : IVector<T>
    {
        private int _pivotSearchReduction = 0;
        private int _degeneracy = 0;

        /// <summary>
        /// Gets or sets the degeneracy of the matrix. For example, specifying 1 will let the solver know that one equation is
        /// expected to be linearly dependent on the others.
        /// </summary>
        /// <value>
        /// The degeneracy.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is negative.</exception>
        [GreaterThanOrEquals(0)]
        public int Degeneracy
        {
            get => _degeneracy;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Degeneracy), 0);
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
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is negative.</exception>
        [GreaterThanOrEquals(0)]
        public int PivotSearchReduction
        {
            get => _pivotSearchReduction;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(PivotSearchReduction), 0);
                _pivotSearchReduction = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this solver has been factored.
        /// A solver needs to be factored becore it can solve for a solution.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this solver is factored; otherwise, <c>false</c>.
        /// </value>
        public bool IsFactored { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the solver needs to be reordered all the way from the start.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the solver needs reordering; otherwise, <c>false</c>.
        /// </value>
        public bool NeedsReordering { get; set; }

        /// <summary>
        /// Gets the size of the solver. This is the total number of equations.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int Size => Math.Max(Matrix.Size, Vector.Length);

        /// <summary>
        /// Gets or sets the value of the matrix at the specified row and column.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>
        /// The value.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="row"/> or <paramref name="column"/> is negative.</exception>
        public T this[int row, int column]
        {
            get => this[new MatrixLocation(row, column)];
            set => this[new MatrixLocation(row, column)] = value;
        }

        /// <summary>
        /// Gets or sets the value of the matrix at the specified location.
        /// </summary>
        /// <value>
        /// The value of the matrix element.
        /// </value>
        /// <param name="location">The location.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public T this[MatrixLocation location]
        {
            get => Matrix[ExternalToInternal(location)];
            set => Matrix[ExternalToInternal(location)] = value;
        }

        /// <summary>
        /// Gets or sets the value of the right hand side vector at the specified row.
        /// </summary>
        /// <value>
        /// The value of the right hand side vector.
        /// </value>
        /// <param name="row">The row.</param>
        /// <returns>
        /// The value.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="row"/> is negative.</exception>
        public T this[int row]
        {
            get
            {
                row = Row[row];
                return Vector[row];
            }
            set
            {
                row = Row[row];
                Vector[row] = value;
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
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="row1"/> or <paramref name="row2"/> is not greater than 0.
        /// </exception>
        protected void SwapRows(int row1, int row2)
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
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="column1"/> or <paramref name="column2"/> is not greater than 0.
        /// </exception>
        protected void SwapColumns(int column1, int column2)
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
        /// Maps an external matrix location to an internal one.
        /// </summary>
        /// <param name="indices">The external matrix location.</param>
        /// <returns>
        /// The internal matrix location.
        /// </returns>
        public MatrixLocation ExternalToInternal(MatrixLocation indices)
        {
            return new MatrixLocation(Row[indices.Row], Column[indices.Column]);
        }

        /// <summary>
        /// Maps an internal matrix location to an external one.
        /// </summary>
        /// <param name="indices">The internal matrix location.</param>
        /// <returns>
        /// The external matrix location.
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
        public override string ToString() => "Pivoting solver ({0}x{1})".FormatString(Size, Size + 1);

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

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The current instance for chaining.
        /// </returns>
        public new ISolver<T> SetParameter<P>(string name, P value)
        {
            base.SetParameter(name, value);
            return this;
        }

        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <returns>
        /// The current instance for chaining.
        /// </returns>
        public new ISolver<T> SetParameter(string name)
        {
            base.SetParameter(name);
            return this;
        }
    }
}
