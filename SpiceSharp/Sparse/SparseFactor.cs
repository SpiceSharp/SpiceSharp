using System;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Extension methods for factoring a sparse matrix
    /// </summary>
    public static class SparseFactor
    {
        /// <summary>
        /// Order and factor the matrix
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Right-Hand Side</param>
        /// <param name="relativeThreshold">Relative threshold for pivot selection</param>
        /// <param name="absoluteThreshold">Absolute threshold for pivot selection</param>
        /// <param name="diagonalPivoting">Use diagonal pivoting</param>
        /// <returns></returns>
        public static SparseError OrderAndFactor(this Matrix matrix, Vector<double> rhs, double relativeThreshold, double absoluteThreshold, bool diagonalPivoting)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            var pivoting = matrix.Pivoting;
            MatrixElement pPivot;
            int Step, Size;
            bool ReorderingRequired;
            double LargestInCol;

            if (matrix.Factored)
                throw new SparseException("Matrix is already factored");

            matrix.Error = SparseError.Okay;
            Size = matrix.IntSize;
            if (relativeThreshold <= 0.0)
                relativeThreshold = matrix.RelThreshold;
            if (relativeThreshold > 1.0)
                relativeThreshold = matrix.RelThreshold;
            matrix.RelThreshold = relativeThreshold;
            if (absoluteThreshold < 0.0)
                absoluteThreshold = matrix.AbsThreshold;
            matrix.AbsThreshold = absoluteThreshold;
            ReorderingRequired = false;

            if (!matrix.NeedsOrdering)
            {
                // Matrix has been factored before and reordering is not required. 
                for (Step = 1; Step <= Size; Step++)
                {
                    pPivot = matrix.Diag[Step];
                    LargestInCol = SparsePivoting.FindLargestInCol(pPivot.NextInColumn);
                    if (LargestInCol * relativeThreshold < pPivot.Value.Magnitude)
                    {
                        if (matrix.Complex)
                            ComplexRowColElimination(matrix, pPivot);
                        else
                            RealRowColElimination(matrix, pPivot);
                    }
                    else
                    {
                        ReorderingRequired = true;
                        break; // for loop 
                    }
                }
                if (!ReorderingRequired)
                    goto Done;
                else
                {
                    // A pivot was not large enough to maintain accuracy,
                    // so a partial reordering is required.
                }
            } // End of if(!matrix.NeedsOrdering)
            else
            {

                // This is the first time the matrix has been factored.  These few statements
                // indicate to the rest of the code that a full reodering is required rather
                // than a partial reordering, which occurs during a failure of a fast
                // factorization.
                Step = 1;
                if (!matrix.RowsLinked)
                    matrix.LinkRows();
                if (!matrix.Pivoting.InternalVectorsAllocated)
                    matrix.Pivoting.CreateInternalVectors(matrix.IntSize);
                if ((int)matrix.Error >= (int)SparseError.Fatal)
                    return matrix.Error;
            }

            // Form initial Markowitz products. 
            pivoting.CountMarkowitz(matrix, rhs, Step);
            pivoting.MarkowitzProducts(matrix, Step);
            matrix.MaxRowCountInLowerTri = -1;

            // Perform reordering and factorization. 
            for (; Step <= Size; Step++)
            {
                pPivot = pivoting.SearchForPivot(matrix, Step, diagonalPivoting);
                if (pPivot == null)
                    return MatrixIsSingular(matrix, Step);
                ExchangeRowsAndCols(matrix, pPivot, Step);

                if (matrix.Complex)
                    ComplexRowColElimination(matrix, pPivot);
                else
                    RealRowColElimination(matrix, pPivot);

                if ((int)matrix.Error >= (int)SparseError.Fatal)
                    return matrix.Error;
                pivoting.UpdateMarkowitzNumbers(matrix, pPivot);
            }

            Done:
            matrix.NeedsOrdering = false;
            matrix.Reordered = true;
            matrix.Factored = true;
            return matrix.Error;
        }

        /// <summary>
        /// Factor the matrix
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <returns></returns>
        public static SparseError Factor(this Matrix matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            var pivoting = matrix.Pivoting;

            if (matrix.Factored)
                throw new SparseException("Matrix is factored");
            MatrixElement pElement, pColumn;

            if (matrix.NeedsOrdering)
                return OrderAndFactor(matrix, null, 0.0, 0.0, Matrix.DIAG_PIVOTING_AS_DEFAULT);
            if (!pivoting.Partitioned)
                pivoting.Partition(matrix, SparsePartition.Default);
            if (matrix.Complex)
                return FactorComplexMatrix(matrix);

            int Size = matrix.IntSize;

            if (matrix.Diag[1].Value.Real == 0.0)
                return ZeroPivot(matrix, 1);
            matrix.Diag[1].Value.Real = 1.0 / matrix.Diag[1].Value.Real;

            // Start factorization
            for (int Step = 2; Step <= Size; Step++)
            {
                if (pivoting.DoRealDirect[Step])
                {
                    // Update column using direct addressing scatter-gather
                    ElementValue[] Dest = pivoting.Intermediate;

                    // Scatter
                    pElement = matrix.FirstInCol[Step];
                    while (pElement != null)
                    {
                        Dest[pElement.Row] = pElement;
                        pElement = pElement.NextInColumn;
                    }

                    // Update column
                    pColumn = matrix.FirstInCol[Step];
                    while (pColumn.Row < Step)
                    {
                        pElement = matrix.Diag[pColumn.Row];
                        pColumn.Value.Real = Dest[pColumn.Row] * pElement.Value.Real;
                        while ((pElement = pElement.NextInColumn) != null)
                            Dest[pElement.Row].Real -= pColumn.Value.Real * pElement.Value.Real;
                        pColumn = pColumn.NextInColumn;
                    }

                    // Gather
                    pElement = matrix.Diag[Step].NextInColumn;
                    while (pElement != null)
                    {
                        pElement.Value.Real = Dest[pElement.Row];
                        pElement = pElement.NextInColumn;
                    }

                    // Check for singular matrix
                    if (Dest[Step] == 0.0)
                        return ZeroPivot(matrix, Step);
                    matrix.Diag[Step].Value.Real = 1.0 / Dest[Step];
                }
                else
                {
                    // Update column using indirect addressing scatter-gather
                    MatrixElement[] pDest = new MatrixElement[matrix.Diag.Length];

                    // Scatter
                    pElement = matrix.FirstInCol[Step];
                    while (pElement != null)
                    {
                        pDest[pElement.Row] = pElement;
                        pElement = pElement.NextInColumn;
                    }

                    // Update column
                    pColumn = matrix.FirstInCol[Step];
                    while (pColumn.Row < Step)
                    {
                        pElement = matrix.Diag[pColumn.Row];
                        double Mult = (pDest[pColumn.Row].Value.Real *= pElement.Value.Real);
                        while ((pElement = pElement.NextInColumn) != null)
                            pDest[pElement.Row].Value.Real -= Mult * pElement.Value.Real;
                        pColumn = pColumn.NextInColumn;
                    }

                    // Check for singular matrix
                    if (matrix.Diag[Step].Value.Real == 0.0)
                        return ZeroPivot(matrix, Step);
                    matrix.Diag[Step].Value.Real = 1.0 / matrix.Diag[Step].Value.Real;
                }
            }

            matrix.Factored = true;
            return (matrix.Error = SparseError.Okay);
        }

        /// <summary>
        /// Factor the matrix in the complex domain
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <returns></returns>
        private static SparseError FactorComplexMatrix(Matrix matrix)
        {
            var pivoting = matrix.Pivoting;
            MatrixElement pElement, pColumn;
            int Step, Size;
            ElementValue Mult = new ElementValue();
            ElementValue Pivot;

            if (!matrix.Complex)
                throw new SparseException("Matrix is not complex");

            Size = matrix.IntSize;
            pElement = matrix.Diag[1];
            if (pElement.Value.Magnitude.Equals(0.0))
                return ZeroPivot(matrix, 1);

            // Cmplx expr: *pPivot = 1.0 / *pPivot
            pElement.Value.CopyReciprocal(pElement);

            // Start factorization
            for (Step = 2; Step <= Size; Step++)
            {
                if (pivoting.DoCmplxDirect[Step])
                {
                    // Update column using direct addressing scatter-gather
                    ElementValue[] Dest = pivoting.Intermediate;

                    // Scatter
                    pElement = matrix.FirstInCol[Step];
                    while (pElement != null)
                    {
                        Dest[pElement.Row] = pElement;
                        pElement = pElement.NextInColumn;
                    }

                    // Update column
                    pColumn = matrix.FirstInCol[Step];
                    while (pColumn.Row < Step)
                    {
                        pElement = matrix.Diag[pColumn.Row];

                        // Cmplx expr: Mult = Dest[pColumn.Row] * (1.0 / *pPivot)
                        Mult.CopyMultiply(Dest[pColumn.Row], pElement);
                        pColumn.Value.CopyFrom(Mult);
                        while ((pElement = pElement.NextInColumn) != null)
                        {
                            // Cmplx expr: Dest[pElement.Row] -= Mult * pElement
                            Dest[pElement.Row].SubtractMultiply(Mult, pElement);
                        }
                        pColumn = pColumn.NextInColumn;
                    }

                    // Gather
                    pElement = matrix.Diag[Step].NextInColumn;
                    while (pElement != null)
                    {
                        pElement.Value.Real = Dest[pElement.Row].Real;
                        pElement.Value.Imaginary = Dest[pElement.Row].Imaginary;
                        pElement = pElement.NextInColumn;
                    }

                    // Check for singular matrix
                    Pivot = Dest[Step];
                    if (Pivot.Magnitude.Equals(0.0))
                        return ZeroPivot(matrix, Step);
                    matrix.Diag[Step].Value.CopyReciprocal(Pivot);
                }
                else
                {
                    // Update column using direct addressing scatter-gather
                    MatrixElement[] pDest = new MatrixElement[pivoting.Intermediate.Length];

                    // Scatter
                    pElement = matrix.FirstInCol[Step];
                    while (pElement != null)
                    {
                        pDest[pElement.Row] = pElement;
                        pElement = pElement.NextInColumn;
                    }

                    // Update column
                    pColumn = matrix.FirstInCol[Step];
                    while (pColumn.Row < Step)
                    {
                        pElement = matrix.Diag[pColumn.Row];

                        // Cmplx expr: Mult = *pDest[pColumn.Row] * (1.0 / *pPivot)
                        Mult.CopyMultiply(pDest[pColumn.Row], pElement);
                        pDest[pColumn.Row].Value.CopyFrom(Mult);
                        while ((pElement = pElement.NextInColumn) != null)
                        {
                            // Cmplx expr: *pDest[pElement.Row] -= Mult * pElement
                            pDest[pElement.Row].Value.SubtractMultiply(Mult, pElement);
                        }
                        pColumn = pColumn.NextInColumn;
                    }

                    // Check for singular matrix
                    pElement = matrix.Diag[Step];
                    if (pElement.Value.Magnitude.Equals(0.0))
                        return ZeroPivot(matrix, Step);
                    pElement.Value.CopyReciprocal(pElement);
                }
            }

            matrix.Factored = true;
            return (matrix.Error = SparseError.Okay);
        }

        /// <summary>
        /// Exchange row and column
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="pPivot">Pivot</param>
        /// <param name="Step">Current step</param>
        private static void ExchangeRowsAndCols(Matrix matrix, MatrixElement pPivot, int Step)
        {
            var pivoting = matrix.Pivoting;
            int Row, Col;
            long OldMarkowitzProd_Step, OldMarkowitzProd_Row, OldMarkowitzProd_Col;

            Row = pPivot.Row;
            Col = pPivot.Column;
            pivoting.PivotsOriginalRow = Row;
            pivoting.PivotsOriginalCol = Col;

            if ((Row == Step) && (Col == Step)) return;

            // Exchange rows and columns. 
            if (Row == Col)
            {
                RowExchange(matrix, Step, Row);
                ColExchange(matrix, Step, Col);
                SparseDefinitions.Swap(ref pivoting.MarkowitzProd[Step], ref pivoting.MarkowitzProd[Row]);
                SparseDefinitions.Swap(ref matrix.Diag[Row], ref matrix.Diag[Step]);
            }
            else
            {

                // Initialize variables that hold old Markowitz products. 
                OldMarkowitzProd_Step = pivoting.MarkowitzProd[Step];
                OldMarkowitzProd_Row = pivoting.MarkowitzProd[Row];
                OldMarkowitzProd_Col = pivoting.MarkowitzProd[Col];

                // Exchange rows. 
                if (Row != Step)
                {
                    RowExchange(matrix, Step, Row);
                    matrix.NumberOfInterchangesIsOdd = !matrix.NumberOfInterchangesIsOdd;
                    pivoting.MarkowitzProd[Row] = pivoting.MarkowitzRow[Row] * pivoting.MarkowitzCol[Row];

                    // Update singleton count. 
                    if ((pivoting.MarkowitzProd[Row] == 0) != (OldMarkowitzProd_Row == 0))
                    {
                        if (OldMarkowitzProd_Row == 0)
                            pivoting.Singletons--;
                        else
                            pivoting.Singletons++;
                    }
                }

                // Exchange columns. 
                if (Col != Step)
                {
                    ColExchange(matrix, Step, Col);
                    matrix.NumberOfInterchangesIsOdd = !matrix.NumberOfInterchangesIsOdd;
                    pivoting.MarkowitzProd[Col] = pivoting.MarkowitzCol[Col] * pivoting.MarkowitzRow[Col];

                    // Update singleton count. 
                    if ((pivoting.MarkowitzProd[Col] == 0) != (OldMarkowitzProd_Col == 0))
                    {
                        if (OldMarkowitzProd_Col == 0)
                            pivoting.Singletons--;
                        else
                            pivoting.Singletons++;
                    }

                    matrix.Diag[Col] = matrix.FindReorderedElement(Col, Col);
                }
                if (Row != Step)
                {
                    matrix.Diag[Row] = matrix.FindReorderedElement(Row, Row);
                }
                matrix.Diag[Step] = matrix.FindReorderedElement(Step, Step);

                // Update singleton count. 
                pivoting.MarkowitzProd[Step] = pivoting.MarkowitzCol[Step] * pivoting.MarkowitzRow[Step];
                if ((pivoting.MarkowitzProd[Step] == 0) != (OldMarkowitzProd_Step == 0))
                {
                    if (OldMarkowitzProd_Step == 0)
                        pivoting.Singletons--;
                    else
                        pivoting.Singletons++;
                }
            }
            return;
        }

        /// <summary>
        /// Exchange rows
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="Row1">Row 1</param>
        /// <param name="Row2">Row 2</param>
        private static void RowExchange(this Matrix matrix, int Row1, int Row2)
        {
            MatrixElement Row1Ptr, Row2Ptr;
            int Column;
            MatrixElement Element1, Element2;

            if (Row1 > Row2)
                SparseDefinitions.Swap(ref Row1, ref Row2);

            Row1Ptr = matrix.FirstInRow[Row1];
            Row2Ptr = matrix.FirstInRow[Row2];
            while (Row1Ptr != null || Row2Ptr != null)
            {
                // Exchange elements in rows while traveling from left to right. 
                if (Row1Ptr == null)
                {
                    Column = Row2Ptr.Column;
                    Element1 = null;
                    Element2 = Row2Ptr;
                    Row2Ptr = Row2Ptr.NextInRow;
                }
                else if (Row2Ptr == null)
                {
                    Column = Row1Ptr.Column;
                    Element1 = Row1Ptr;
                    Element2 = null;
                    Row1Ptr = Row1Ptr.NextInRow;
                }
                else if (Row1Ptr.Column < Row2Ptr.Column)
                {
                    Column = Row1Ptr.Column;
                    Element1 = Row1Ptr;
                    Element2 = null;
                    Row1Ptr = Row1Ptr.NextInRow;
                }
                else if (Row1Ptr.Column > Row2Ptr.Column)
                {
                    Column = Row2Ptr.Column;
                    Element1 = null;
                    Element2 = Row2Ptr;
                    Row2Ptr = Row2Ptr.NextInRow;
                }
                else   // Row1Ptr.Col == Row2Ptr.Col 
                {
                    Column = Row1Ptr.Column;
                    Element1 = Row1Ptr;
                    Element2 = Row2Ptr;
                    Row1Ptr = Row1Ptr.NextInRow;
                    Row2Ptr = Row2Ptr.NextInRow;
                }

                ExchangeColElements(matrix, Row1, Element1, Row2, Element2, Column);
            }  // end of while(Row1Ptr != null ||  Row2Ptr != null) 

            if (matrix.Pivoting.InternalVectorsAllocated)
                SparseDefinitions.Swap(ref matrix.Pivoting.MarkowitzRow[Row1], ref matrix.Pivoting.MarkowitzRow[Row2]);
            SparseDefinitions.Swap(ref matrix.FirstInRow[Row1], ref matrix.FirstInRow[Row2]);
            SparseDefinitions.Swap(ref matrix.Translation.IntToExtRowMap[Row1], ref matrix.Translation.IntToExtRowMap[Row2]);
            matrix.Translation.ExtToIntRowMap[matrix.Translation.IntToExtRowMap[Row1]] = Row1;
            matrix.Translation.ExtToIntRowMap[matrix.Translation.IntToExtRowMap[Row2]] = Row2;
        }

        /// <summary>
        /// Exchange columns
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="Col1">Column 1</param>
        /// <param name="Col2">Column 2</param>
        private static void ColExchange(Matrix matrix, int Col1, int Col2)
        {
            MatrixElement Col1Ptr, Col2Ptr;
            int Row;
            MatrixElement Element1, Element2;

            // Begin `spcColExchange'
            if (Col1 > Col2)
                SparseDefinitions.Swap(ref Col1, ref Col2);

            Col1Ptr = matrix.FirstInCol[Col1];
            Col2Ptr = matrix.FirstInCol[Col2];
            while (Col1Ptr != null || Col2Ptr != null)
            {
                // Exchange elements in rows while traveling from top to bottom
                if (Col1Ptr == null)
                {
                    Row = Col2Ptr.Row;
                    Element1 = null;
                    Element2 = Col2Ptr;
                    Col2Ptr = Col2Ptr.NextInColumn;
                }
                else if (Col2Ptr == null)
                {
                    Row = Col1Ptr.Row;
                    Element1 = Col1Ptr;
                    Element2 = null;
                    Col1Ptr = Col1Ptr.NextInColumn;
                }
                else if (Col1Ptr.Row < Col2Ptr.Row)
                {
                    Row = Col1Ptr.Row;
                    Element1 = Col1Ptr;
                    Element2 = null;
                    Col1Ptr = Col1Ptr.NextInColumn;
                }
                else if (Col1Ptr.Row > Col2Ptr.Row)
                {
                    Row = Col2Ptr.Row;
                    Element1 = null;
                    Element2 = Col2Ptr;
                    Col2Ptr = Col2Ptr.NextInColumn;
                }
                else   // Col1Ptr.Row == Col2Ptr.Row
                {
                    Row = Col1Ptr.Row;
                    Element1 = Col1Ptr;
                    Element2 = Col2Ptr;
                    Col1Ptr = Col1Ptr.NextInColumn;
                    Col2Ptr = Col2Ptr.NextInColumn;
                }

                ExchangeRowElements(matrix, Col1, Element1, Col2, Element2, Row);
            }  // end of while(Col1Ptr != null || Col2Ptr != null)

            if (matrix.Pivoting.InternalVectorsAllocated)
                SparseDefinitions.Swap(ref matrix.Pivoting.MarkowitzCol[Col1], ref matrix.Pivoting.MarkowitzCol[Col2]);

            SparseDefinitions.Swap(ref matrix.FirstInCol[Col1], ref matrix.FirstInCol[Col2]);
            SparseDefinitions.Swap(ref matrix.Translation.IntToExtColMap[Col1], ref matrix.Translation.IntToExtColMap[Col2]);
            matrix.Translation.ExtToIntColMap[matrix.Translation.IntToExtColMap[Col1]] = Col1;
            matrix.Translation.ExtToIntColMap[matrix.Translation.IntToExtColMap[Col2]] = Col2;

            return;
        }

        /// <summary>
        /// Exchange column elements
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="Row1">Row of the first element</param>
        /// <param name="Element1">First element</param>
        /// <param name="Row2">Row of the second element</param>
        /// <param name="Element2">Second element</param>
        /// <param name="Column">Column</param>
        private static void ExchangeColElements(Matrix matrix, int Row1, MatrixElement Element1, int Row2, MatrixElement Element2, int Column)
        {
            MatrixElement ElementAboveRow1, ElementAboveRow2;
            MatrixElement ElementBelowRow1, ElementBelowRow2;
            MatrixElement pElement;

            // Begin `ExchangeColElements'. 
            // Search to find the ElementAboveRow1. 
            ElementAboveRow1 = null; // &(matrix.FirstInCol[Column]);
            pElement = matrix.FirstInCol[Column]; // *ElementAboveRow1;
            while (pElement.Row < Row1)
            {
                ElementAboveRow1 = pElement; // &(pElement.NextInCol);
                pElement = pElement.NextInColumn; // *ElementAboveRow1;
            }
            if (Element1 != null)
            {
                ElementBelowRow1 = Element1.NextInColumn;
                if (Element2 == null)
                {
                    // Element2 does not exist, move Element1 down to Row2. 
                    if (ElementBelowRow1 != null && ElementBelowRow1.Row < Row2)
                    {
                        // Element1 must be removed from linked list and moved.
                        if (ElementAboveRow1 != null)
                            ElementAboveRow1.NextInColumn = ElementBelowRow1;
                        else
                            matrix.FirstInCol[Column] = ElementBelowRow1;
                        // *ElementAboveRow1 = ElementBelowRow1;

                        // Search column for Row2. 
                        pElement = ElementBelowRow1;
                        do
                        {
                            ElementAboveRow2 = pElement; // &(pElement.NextInCol);
                            pElement = pElement.NextInColumn; // *ElementAboveRow2;
                        } while (pElement != null && pElement.Row < Row2);

                        // Place Element1 in Row2
                        if (ElementAboveRow2 != null)
                            ElementAboveRow2.NextInColumn = Element1;
                        else
                            matrix.FirstInCol[Column] = Element1;
                        // *ElementAboveRow2 = Element1;
                        Element1.NextInColumn = pElement;
                        if (ElementAboveRow1 != null)
                            ElementAboveRow1.NextInColumn = ElementBelowRow1;
                        else
                            matrix.FirstInCol[Column] = ElementBelowRow1;
                        // *ElementAboveRow1 =ElementBelowRow1;
                    }
                    Element1.Row = Row2;
                }
                else
                {
                    // Element2 does exist, and the two elements must be exchanged. 
                    if (ElementBelowRow1.Row == Row2)
                    {
                        // Element2 is just below Element1, exchange them. 
                        Element1.NextInColumn = Element2.NextInColumn;
                        Element2.NextInColumn = Element1;
                        if (ElementAboveRow1 != null)
                            ElementAboveRow1.NextInColumn = Element2;
                        else
                            matrix.FirstInCol[Column] = Element2;
                        // *ElementAboveRow1 = Element2;
                    }
                    else
                    {
                        // Element2 is not just below Element1 and must be searched for. 
                        pElement = ElementBelowRow1;
                        do
                        {
                            ElementAboveRow2 = pElement; // &(pElement.NextInCol);
                            pElement = pElement.NextInColumn; // *ElementAboveRow2;
                        } while (pElement.Row < Row2);

                        ElementBelowRow2 = Element2.NextInColumn;

                        // Switch Element1 and Element2
                        if (ElementAboveRow1 != null)
                            ElementAboveRow1.NextInColumn = Element2;
                        else
                            matrix.FirstInCol[Column] = Element2;
                        // *ElementAboveRow1 = Element2;
                        Element2.NextInColumn = ElementBelowRow1;
                        if (ElementAboveRow2 != null)
                            ElementAboveRow2.NextInColumn = Element1;
                        else
                            matrix.FirstInCol[Column] = Element1;
                        // *ElementAboveRow2 = Element1;
                        Element1.NextInColumn = ElementBelowRow2;
                    }
                    Element1.Row = Row2;
                    Element2.Row = Row1;
                }
            }
            else
            {
                // Element1 does not exist. 
                ElementBelowRow1 = pElement;

                // Find Element2. 
                if (ElementBelowRow1.Row != Row2)
                {
                    do
                    {
                        ElementAboveRow2 = pElement; // &(pElement.NextInCol);
                        pElement = pElement.NextInColumn; // *ElementAboveRow2;
                    } while (pElement.Row < Row2);

                    ElementBelowRow2 = Element2.NextInColumn;

                    // Move Element2 to Row1
                    if (ElementAboveRow2 != null)
                        ElementAboveRow2.NextInColumn = Element2.NextInColumn;
                    else
                        matrix.FirstInCol[Column] = Element2.NextInColumn;
                    // *ElementAboveRow2 = Element2.NextInCol;
                    if (ElementAboveRow1 != null)
                        ElementAboveRow1.NextInColumn = Element2;
                    else
                        matrix.FirstInCol[Column] = Element2;
                    // *ElementAboveRow1 = Element2;
                    Element2.NextInColumn = ElementBelowRow1;
                }
                Element2.Row = Row1;
            }
            return;
        }

        /// <summary>
        /// Exchange row elements
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="Col1">Column of the first element</param>
        /// <param name="Element1">First element</param>
        /// <param name="Col2">Column of the second element</param>
        /// <param name="Element2">Second element</param>
        /// <param name="Row">Row</param>
        private static void ExchangeRowElements(Matrix matrix, int Col1, MatrixElement Element1, int Col2, MatrixElement Element2, int Row)
        {
            MatrixElement ElementLeftOfCol1, ElementLeftOfCol2;
            MatrixElement ElementRightOfCol1, ElementRightOfCol2;
            MatrixElement pElement;

            // Begin `ExchangeRowElements'. 

            // Search to find the element left of Col1
            ElementLeftOfCol1 = null;
            pElement = matrix.FirstInRow[Row];
            while (pElement.Column < Col1)
            {
                ElementLeftOfCol1 = pElement;
                pElement = pElement.NextInRow;
            }

            // Swap the elements depending on whether or not the element exist
            if (Element1 != null)
            {
                ElementRightOfCol1 = Element1.NextInRow;
                if (Element2 == null)
                {
                    // Element2 does not exist, remove Element1 and splice it in at Col2
                    if (ElementRightOfCol1 != null && ElementRightOfCol1.Column < Col2)
                    {
                        // Remove Element1
                        if (ElementLeftOfCol1 != null)
                            ElementLeftOfCol1.NextInRow = ElementRightOfCol1;
                        else
                            matrix.FirstInRow[Row] = ElementRightOfCol1;

                        // Find point to insert
                        pElement = ElementRightOfCol1;
                        do
                        {
                            ElementLeftOfCol2 = pElement;
                            pElement = pElement.NextInRow;
                        } while (pElement != null && pElement.Column < Col2);

                        // Place Element1 in Col2. 
                        ElementLeftOfCol2.NextInRow = Element1;
                        Element1.NextInRow = pElement;
                    }
                    Element1.Column = Col2;
                }
                else
                {
                    // Element2 does exist, and the two elements must be exchanged. 
                    if (ElementRightOfCol1.Column == Col2)
                    {
                        // Element1 and Element2 are next to each other
                        Element1.NextInRow = Element2.NextInRow;
                        Element2.NextInRow = Element1;

                        if (ElementLeftOfCol1 != null)
                            ElementLeftOfCol1.NextInRow = Element2;
                        else
                            matrix.FirstInRow[Row] = Element2;
                    }
                    else
                    {
                        // Element1 and Element2 have elements in between them
                        pElement = ElementRightOfCol1;
                        do
                        {
                            ElementLeftOfCol2 = pElement;
                            pElement = pElement.NextInRow;
                        } while (pElement.Column < Col2);
                        ElementRightOfCol2 = Element2.NextInRow;

                        // Switch Element1 and Element2
                        if (ElementLeftOfCol1 != null)
                            ElementLeftOfCol1.NextInRow = Element2;
                        else
                            matrix.FirstInRow[Row] = Element2;
                        Element2.NextInRow = ElementRightOfCol1;

                        ElementLeftOfCol2.NextInRow = Element1; // Cannot be null
                        Element1.NextInRow = ElementRightOfCol2;
                    }
                    Element1.Column = Col2;
                    Element2.Column = Col1;
                }
            }
            else
            {
                // Remove Element2 and insert it at Col1
                ElementRightOfCol1 = pElement;
                if (ElementRightOfCol1.Column != Col2)
                {
                    // Find the elements next to Element2 to remove it
                    do
                    {
                        ElementLeftOfCol2 = pElement;
                        pElement = pElement.NextInRow;
                    } while (pElement.Column < Col2);
                    ElementRightOfCol2 = Element2.NextInRow;

                    // Move Element2 to Col1. 
                    if (ElementLeftOfCol2 != null)
                        ElementLeftOfCol2.NextInRow = Element2.NextInRow;
                    else
                        matrix.FirstInRow[Row] = Element2.NextInRow;
                    // *ElementLeftOfCol2 = Element2.NextInRow;

                    if (ElementLeftOfCol1 != null)
                        ElementLeftOfCol1.NextInRow = Element2;
                    else
                        matrix.FirstInRow[Row] = Element2;
                    // *ElementLeftOfCol1 = Element2;
                    Element2.NextInRow = ElementRightOfCol1;
                }
                Element2.Column = Col1;
            }
            return;
        }

        /// <summary>
        /// Eliminate a row with real numbers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="pPivot">Current pivot</param>
        private static void RealRowColElimination(Matrix matrix, MatrixElement pPivot)
        {
            MatrixElement pSub;
            int Row;
            MatrixElement pLower, pUpper;

            // Test for zero pivot. 
            if (Math.Abs(pPivot.Value.Real) == 0.0)
            {
                MatrixIsSingular(matrix, pPivot.Row);
                return;
            }
            pPivot.Value.Real = 1.0 / pPivot.Value.Real;

            pUpper = pPivot.NextInRow;
            while (pUpper != null)
            {
                // Calculate upper triangular element. 
                pUpper.Value.Real *= pPivot.Value.Real;

                pSub = pUpper.NextInColumn;
                pLower = pPivot.NextInColumn;
                while (pLower != null)
                {
                    Row = pLower.Row;

                    // Find element in row that lines up with current lower triangular element. 
                    while (pSub != null && pSub.Row < Row)
                        pSub = pSub.NextInColumn;

                    // Test to see if desired element was not found, if not, create fill-in. 
                    if (pSub == null || pSub.Row > Row)
                    {
                        pSub = matrix.CreateFillin(Row, pUpper.Column);
                    }
                    pSub.Value.Real -= pUpper.Value.Real * pLower.Value.Real;
                    pSub = pSub.NextInColumn;
                    pLower = pLower.NextInColumn;
                }
                pUpper = pUpper.NextInRow;
            }
        }

        /// <summary>
        /// Eliminate a row with complex numbers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="pPivot">Current pivot</param>
        private static void ComplexRowColElimination(Matrix matrix, MatrixElement pPivot)
        {
            MatrixElement pSub;
            int Row;
            MatrixElement pLower, pUpper;

            // Test for zero pivot. 
            if (pPivot.Value.Magnitude.Equals(0.0))
            {
                MatrixIsSingular(matrix, pPivot.Row);
                return;
            }
            pPivot.Value.CopyReciprocal(pPivot);

            pUpper = pPivot.NextInRow;
            while (pUpper != null)
            {
                // Calculate upper triangular element. 
                // Cmplx expr: *pUpper = *pUpper * (1.0 / *pPivot)
                pUpper.Value.Multiply(pPivot);

                pSub = pUpper.NextInColumn;
                pLower = pPivot.NextInColumn;
                while (pLower != null)
                {
                    Row = pLower.Row;

                    // Find element in row that lines up with current lower triangular element. 
                    while (pSub != null && pSub.Row < Row)
                        pSub = pSub.NextInColumn;

                    // Test to see if desired element was not found, if not, create fill-in. 
                    if (pSub == null || pSub.Row > Row)
                    {
                        pSub = matrix.CreateFillin(Row, pUpper.Column);
                    }

                    // Cmplx expr: pElement -= *pUpper * pLower
                    pSub.Value.SubtractMultiply(pUpper, pLower);
                    pSub = pSub.NextInColumn;
                    pLower = pLower.NextInColumn;
                }
                pUpper = pUpper.NextInRow;
            }
        }

        /// <summary>
        /// We have a singular matrix
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="Step">Current step</param>
        /// <returns></returns>
        private static SparseError MatrixIsSingular(Matrix matrix, int Step)
        {
            matrix.SingularRow = matrix.Translation.IntToExtRowMap[Step];
            matrix.SingularCol = matrix.Translation.IntToExtColMap[Step];
            return (matrix.Error = SparseError.Singular);
        }

        /// <summary>
        /// We have a zero on the diagonal
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="Step">Current step</param>
        /// <returns></returns>
        private static SparseError ZeroPivot(Matrix matrix, int Step)
        {
            matrix.SingularRow = matrix.Translation.IntToExtRowMap[Step];
            matrix.SingularCol = matrix.Translation.IntToExtColMap[Step];
            return (matrix.Error = SparseError.ZeroDiagonal);
        }
    }
}
