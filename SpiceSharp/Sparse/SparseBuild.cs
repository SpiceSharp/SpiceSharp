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
            Translate(matrix, ref row, ref col);

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
            Translate(matrix, ref row, ref col);

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
        
        /// <summary>
        /// Translate external indices to internal indices
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="Row">Row index</param>
        /// <param name="Col">Column index</param>
        internal static void Translate(this Matrix matrix, ref int Row, ref int Col)
        {
            int IntRow, IntCol, ExtRow, ExtCol;

            // Begin `Translate'
            ExtRow = Row;
            ExtCol = Col;

            // Expand translation arrays if necessary
            if ((ExtRow > matrix.AllocatedExtSize) || (ExtCol > matrix.AllocatedExtSize))
                ExpandTranslationArrays(matrix, Math.Max(ExtRow, ExtCol));

            // Set Size if necessary. */
            if ((ExtRow > matrix.Size) || (ExtCol > matrix.Size))
                matrix.Size = Math.Max(ExtRow, ExtCol);

            // Translate external row or node number to internal row or node number
            IntRow = matrix.ExtToIntRowMap[ExtRow];
            if (IntRow == -1)
            {
                // We don't have an internal row yet!
                matrix.CurrentSize++;
                matrix.ExtToIntRowMap[ExtRow] = matrix.CurrentSize;
                matrix.ExtToIntColMap[ExtRow] = matrix.CurrentSize;
                IntRow = matrix.CurrentSize;

                // Re-size Matrix if necessary
                if (IntRow > matrix.IntSize)
                    EnlargeMatrix(matrix, IntRow);

                matrix.IntToExtRowMap[IntRow] = ExtRow;
                matrix.IntToExtColMap[IntRow] = ExtRow;
            }

            // Translate external column or node number to internal column or node number
            if ((IntCol = matrix.ExtToIntColMap[ExtCol]) == -1)
            {
                matrix.CurrentSize++;
                matrix.ExtToIntRowMap[ExtCol] = matrix.CurrentSize;
                matrix.ExtToIntColMap[ExtCol] = matrix.CurrentSize;
                IntCol = matrix.CurrentSize;

                // Re-size Matrix if necessary
                if (IntCol > matrix.IntSize)
                    EnlargeMatrix(matrix, IntCol);

                matrix.IntToExtRowMap[IntCol] = ExtCol;
                matrix.IntToExtColMap[IntCol] = ExtCol;
            }

            Row = IntRow;
            Col = IntCol;
        }
        
        /// <summary>
        /// Allocate memory when making a matrix bigger
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="NewSize">Matrix size</param>
        private static void EnlargeMatrix(this Matrix matrix, int NewSize)
        {
            int OldAllocatedSize = matrix.AllocatedSize;
            matrix.IntSize = NewSize;

            if (NewSize <= OldAllocatedSize)
                return;

            // Expand the matrix frame
            NewSize = Math.Max(NewSize, (int)(Matrix.EXPANSION_FACTOR * OldAllocatedSize));
            matrix.AllocatedSize = NewSize;

            Array.Resize(ref matrix.IntToExtColMap, NewSize + 1);
            Array.Resize(ref matrix.IntToExtRowMap, NewSize + 1);
            Array.Resize(ref matrix.Diag, NewSize + 1);
            Array.Resize(ref matrix.FirstInCol, NewSize + 1);
            Array.Resize(ref matrix.FirstInRow, NewSize + 1);

            // Destroy the Markowitz and Intermediate vectors, they will be recreated
            // in spOrderAndFactor().
            matrix.MarkowitzRow = null;
            matrix.MarkowitzCol = null;
            matrix.MarkowitzProd = null;
            matrix.DoRealDirect = null;
            matrix.DoCmplxDirect = null;
            matrix.Intermediate = null;
            matrix.InternalVectorsAllocated = false;

            /* Initialize the new portion of the vectors. */
            for (int I = OldAllocatedSize + 1; I <= NewSize; I++)
            {
                matrix.IntToExtColMap[I] = I;
                matrix.IntToExtRowMap[I] = I;
                matrix.Diag[I] = null;
                matrix.FirstInRow[I] = null;
                matrix.FirstInCol[I] = null;
            }
        }

        /// <summary>
        /// Expand the translation vectors
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="NewSize">Matrix size</param>
        private static void ExpandTranslationArrays(Matrix matrix, int NewSize)
        {
            int OldAllocatedSize = matrix.AllocatedExtSize;
            matrix.Size = NewSize;

            if (NewSize <= OldAllocatedSize)
                return;

            // Expand the translation arrays ExtToIntRowMap and ExtToIntColMap
            NewSize = Math.Max(NewSize, (int)(Matrix.EXPANSION_FACTOR * OldAllocatedSize));
            matrix.AllocatedExtSize = NewSize;

            Array.Resize(ref matrix.ExtToIntRowMap, NewSize + 1);
            Array.Resize(ref matrix.ExtToIntColMap, NewSize + 1);

            // Initialize the new portion of the vectors
            for (int I = OldAllocatedSize + 1; I <= NewSize; I++)
            {
                matrix.ExtToIntRowMap[I] = -1;
                matrix.ExtToIntColMap[I] = -1;
            }

            return;
        }
    }
}
