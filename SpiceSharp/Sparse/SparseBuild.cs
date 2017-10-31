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
            for (int i = 1; i <= matrix.Size; i++)
            {
                MatrixElement elt = matrix.FirstInCol[i];
                while (elt != null)
                {
                    elt.Value.Cplx = 0.0;
                    elt = elt.NextInCol;
                }
            }
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
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="row">Row</param>
        /// <param name="col">Column</param>
        /// <returns></returns>
        internal static MatrixElement CreateElement(this Matrix matrix, int row, int col)
        {
            MatrixElement splice = matrix.FirstInCol[col];
            MatrixElement elt;

            // Splice into the column vector while also searching for an existing element
            if (splice == null || splice.Row > row)
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
                while (splice.NextInCol != null && splice.NextInCol.Row < row)
                    splice = splice.NextInCol;
                if (splice.Row == row)
                    elt = splice;
                else
                {
                    elt = new MatrixElement(row, col);
                    elt.NextInCol = splice.NextInCol;
                    splice.NextInCol = elt;

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
            for (int Col = matrix.Size; Col >= 1; Col--)
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

        public static void spClear(this Matrix matrix)
        {
            MatrixElement pElement;

            // Clear matrix
            if (matrix.PreviousMatrixWasComplex || matrix.Complex)
            {
                for (int I = matrix.Size; I > 0; I--)
                {
                    pElement = matrix.FirstInCol[I];
                    while (pElement != null)
                    {
                        pElement.Value.Cplx = 0.0;
                        pElement = pElement.NextInCol;
                    }
                }
            }
            else
            {
                for (int I = matrix.Size; I > 0; I--)
                {
                    pElement = matrix.FirstInCol[I];
                    while (pElement != null)
                    {
                        pElement.Value.Real = 0.0;
                        pElement = pElement.NextInCol;
                    }
                }
            }

            // Empty the trash
            matrix.TrashCan.Value.Cplx = 0.0;

            matrix.Error = Matrix.SparseError.Okay;
            matrix.Factored = false;
            matrix.SingularCol = 0;
            matrix.SingularRow = 0;
            matrix.PreviousMatrixWasComplex = matrix.Complex;
            return;
        }

        public static MatrixElement spGetElement(this Matrix matrix, int Row, int Col)
        {
            if (Row < 0 || Col < 0)
                throw new SparseException("Indices out of bounds");

            if ((Row == 0) || (Col == 0))
                return matrix.TrashCan;

            Translate(matrix, ref Row, ref Col);

            /*
             * The condition part of the following if statement tests to see if the
             * element resides along the diagonal, if it does then it tests to see
             * if the element has been created yet (Diag pointer not NULL).  The
             * pointer to the element is then assigned to Element after it is cast
             * into a pointer to a RealNumber.  This casting makes the pointer into
             * a pointer to Real.  This statement depends on the fact that Real
             * is the first record in the MatrixElement structure.
             */
            MatrixElement pElement;
            if ((Row != Col) || ((pElement = matrix.Diag[Row]) == null))
            {
                /*
                 * Element does not exist or does not reside along diagonal.  Search
                 * column for element.  As in the if statement above, the pointer to the
                 * element which is returned by spcFindElementInCol is cast into a
                 * pointer to Real, a RealNumber.
                 */
                pElement = spcFindElementInCol(matrix, Row, Col, true);
            }
            return pElement;
        }

        public static MatrixElement spcFindElementInCol(Matrix matrix, int Row, int Col, bool CreateIfMissing)
        {
            MatrixElement pElement = matrix.FirstInCol[Col];
            MatrixElement last = null;

            // Search for element
            while (pElement != null)
            {
                if (pElement.Row < Row)
                {
                    // Have not reached element yet
                    last = pElement;
                    pElement = pElement.NextInCol;
                }
                else if (pElement.Row == Row)
                {
                    // Reached element
                    return pElement;
                }
                else
                    break;
            }

            // Element does not exist and must be created
            if (CreateIfMissing)
                return spcCreateElement(matrix, Row, Col, last, false);
            else return null;
        }

        internal static void Translate(this Matrix matrix, ref int Row, ref int Col)
        {
            int IntRow, IntCol, ExtRow, ExtCol;

            // Begin `Translate'
            ExtRow = Row;
            ExtCol = Col;

            // Expand translation arrays if necessary
            if ((ExtRow > matrix.AllocatedExtSize) || (ExtCol > matrix.AllocatedExtSize))
                ExpandTranslationArrays(matrix, Math.Max(ExtRow, ExtCol));

            // Set ExtSize if necessary. */
            if ((ExtRow > matrix.ExtSize) || (ExtCol > matrix.ExtSize))
                matrix.ExtSize = Math.Max(ExtRow, ExtCol);

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
                if (IntRow > matrix.Size)
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
                if (IntCol > matrix.Size)
                    EnlargeMatrix(matrix, IntCol);

                matrix.IntToExtRowMap[IntCol] = ExtCol;
                matrix.IntToExtColMap[IntCol] = ExtCol;
            }

            Row = IntRow;
            Col = IntCol;
        }

        public static MatrixElement spcCreateElement(this Matrix matrix, int Row, int Col, MatrixElement LastAddr, bool Fillin)
        {
            MatrixElement pElement, pCreatedElement, pLastElement;

            if (matrix.RowsLinked)
            {
                // Row pointers cannot be ignored
                if (Fillin)
                {
                    pElement = new MatrixElement(Row, Col);
                    matrix.Fillins++;
                }
                else
                {
                    pElement = new MatrixElement(Row, Col);
                    matrix.NeedsOrdering = true;
                }
                if (pElement == null)
                    return null;

                // If element is on diagonal, store pointer in Diag
                if (Row == Col)
                    matrix.Diag[Row] = pElement;

                // Initialize Element
                pCreatedElement = pElement;
                pElement.Row = Row;
                pElement.Col = Col;
                pElement.Value.Cplx = 0.0;

                // Splice element into column
                if (LastAddr != null)
                {
                    pElement.NextInCol = LastAddr.NextInCol;
                    LastAddr.NextInCol = pElement;
                }
                else
                {
                    pElement.NextInCol = matrix.FirstInCol[Col];
                    matrix.FirstInCol[Col] = pElement;
                }

                // Search row for proper element position
                pElement = matrix.FirstInRow[Row];
                pLastElement = null;
                while (pElement != null)
                {
                    // Search for element row position
                    if (pElement.Col < Col)
                    {
                        // Have not reached desired element
                        pLastElement = pElement;
                        pElement = pElement.NextInRow;
                    }
                    else
                        pElement = null;
                }

                // Splice element into row
                pElement = pCreatedElement;
                if (pLastElement == null)
                {
                    // Element is first in row
                    pElement.NextInRow = matrix.FirstInRow[Row];
                    matrix.FirstInRow[Row] = pElement;
                }
                else
                {
                    // Element is not first in row
                    pElement.NextInRow = pLastElement.NextInRow;
                    pLastElement.NextInRow = pElement;
                }

            }
            else
            {
                // Matrix has not been factored yet.  Thus get element rather than fill-in.
                // Also, row pointers can be ignored.

                // Allocate memory for Element
                pElement = new MatrixElement(Row, Col);

                // If element is on diagonal, store pointer in Diag
                if (Row == Col)
                    matrix.Diag[Row] = pElement;

                // Initialize Element
                pCreatedElement = pElement;
                pElement.Col = Col;
                pElement.Row = Row;
                pElement.Value.Cplx = 0.0;

                // Splice element into column
                if (LastAddr != null)
                {
                    pElement.NextInCol = LastAddr.NextInCol;
                    LastAddr.NextInCol = pElement;
                }
                else
                {
                    pElement.NextInCol = matrix.FirstInCol[Col];
                    matrix.FirstInCol[Col] = pElement;
                }
            }

            matrix.Elements++;
            return pCreatedElement;
        }

        public static void spcLinkRows(this Matrix matrix)
        {
            for (int Col = matrix.Size; Col >= 1; Col--)
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
        /// Allocate memory when making a matrix bigger
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="NewSize">Matrix size</param>
        private static void EnlargeMatrix(this Matrix matrix, int NewSize)
        {
            int OldAllocatedSize = matrix.AllocatedSize;
            matrix.Size = NewSize;

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
            matrix.ExtSize = NewSize;

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
