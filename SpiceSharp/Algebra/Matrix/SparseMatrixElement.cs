using System;

namespace SpiceSharp.Algebra.Matrix
{
    /// <summary>
    /// Element in a matrix
    /// Used by <see cref="SparseMatrix{T}"/> to store values
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    internal class SparseMatrixElement<T> : MatrixElement<T> where T : IFormattable
    {
        /// <summary>
        /// Gets or sets the row index
        /// </summary>
        public new int Row
        {
            get => base.Row;
            set => base.Row = value;
        }

        /// <summary>
        /// Gets or sets the column index
        /// </summary>
        public new int Column
        {
            get => base.Column;
            set => base.Column = value;
        }

        /// <summary>
        /// Next element in the row
        /// </summary>
        public SparseMatrixElement<T> NextInRow { get; set; }

        /// <summary>
        /// Next element in the column
        /// </summary>
        public SparseMatrixElement<T> NextInColumn { get; set; }

        /// <summary>
        /// Previous element in the row
        /// </summary>
        public SparseMatrixElement<T> PreviousInRow { get; set; }

        /// <summary>
        /// Previous element in the column
        /// </summary>
        public SparseMatrixElement<T> PreviousInColumn { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="column">Column index</param>
        public SparseMatrixElement(int row, int column)
        {
            Value = default;
            Row = row;
            Column = column;
        }

        /// <summary>
        /// Get the element above (same column)
        /// </summary>
        public override MatrixElement<T> Above => PreviousInColumn;

        /// <summary>
        /// Get the element below (same column)
        /// </summary>
        public override MatrixElement<T> Below => NextInColumn;

        /// <summary>
        /// Get the element on the right (same row)
        /// </summary>
        public override MatrixElement<T> Right => NextInRow;

        /// <summary>
        /// Get the element on the left (same row)
        /// </summary>
        public override MatrixElement<T> Left => PreviousInRow;
    }
}
