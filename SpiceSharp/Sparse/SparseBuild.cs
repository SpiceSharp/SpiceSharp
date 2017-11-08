using System;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Methods for building a Sparse Matrix
    /// </summary>
    public static class SparseBuild
    {
        /// <summary>
        /// Clear all elements of a matrix
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public static void Clear(this Matrix matrix)
        {
            for (int i = 1; i <= matrix.IntSize; i++)
            {
                MatrixElement elt = matrix.FirstInCol[i];
                while (elt != null)
                {
                    elt.Value.Cplx = 0.0;
                    elt = elt.NextInCol;
                }
            }

            // Reset flags
            matrix.Error = SparseError.Okay;
            matrix.Factored = false;
            matrix.SingularCol = 0;
            matrix.SingularRow = 0;
        }

        /// <summary>
        /// Get an element from the matrix
        /// If it does not find the element in the matrix, it will create it!
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="row">Row</param>
        /// <param name="col">Column</param>
        /// <returns></returns>
        public static MatrixElement GetElement(this Matrix matrix, int row, int col)
        {
            if (row < 0 || col < 0)
                throw new SparseException("Index out of bounds");

            // Trash
            if (row == 0 || col == 0)
                return matrix.TrashCan;

            // Translate external indices to internal indices
            matrix.Translation.Translate(matrix, ref row, ref col);

            // Quickly access diagonal
            MatrixElement elt;
            if (row != col || (elt = matrix.Diag[row]) == null)
            {
                // We have to find the element or create it!
                elt = CreateElement(matrix, row, col);
            }
            return elt;
        }

        /// <summary>
        /// Find an element in the matrix without creating it
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="row">Row</param>
        /// <param name="col">Column</param>
        /// <returns></returns>
        public static MatrixElement FindElement(this Matrix matrix, int row, int col)
        {
            if (row < 0 || col < 0)
                throw new SparseException("Index out of bounds");

            if (row == 0 || col == 0)
                return matrix.TrashCan;

            // Translate external indices to internal indices
            matrix.Translation.Translate(matrix, ref row, ref col);

            // Find the element at the right place
            MatrixElement elt = matrix.FirstInCol[col];
            while (elt != null)
            {
                if (elt.Row < row)
                {
                    // Next one maybe?
                    elt = elt.NextInCol;
                }
                else if (elt.Row == row)
                {
                    // Found it!
                    return elt;
                }
                else
                {
                    // Aww...
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Create a new element in the matrix if it doesn't exist
        /// Only used internally!
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="row">Row</param>
        /// <param name="col">Column</param>
        /// <returns></returns>
        internal static MatrixElement CreateElement(this Matrix matrix, int row, int col)
        {
            MatrixElement elt = matrix.FirstInCol[col], last = null;

            // Splice into the column vector while also searching for an existing element
            if (elt == null || elt.Row > row)
            {
                // There are no elements yet in the column
                elt = new MatrixElement(row, col);
                elt.NextInCol = matrix.FirstInCol[col];
                matrix.FirstInCol[col] = elt;
                if (row == col)
                    matrix.Diag[row] = elt;

                if (matrix.RowsLinked)
                    SpliceInRows(matrix, elt);
            }
            else
            {
                // Find the insert point
                while (elt != null && elt.Row < row)
                {
                    last = elt;
                    elt = elt.NextInCol;
                }

                // If the element does not exist yet, create it
                if (elt == null || elt.Row != row)
                {
                    elt = new MatrixElement(row, col);
                    elt.NextInCol = last.NextInCol;
                    last.NextInCol = elt;

                    if (row == col)
                        matrix.Diag[row] = elt;

                    // Splice it in the row vector
                    if (matrix.RowsLinked)
                        SpliceInRows(matrix, elt);
                }
            }
            return elt;
        }

        /// <summary>
        /// Build the row links
        /// </summary>
        /// <param name="matrix">Matrix</param>
        internal static void LinkRows(this Matrix matrix)
        {
            for (int Col = matrix.IntSize; Col >= 1; Col--)
            {
                // Generate row links for the elements in the Col'th column
                MatrixElement pElement = matrix.FirstInCol[Col];

                while (pElement != null)
                {
                    pElement.Col = Col;
                    pElement.NextInRow = matrix.FirstInRow[pElement.Row];
                    matrix.FirstInRow[pElement.Row] = pElement;
                    pElement = pElement.NextInCol;
                }
            }
            matrix.RowsLinked = true;
            return;
        }

        /// <summary>
        /// Splice a matrix element in the row vectors
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="elt">Element</param>
        private static void SpliceInRows(Matrix matrix, MatrixElement elt)
        {
            int row = elt.Row;
            int col = elt.Col;

            MatrixElement splice = matrix.FirstInRow[row];
            if (splice == null || splice.Col > col)
            {
                elt.NextInRow = matrix.FirstInRow[row];
                matrix.FirstInRow[row] = elt;
            }
            else
            {
                while (splice.NextInRow != null && splice.NextInRow.Col < col)
                    splice = splice.NextInRow;
                elt.NextInRow = splice.NextInRow;
                splice.NextInRow = elt;
            }
        }
    }
}
