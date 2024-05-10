using SpiceSharp.ParameterSets;
using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// A standard implementation for pivoting solvers.
    /// </summary>
    /// <typeparam name="M">The matrix type.</typeparam>
    /// <typeparam name="V">The vector type.</typeparam>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IPivotingSolver{M, V, T}" />
    public abstract partial class PivotingSolver<M, V, T> : ParameterSetCollection,
        IPivotingSolver<M, V, T>
        where M : IMatrix<T>
        where V : IVector<T>
    {
        /// <inheritdoc/>
        [GreaterThanOrEquals(0)]
        private int _pivotSearchReduction = 0;

        /// <inheritdoc/>
        [GreaterThanOrEquals(0)]
        private int _degeneracy = 0;

        /// <inheritdoc/>
        public bool IsFactored { get; protected set; }

        /// <inheritdoc/>
        public bool NeedsReordering { get; set; }

        /// <inheritdoc/>
        public int Size => Math.Max(Matrix.Size, Vector.Length);

        /// <inheritdoc/>
        public T this[int row, int column]
        {
            get => this[new MatrixLocation(row, column)];
            set => this[new MatrixLocation(row, column)] = value;
        }

        /// <inheritdoc/>
        public T this[MatrixLocation location]
        {
            get => Matrix[ExternalToInternal(location)];
            set => Matrix[ExternalToInternal(location)] = value;
        }

        /// <inheritdoc/>
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
        /// <value>
        /// The row translation.
        /// </value>
        protected Translation Row { get; } = new Translation();

        /// <summary>
        /// Gets the column translation.
        /// </summary>
        /// <value>
        /// The column translation.
        /// </value>
        protected Translation Column { get; } = new Translation();

        /// <summary>
        /// Gets the reordered equation matrix.
        /// </summary>
        /// <value>
        /// The reordered equation matrix.
        /// </value>
        protected M Matrix { get; }

        /// <summary>
        /// Gets the reordered right hand side vector.
        /// </summary>
        /// <value>
        /// The reordered right hand side vector.
        /// </value>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public virtual void Reset()
        {
            Matrix.Reset();
            Vector.Reset();
            IsFactored = false;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public MatrixLocation ExternalToInternal(MatrixLocation indices)
        {
            return new MatrixLocation(Row[indices.Row], Column[indices.Column]);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public abstract bool Factor();

        /// <inheritdoc/>
        public abstract int OrderAndFactor();

        /// <inheritdoc/>
        public new ISolver<T> SetParameter<P>(string name, P value)
        {
            base.SetParameter(name, value);
            return this;
        }

        /// <inheritdoc />
        public abstract void ForwardSubstitute(IVector<T> solution);

        /// <inheritdoc />
        public abstract void BackwardSubstitute(IVector<T> solution);

        /// <inheritdoc />
        public abstract T ComputeDegenerateContribution(int index);

        /// <inheritdoc />
        public abstract void ForwardSubstituteTransposed(IVector<T> solution);

        /// <inheritdoc />
        public abstract void BackwardSubstituteTransposed(IVector<T> solution);

        /// <inheritdoc />
        public abstract T ComputeDegenerateContributionTransposed(int index);
    }
}
