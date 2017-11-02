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
        /// <param name="RHS">Right-Hand Side</param>
        /// <param name="RelThreshold">Relative threshold for pivot selection</param>
        /// <param name="AbsThreshold">Absolute threshold for pivot selection</param>
        /// <param name="DiagPivoting">Use diagonal pivoting</param>
        /// <returns></returns>
        public static SparseError OrderAndFactor(this Matrix matrix, double[] RHS, double RelThreshold, double AbsThreshold, bool DiagPivoting)
        {
            MatrixElement pPivot;
            int Step, Size;
            bool ReorderingRequired;
            double LargestInCol;

            if (matrix.Factored)
                throw new SparseException("Matrix is already factored");

            matrix.Error = SparseError.Okay;
            Size = matrix.Size;
            if (RelThreshold <= 0.0)
                RelThreshold = matrix.RelThreshold;
            if (RelThreshold > 1.0)
                RelThreshold = matrix.RelThreshold;
            matrix.RelThreshold = RelThreshold;
            if (AbsThreshold < 0.0)
                AbsThreshold = matrix.AbsThreshold;
            matrix.AbsThreshold = AbsThreshold;
            ReorderingRequired = false;

            if (!matrix.NeedsOrdering)
            {
                // Matrix has been factored before and reordering is not required. 
                for (Step = 1; Step <= Size; Step++)
                {
                    pPivot = matrix.Diag[Step];
                    LargestInCol = FindLargestInCol(pPivot.NextInCol);
                    if (LargestInCol * RelThreshold < SparseDefinitions.ELEMENT_MAG(pPivot))
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
                    SparseBuild.spcLinkRows(matrix);
                if (!matrix.InternalVectorsAllocated)
                    spcCreateInternalVectors(matrix);
                if ((int)matrix.Error >= (int)SparseError.Fatal)
                    return matrix.Error;
            }

            // Form initial Markowitz products. 
            CountMarkowitz(matrix, RHS, Step);
            MarkowitzProducts(matrix, Step);
            matrix.MaxRowCountInLowerTri = -1;

            // Perform reordering and factorization. 
            for (; Step <= Size; Step++)
            {
                pPivot = SearchForPivot(matrix, Step, DiagPivoting);
                if (pPivot == null)
                    return MatrixIsSingular(matrix, Step);
                ExchangeRowsAndCols(matrix, pPivot, Step);

                if (matrix.Complex)
                    ComplexRowColElimination(matrix, pPivot);
                else
                    RealRowColElimination(matrix, pPivot);

                if ((int)matrix.Error >= (int)SparseError.Fatal)
                    return matrix.Error;
                UpdateMarkowitzNumbers(matrix, pPivot);
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
        internal static SparseError Factor(this Matrix matrix)
        {
            if (matrix.Factored)
                throw new SparseException("Matrix is factored");
            MatrixElement pElement, pColumn;

            if (matrix.NeedsOrdering)
                return OrderAndFactor(matrix, null, 0.0, 0.0, Matrix.DIAG_PIVOTING_AS_DEFAULT);
            if (!matrix.Partitioned)
                Partition(matrix, SparsePartition.Default);
            if (matrix.Complex)
                return FactorComplexMatrix(matrix);

            int Size = matrix.Size;

            if (matrix.Diag[1].Value.Real == 0.0)
                return ZeroPivot(matrix, 1);
            matrix.Diag[1].Value.Real = 1.0 / matrix.Diag[1].Value.Real;

            // Start factorization
            for (int Step = 2; Step <= Size; Step++)
            {
                if (matrix.DoRealDirect[Step])
                {
                    // Update column using direct addressing scatter-gather
                    ElementValue[] Dest = matrix.Intermediate;

                    // Scatter
                    pElement = matrix.FirstInCol[Step];
                    while (pElement != null)
                    {
                        Dest[pElement.Row] = pElement;
                        pElement = pElement.NextInCol;
                    }

                    // Update column
                    pColumn = matrix.FirstInCol[Step];
                    while (pColumn.Row < Step)
                    {
                        pElement = matrix.Diag[pColumn.Row];
                        pColumn.Value.Real = Dest[pColumn.Row] * pElement.Value.Real;
                        while ((pElement = pElement.NextInCol) != null)
                            Dest[pElement.Row].Real -= pColumn.Value.Real * pElement.Value.Real;
                        pColumn = pColumn.NextInCol;
                    }

                    // Gather
                    pElement = matrix.Diag[Step].NextInCol;
                    while (pElement != null)
                    {
                        pElement.Value.Real = Dest[pElement.Row];
                        pElement = pElement.NextInCol;
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
                        pElement = pElement.NextInCol;
                    }

                    // Update column
                    pColumn = matrix.FirstInCol[Step];
                    while (pColumn.Row < Step)
                    {
                        pElement = matrix.Diag[pColumn.Row];
                        double Mult = (pDest[pColumn.Row].Value.Real *= pElement.Value.Real);
                        while ((pElement = pElement.NextInCol) != null)
                            pDest[pElement.Row].Value.Real -= Mult * pElement.Value.Real;
                        pColumn = pColumn.NextInCol;
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
        internal static SparseError FactorComplexMatrix(Matrix matrix)
        {
            MatrixElement pElement, pColumn;
            int Step, Size;
            ElementValue Mult = new ElementValue();
            ElementValue Pivot;

            if (!matrix.Complex)
                throw new SparseException("Matrix is not complex");

            Size = matrix.Size;
            pElement = matrix.Diag[1];
            if (SparseDefinitions.ELEMENT_MAG(pElement) == 0.0)
                return ZeroPivot(matrix, 1);

            // Cmplx expr: *pPivot = 1.0 / *pPivot
            SparseDefinitions.CMPLX_RECIPROCAL(ref pElement.Value, pElement);

            // Start factorization
            for (Step = 2; Step <= Size; Step++)
            {
                if (matrix.DoCmplxDirect[Step])
                {
                    // Update column using direct addressing scatter-gather
                    ElementValue[] Dest = matrix.Intermediate;

                    // Scatter
                    pElement = matrix.FirstInCol[Step];
                    while (pElement != null)
                    {
                        Dest[pElement.Row] = pElement;
                        pElement = pElement.NextInCol;
                    }

                    // Update column
                    pColumn = matrix.FirstInCol[Step];
                    while (pColumn.Row < Step)
                    {
                        pElement = matrix.Diag[pColumn.Row];

                        // Cmplx expr: Mult = Dest[pColumn.Row] * (1.0 / *pPivot)
                        SparseDefinitions.CMPLX_MULT(ref Mult, Dest[pColumn.Row], pElement);
                        SparseDefinitions.CMPLX_ASSIGN(ref pColumn.Value, Mult);
                        while ((pElement = pElement.NextInCol) != null)
                        {
                            // Cmplx expr: Dest[pElement.Row] -= Mult * pElement
                            SparseDefinitions.CMPLX_MULT_SUBT_ASSIGN(ref Dest[pElement.Row], Mult, pElement);
                        }
                        pColumn = pColumn.NextInCol;
                    }

                    // Gather
                    pElement = matrix.Diag[Step].NextInCol;
                    while (pElement != null)
                    {
                        pElement.Value.Real = Dest[pElement.Row].Real;
                        pElement.Value.Imag = Dest[pElement.Row].Imag;
                        pElement = pElement.NextInCol;
                    }

                    // Check for singular matrix
                    Pivot = Dest[Step];
                    if (SparseDefinitions.CMPLX_1_NORM(Pivot) == 0.0)
                        return ZeroPivot(matrix, Step);
                    SparseDefinitions.CMPLX_RECIPROCAL(ref matrix.Diag[Step].Value, Pivot);
                }
                else
                {
                    // Update column using direct addressing scatter-gather
                    ElementValue[] pDest = matrix.Intermediate;

                    // Scatter
                    pElement = matrix.FirstInCol[Step];
                    while (pElement != null)
                    {
                        pDest[pElement.Row] = pElement;
                        pElement = pElement.NextInCol;
                    }

                    // Update column
                    pColumn = matrix.FirstInCol[Step];
                    while (pColumn.Row < Step)
                    {
                        pElement = matrix.Diag[pColumn.Row];

                        // Cmplx expr: Mult = *pDest[pColumn.Row] * (1.0 / *pPivot)
                        SparseDefinitions.CMPLX_MULT(ref Mult, pDest[pColumn.Row], pElement);
                        SparseDefinitions.CMPLX_ASSIGN(ref pDest[pColumn.Row], Mult);
                        while ((pElement = pElement.NextInCol) != null)
                        {
                            // Cmplx expr: *pDest[pElement.Row] -= Mult * pElement
                            SparseDefinitions.CMPLX_MULT_SUBT_ASSIGN(ref pDest[pElement.Row], Mult, pElement);
                        }
                        pColumn = pColumn.NextInCol;
                    }

                    // Check for singular matrix
                    pElement = matrix.Diag[Step];
                    if (SparseDefinitions.ELEMENT_MAG(pElement) == 0.0)
                        return ZeroPivot(matrix, Step);
                    SparseDefinitions.CMPLX_RECIPROCAL(ref pElement.Value, pElement);
                }
            }

            matrix.Factored = true;
            return (matrix.Error = SparseError.Okay);
        }

        internal static void Partition(Matrix matrix, SparsePartition Mode)
        {
            MatrixElement pElement, pColumn;
            int Step, Size;
            int[] Nc, No;
            long[] Nm;
            bool[] DoRealDirect, DoCmplxDirect;

            if (matrix.Partitioned)
                return;
            Size = matrix.Size;
            DoRealDirect = matrix.DoRealDirect;
            DoCmplxDirect = matrix.DoCmplxDirect;
            matrix.Partitioned = true;

            // If partition is specified by the user, this is easy
            if (Mode == SparsePartition.Default)
                Mode = Matrix.DEFAULT_PARTITION;
            if (Mode == SparsePartition.Direct)
            {
                for (Step = 1; Step <= Size; Step++)
                {
                    DoRealDirect[Step] = true;
                    DoCmplxDirect[Step] = true;
                }
                return;
            }
            else if (Mode == SparsePartition.Indirect)
            {
                for (Step = 1; Step <= Size; Step++)
                {
                    DoRealDirect[Step] = false;
                    DoCmplxDirect[Step] = false;
                }
                return;
            }
            else if (Mode != SparsePartition.Auto)
                throw new SparseException("Invalid partition mode");

            // Otherwise, count all operations needed in when factoring matrix
            Nc = matrix.MarkowitzRow;
            No = matrix.MarkowitzCol;
            Nm = matrix.MarkowitzProd;

            // Start mock-factorization. 
            for (Step = 1; Step <= Size; Step++)
            {
                Nc[Step] = No[Step] = 0;
                Nm[Step] = 0;

                pElement = matrix.FirstInCol[Step];
                while (pElement != null)
                {
                    Nc[Step]++;
                    pElement = pElement.NextInCol;
                }

                pColumn = matrix.FirstInCol[Step];
                while (pColumn.Row < Step)
                {
                    pElement = matrix.Diag[pColumn.Row];
                    Nm[Step]++;
                    while ((pElement = pElement.NextInCol) != null)
                        No[Step]++;
                    pColumn = pColumn.NextInCol;
                }
            }

            for (Step = 1; Step <= Size; Step++)
            {
                // The following are just estimates based on a count on the number of
                // machine instructions used on each machine to perform the various
                // tasks.  It was assumed that each machine instruction required the
                // same amount of time (I don't believe this is true for the VAX, and
                // have no idea if this is true for the 68000 family).  For optimum
                // performance, these numbers should be tuned to the machine.
                //   Nc is the number of nonzero elements in the column.
                //   Nm is the number of multipliers in the column.
                //   No is the number of operations in the inner loop.

                DoRealDirect[Step] = (Nm[Step] + No[Step] > 3 * Nc[Step] - 2 * Nm[Step]);
                DoCmplxDirect[Step] = (Nm[Step] + No[Step] > 7 * Nc[Step] - 4 * Nm[Step]);
            }
        }

        public static void spcCreateInternalVectors(Matrix matrix)
        {
            int Size;

            // Create Markowitz arrays
            Size = matrix.Size;

            if (matrix.MarkowitzRow == null)
                matrix.MarkowitzRow = new int[Size + 1];
            if (matrix.MarkowitzCol == null)
                matrix.MarkowitzCol = new int[Size + 1];
            if (matrix.MarkowitzProd == null)
                matrix.MarkowitzProd = new long[Size + 2];

            // Create DoDirect vectors for use in spFactor()
            if (matrix.DoRealDirect == null)
                matrix.DoRealDirect = new bool[Size + 1];
            if (matrix.DoCmplxDirect == null)
                matrix.DoCmplxDirect = new bool[Size + 1];

            // Create Intermediate vectors for use in MatrixSolve
            if (matrix.Intermediate == null)
            {
                matrix.Intermediate = new ElementValue[Size + 1];
            }

            matrix.InternalVectorsAllocated = true;
        }

        public static void CountMarkowitz(Matrix matrix, double[] RHS, int Step)
        {
            int Count, I, Size = matrix.Size;
            MatrixElement pElement;
            int ExtRow;

            // Generate MarkowitzRow Count for each row
            for (I = Step; I <= Size; I++)
            {
                // Set Count to -1 initially to remove count due to pivot element
                Count = -1;
                pElement = matrix.FirstInRow[I];
                while (pElement != null && pElement.Col < Step)
                    pElement = pElement.NextInRow;
                while (pElement != null)
                {
                    Count++;
                    pElement = pElement.NextInRow;
                }

                // Include nonzero elements in the RHS vector
                ExtRow = matrix.IntToExtRowMap[I];

                if (RHS != null)
                    if (RHS[ExtRow] != 0.0)
                        Count++;
                matrix.MarkowitzRow[I] = Count;
            }

            // Generate the MarkowitzCol count for each column
            for (I = Step; I <= Size; I++)
            {
                // Set Count to -1 initially to remove count due to pivot element
                Count = -1;
                pElement = matrix.FirstInCol[I];
                while (pElement != null && pElement.Row < Step)
                    pElement = pElement.NextInCol;
                while (pElement != null)
                {
                    Count++;
                    pElement = pElement.NextInCol;
                }
                matrix.MarkowitzCol[I] = Count;
            }
        }

        public static void MarkowitzProducts(Matrix matrix, int Step)
        {
            int I;
            int[] pMarkowitzRow, pMarkowitzCol;
            long Product;
            long[] pMarkowitzProduct;
            int Size = matrix.Size;
            double fProduct;

            matrix.Singletons = 0;

            pMarkowitzProduct = matrix.MarkowitzProd;
            pMarkowitzRow = matrix.MarkowitzRow;
            pMarkowitzCol = matrix.MarkowitzCol;

            for (I = Step; I <= Size; I++)
            {
                // If chance of overflow, use real numbers. 
                if ((pMarkowitzRow[I] > short.MaxValue && pMarkowitzCol[I] != 0) ||
                     (pMarkowitzCol[I] > short.MaxValue && pMarkowitzRow[I] != 0))
                {
                    fProduct = (double)pMarkowitzRow[I] * pMarkowitzCol[I];
                    if (fProduct >= long.MaxValue)
                        pMarkowitzProduct[I] = long.MaxValue;
                    else
                        pMarkowitzProduct[I] = (long)fProduct;
                }
                else
                {
                    Product = pMarkowitzRow[I] * pMarkowitzCol[I];
                    if ((pMarkowitzProduct[I] = Product) == 0)
                        matrix.Singletons++;
                }
            }
        }

        public static MatrixElement SearchForPivot(Matrix matrix, int Step, bool DiagPivoting)
        {
            MatrixElement ChosenPivot;

            // If singletons exist, look for an acceptable one to use as pivot. 
            if (matrix.Singletons != 0)
            {
                ChosenPivot = SearchForSingleton(matrix, Step);
                if (ChosenPivot != null)
                {
                    matrix.PivotSelectionMethod = 's';
                    return ChosenPivot;
                }
            }

            if (DiagPivoting)
            {

                // Either no singletons exist or they weren't acceptable.  Take quick first
                // pass at searching diagonal.  First search for element on diagonal of 
                // remaining submatrix with smallest Markowitz product, then check to see
                // if it okay numerically.  If not, QuicklySearchDiagonal fails.

                ChosenPivot = QuicklySearchDiagonal(matrix, Step);
                if (ChosenPivot != null)
                {
                    matrix.PivotSelectionMethod = 'q';
                    return ChosenPivot;
                }

                // Quick search of diagonal failed, carefully search diagonal and check each
                // pivot candidate numerically before even tentatively accepting it.

                ChosenPivot = SearchDiagonal(matrix, Step);
                if (ChosenPivot != null)
                {
                    matrix.PivotSelectionMethod = 'd';
                    return ChosenPivot;
                }
            }

            // No acceptable pivot found yet, search entire matrix. 
            ChosenPivot = SearchEntireMatrix(matrix, Step);
            matrix.PivotSelectionMethod = 'e';

            return ChosenPivot;
        }

        public static MatrixElement SearchForSingleton(Matrix matrix, int Step)
        {
            MatrixElement ChosenPivot;
            int I;
            long[] pMarkowitzProduct;
            int Singletons;
            double PivotMag;

            // Initialize pointer that is to scan through MarkowitzProduct vector. 
            int index = matrix.Size + 1;
            pMarkowitzProduct = matrix.MarkowitzProd;
            matrix.MarkowitzProd[matrix.Size + 1] = matrix.MarkowitzProd[Step];

            // Decrement the count of available singletons, on the assumption that an
            // acceptable one will be found
            Singletons = matrix.Singletons--;

            // Assure that following while loop will always terminate, this is just
            // preventive medicine, if things are working right this should never
            // be needed.
            matrix.MarkowitzProd[Step - 1] = 0;

            while (Singletons-- > 0)
            {
                // Singletons exist, find them. 

                // This is tricky.  Am using a pointer to sequentially step through the
                // MarkowitzProduct array.  Search terminates when singleton (Product = 0)
                // is found.  Note that the conditional in the while statement
                // ( *pMarkowitzProduct ) is true as long as the MarkowitzProduct is not
                // equal to zero.  The row (and column) index on the diagonal is then
                // calculated by subtracting the pointer to the Markowitz product of
                // the first diagonal from the pointer to the Markowitz product of the
                // desired element, the singleton.
                //
                // Search proceeds from the end (high row and column numbers) to the
                // beginning (low row and column numbers) so that rows and columns with
                // large Markowitz products will tend to be move to the bottom of the
                // matrix.  However, choosing Diag[Step] is desirable because it would
                // require no row and column interchanges, so inspect it first by
                // putting its Markowitz product at the end of the MarkowitzProd
                // vector.

                while (pMarkowitzProduct[index--] != 0)
                {
                    // N bottles of beer on the wall;
                    // N bottles of beer.
                    // you take one down and pass it around;
                    // N-1 bottles of beer on the wall.
                }
                I = (int)(pMarkowitzProduct[index] - matrix.MarkowitzProd[0] + 1);

                // Assure that I is valid. 
                if (I < Step) break;  // while (Singletons-- > 0) 
                if (I > matrix.Size) I = Step;

                // Singleton has been found in either/both row or/and column I. 
                if ((ChosenPivot = matrix.Diag[I]) != null)
                {
                    // Singleton lies on the diagonal. 
                    PivotMag = SparseDefinitions.ELEMENT_MAG(ChosenPivot);
                    if (PivotMag > matrix.AbsThreshold && PivotMag > matrix.RelThreshold * FindBiggestInColExclude(matrix, ChosenPivot, Step))
                        return ChosenPivot;
                }
                else
                {
                    // Singleton does not lie on diagonal, find it. 
                    if (matrix.MarkowitzCol[I] == 0)
                    {
ChosenPivot = matrix.FirstInCol[I];
                        while ((ChosenPivot != null) && (ChosenPivot.Row < Step))
                            ChosenPivot = ChosenPivot.NextInCol;
                        if (ChosenPivot != null)
                        {
                            // Reduced column has no elements, matrix is singular. 
                            break;
                        }
                        PivotMag = SparseDefinitions.ELEMENT_MAG(ChosenPivot);
                        if (PivotMag > matrix.AbsThreshold && PivotMag > matrix.RelThreshold * FindBiggestInColExclude(matrix, ChosenPivot, Step))
                            return ChosenPivot;
                        else
                        {
if (matrix.MarkowitzRow[I] == 0)
                            {
ChosenPivot = matrix.FirstInRow[I];
                                while ((ChosenPivot != null) && (ChosenPivot.Col < Step))
                                    ChosenPivot = ChosenPivot.NextInRow;
                                if (ChosenPivot != null)
                                {
                                    // Reduced row has no elements, matrix is singular. 
                                    break;
                                }
                                PivotMag = SparseDefinitions.ELEMENT_MAG(ChosenPivot);
                                if (PivotMag > matrix.AbsThreshold && PivotMag > matrix.RelThreshold * FindBiggestInColExclude(matrix, ChosenPivot, Step))
                                    return ChosenPivot;
                            }
                        }
                    }
                    else
                    {
ChosenPivot = matrix.FirstInRow[I];
                        while ((ChosenPivot != null) && (ChosenPivot.Col < Step))
                            ChosenPivot = ChosenPivot.NextInRow;
                        if (ChosenPivot != null)
                        {   // Reduced row has no elements, matrix is singular. 
                            break;
                        }
                        PivotMag = SparseDefinitions.ELEMENT_MAG(ChosenPivot);
                        if (PivotMag > matrix.AbsThreshold && PivotMag > matrix.RelThreshold * FindBiggestInColExclude(matrix, ChosenPivot, Step))
                            return ChosenPivot;
                    }
                }
                // Singleton not acceptable (too small), try another. 
            } // end of while(lSingletons>0) 

            // All singletons were unacceptable.  Restore matrix.Singletons count.
            // Initial assumption that an acceptable singleton would be found was wrong.
            matrix.Singletons++;
            return null;
        }

        public static MatrixElement QuicklySearchDiagonal(Matrix matrix, int Step)
        {
            long MinMarkowitzProduct;
            // long pMarkowitzProduct;
            MatrixElement pDiag;
            int I;
            MatrixElement ChosenPivot, pOtherInRow, pOtherInCol;
            double Magnitude, LargestInCol, LargestOffDiagonal;

            ChosenPivot = null;
            MinMarkowitzProduct = long.MaxValue;
            int index = matrix.Singletons + 2;
            // pMarkowitzProduct = matrix.MarkowitzProd[index];
            matrix.MarkowitzProd[matrix.Size + 1] = matrix.MarkowitzProd[Step];

            // Assure that following while loop will always terminate. 
            matrix.MarkowitzProd[Step - 1] = -1;

            // This is tricky.  Am using a pointer in the inner while loop to
            // sequentially step through the MarkowitzProduct array.  Search
            // terminates when the Markowitz product of zero placed at location
            // Step-1 is found.  The row (and column) index on the diagonal is then
            // calculated by subtracting the pointer to the Markowitz product of
            // the first diagonal from the pointer to the Markowitz product of the
            // desired element. The outer for loop is infinite, broken by using
            // break.
            //
            // Search proceeds from the end (high row and column numbers) to the
            // beginning (low row and column numbers) so that rows and columns with
            // large Markowitz products will tend to be move to the bottom of the
            // matrix.  However, choosing Diag[Step] is desirable because it would
            // require no row and column interchanges, so inspect it first by
            // putting its Markowitz product at the end of the MarkowitzProd
            // vector.

            for (; ; )  // Endless for loop. 
            {
                while (matrix.MarkowitzProd[--index] >= MinMarkowitzProduct)
                {
                    // Just passing through. 
                }

                I = index; // pMarkowitzProduct - matrix.MarkowitzProd; // NOTE: Weird way to calculate index?

                // Assure that I is valid; if I < Step, terminate search. 
                if (I < Step)
                    break; // Endless for loop 
                if (I > matrix.Size)
                    I = Step;

                if ((pDiag = matrix.Diag[I]) == null)
                    continue; // Endless for loop 
                if ((Magnitude = SparseDefinitions.ELEMENT_MAG(pDiag)) <= matrix.AbsThreshold)
                    continue; // Endless for loop 

                if (matrix.MarkowitzProd[index] == 1)
                {
                    // Case where only one element exists in row and column other than diagonal. 

                    // Find off-diagonal elements. 
                    pOtherInRow = pDiag.NextInRow;
                    pOtherInCol = pDiag.NextInCol;
                    if (pOtherInRow == null && pOtherInCol == null)
                    {
                        pOtherInRow = matrix.FirstInRow[I];
                        while (pOtherInRow != null)
                        {
if (pOtherInRow.Col >= Step && pOtherInRow.Col != I)
                                break;
                            pOtherInRow = pOtherInRow.NextInRow;
                        }
                        pOtherInCol = matrix.FirstInCol[I];
                        while (pOtherInCol != null)
                        {
if (pOtherInCol.Row >= Step && pOtherInCol.Row != I)
                                break;
                            pOtherInCol = pOtherInCol.NextInCol;
                        }
                    }

                    /* Accept diagonal as pivot if diagonal is larger than off-diagonals and the
                    // off-diagonals are placed symmetricly. */
                    if (pOtherInRow != null && pOtherInCol != null)
                    {
                        if (pOtherInRow.Col == pOtherInCol.Row)
                        {
                            LargestOffDiagonal = Math.Max(SparseDefinitions.ELEMENT_MAG(pOtherInRow), SparseDefinitions.ELEMENT_MAG(pOtherInCol));
                            if (Magnitude >= LargestOffDiagonal)
                            {
                                // Accept pivot, it is unlikely to contribute excess error. 
                                return pDiag;
                            }
                        }
                    }
                }

                MinMarkowitzProduct = matrix.MarkowitzProd[index]; // *pMarkowitzProduct;
                ChosenPivot = pDiag;
            }  // End of endless for loop. 

            if (ChosenPivot != null)
            {
                LargestInCol = FindBiggestInColExclude(matrix, ChosenPivot, Step);
                if (SparseDefinitions.ELEMENT_MAG(ChosenPivot) <= matrix.RelThreshold * LargestInCol)
                    ChosenPivot = null;
            }
            return ChosenPivot;
        }

        public static MatrixElement SearchDiagonal(Matrix matrix, int Step)
        {
            int J;
            long MinMarkowitzProduct;
            //, *pMarkowitzProduct;
            int I;
            MatrixElement pDiag;
            int NumberOfTies = 0, Size = matrix.Size;
            MatrixElement ChosenPivot;
            double Magnitude, Ratio, RatioOfAccepted = 0, LargestInCol;

            ChosenPivot = null;
            MinMarkowitzProduct = long.MaxValue;
            int index = Size + 2;
            // pMarkowitzProduct = &(matrix.MarkowitzProd[Size+2]);
            matrix.MarkowitzProd[Size + 1] = matrix.MarkowitzProd[Step];

            // Start search of diagonal. 
            for (J = Size + 1; J > Step; J--)
            {
                if (matrix.MarkowitzProd[--index] > MinMarkowitzProduct)
                    continue; // for loop 
                if (J > matrix.Size)
                    I = Step;
                else
                    I = J;
                if ((pDiag = matrix.Diag[I]) == null)
                    continue; // for loop 
                if ((Magnitude = SparseDefinitions.ELEMENT_MAG(pDiag)) <= matrix.AbsThreshold)
                    continue; // for loop 

                // Test to see if diagonal's magnitude is acceptable. 
                LargestInCol = FindBiggestInColExclude(matrix, pDiag, Step);
                if (Magnitude <= matrix.RelThreshold * LargestInCol)
                    continue; // for loop 

                if (matrix.MarkowitzProd[index] < MinMarkowitzProduct)
                {
                    // Notice strict inequality in test. This is a new smallest MarkowitzProduct. 
                    ChosenPivot = pDiag;
                    MinMarkowitzProduct = matrix.MarkowitzProd[index];
                    RatioOfAccepted = LargestInCol / Magnitude;
                    NumberOfTies = 0;
                }
                else
                {
                    // This case handles Markowitz ties. 
                    NumberOfTies++;
                    Ratio = LargestInCol / Magnitude;
                    if (Ratio < RatioOfAccepted)
                    {
ChosenPivot = pDiag;
                        RatioOfAccepted = Ratio;
                    }
                    if (NumberOfTies >= MinMarkowitzProduct * Matrix.TIES_MULTIPLIER)
                        return ChosenPivot;
                }
            } // End of for(Step) 
            return ChosenPivot;
        }

        public static MatrixElement SearchEntireMatrix(Matrix matrix, int Step)
        {
            int I, Size = matrix.Size;
            MatrixElement pElement;
            int NumberOfTies = 0;
            long Product, MinMarkowitzProduct;
            MatrixElement ChosenPivot, pLargestElement = null;
            double Magnitude, LargestElementMag, Ratio, RatioOfAccepted = 0, LargestInCol;

            ChosenPivot = null;
            LargestElementMag = 0.0;
            MinMarkowitzProduct = long.MaxValue;

            // Start search of matrix on column by column basis. 
            for (I = Step; I <= Size; I++)
            {
                pElement = matrix.FirstInCol[I];

                while (pElement != null && pElement.Row < Step)
                    pElement = pElement.NextInCol;

                if ((LargestInCol = FindLargestInCol(pElement)) == 0.0)
                    continue; // for loop 

                while (pElement != null)
                {
                    /* Check to see if element is the largest encountered so far.  If so, record
                       its magnitude and address. */
                    if ((Magnitude = SparseDefinitions.ELEMENT_MAG(pElement)) > LargestElementMag)
                    {
                        LargestElementMag = Magnitude;
                        pLargestElement = pElement;
                    }
                    // Calculate element's MarkowitzProduct. 
                    Product = matrix.MarkowitzRow[pElement.Row] *
                              matrix.MarkowitzCol[pElement.Col];

                    // Test to see if element is acceptable as a pivot candidate. 
                    if ((Product <= MinMarkowitzProduct) && (Magnitude > matrix.RelThreshold * LargestInCol) && (Magnitude > matrix.AbsThreshold))
                    {
                        /* Test to see if element has lowest MarkowitzProduct yet found, or whether it
                           is tied with an element found earlier. */
                        if (Product < MinMarkowitzProduct)
                        {
                            // Notice strict inequality in test. This is a new smallest MarkowitzProduct. 
                            ChosenPivot = pElement;
                            MinMarkowitzProduct = Product;
                            RatioOfAccepted = LargestInCol / Magnitude;
                            NumberOfTies = 0;
                        }
                        else
                        {
                            // This case handles Markowitz ties. 
                            NumberOfTies++;
                            Ratio = LargestInCol / Magnitude;
                            if (Ratio < RatioOfAccepted)
                            {
ChosenPivot = pElement;
                                RatioOfAccepted = Ratio;
                            }
                            if (NumberOfTies >= MinMarkowitzProduct * Matrix.TIES_MULTIPLIER)
                                return ChosenPivot;
                        }
                    }
                    pElement = pElement.NextInCol;
                }  // End of while(pElement != null) 
            } // End of for(Step) 

            if (ChosenPivot != null) return ChosenPivot;

            if (LargestElementMag == 0.0)
            {
                matrix.Error = SparseError.Singular;
                return null;
            }

            matrix.Error = SparseError.SmallPivot;
            return pLargestElement;
        }

        public static double FindLargestInCol(MatrixElement pElement)
        {
            double Magnitude, Largest = 0.0;

            // Search column for largest element beginning at Element. 
            while (pElement != null)
            {
                if ((Magnitude = SparseDefinitions.ELEMENT_MAG(pElement)) > Largest)
                    Largest = Magnitude;
                pElement = pElement.NextInCol;
            }

            return Largest;
        }

        public static double FindBiggestInColExclude(Matrix matrix, MatrixElement pElement, int Step)
        {
            int Row;
            int Col;
            double Largest, Magnitude;

            Row = pElement.Row;
            Col = pElement.Col;
            pElement = matrix.FirstInCol[Col];

            // Travel down column until reduced submatrix is entered. 
            while ((pElement != null) && (pElement.Row < Step))
                pElement = pElement.NextInCol;

            // Initialize the variable Largest. 
            if (pElement.Row != Row)
                Largest = SparseDefinitions.ELEMENT_MAG(pElement);
            else
                Largest = 0.0;

            // Search rest of column for largest element, avoiding excluded element. 
            while ((pElement = pElement.NextInCol) != null)
            {
                if ((Magnitude = SparseDefinitions.ELEMENT_MAG(pElement)) > Largest)
                {
if (pElement.Row != Row)
                        Largest = Magnitude;
                }
            }

            return Largest;
        }

        public static void ExchangeRowsAndCols(Matrix matrix, MatrixElement pPivot, int Step)
        {
            int Row, Col;
            long OldMarkowitzProd_Step, OldMarkowitzProd_Row, OldMarkowitzProd_Col;

            Row = pPivot.Row;
            Col = pPivot.Col;
            matrix.PivotsOriginalRow = Row;
            matrix.PivotsOriginalCol = Col;

            if ((Row == Step) && (Col == Step)) return;

            // Exchange rows and columns. 
            if (Row == Col)
            {
                spcRowExchange(matrix, Step, Row);
                spcColExchange(matrix, Step, Col);
                SparseDefinitions.SWAP(ref matrix.MarkowitzProd[Step], ref matrix.MarkowitzProd[Row]);
                SparseDefinitions.SWAP(ref matrix.Diag[Row], ref matrix.Diag[Step]);
            }
            else
            {

                // Initialize variables that hold old Markowitz products. 
                OldMarkowitzProd_Step = matrix.MarkowitzProd[Step];
                OldMarkowitzProd_Row = matrix.MarkowitzProd[Row];
                OldMarkowitzProd_Col = matrix.MarkowitzProd[Col];

                // Exchange rows. 
                if (Row != Step)
                {
                    spcRowExchange(matrix, Step, Row);
                    matrix.NumberOfInterchangesIsOdd = !matrix.NumberOfInterchangesIsOdd;
                    matrix.MarkowitzProd[Row] = matrix.MarkowitzRow[Row] * matrix.MarkowitzCol[Row];

                    // Update singleton count. 
                    if ((matrix.MarkowitzProd[Row] == 0) != (OldMarkowitzProd_Row == 0))
                    {
                        if (OldMarkowitzProd_Row == 0)
                            matrix.Singletons--;
                        else
                            matrix.Singletons++;
                    }
                }

                // Exchange columns. 
                if (Col != Step)
                {
                    spcColExchange(matrix, Step, Col);
                    matrix.NumberOfInterchangesIsOdd = !matrix.NumberOfInterchangesIsOdd;
                    matrix.MarkowitzProd[Col] = matrix.MarkowitzCol[Col] * matrix.MarkowitzRow[Col];

                    // Update singleton count. 
                    if ((matrix.MarkowitzProd[Col] == 0) != (OldMarkowitzProd_Col == 0))
                    {
                        if (OldMarkowitzProd_Col == 0)
                            matrix.Singletons--;
                        else
                            matrix.Singletons++;
                    }

                    matrix.Diag[Col] = SparseBuild.spcFindElementInCol(matrix, Col, Col, false);
                }
                if (Row != Step)
                {
                    matrix.Diag[Row] = SparseBuild.spcFindElementInCol(matrix, Row, Row, false);
                }
                matrix.Diag[Step] = SparseBuild.spcFindElementInCol(matrix, Step, Step, false);

                // Update singleton count. 
                matrix.MarkowitzProd[Step] = matrix.MarkowitzCol[Step] * matrix.MarkowitzRow[Step];
                if ((matrix.MarkowitzProd[Step] == 0) != (OldMarkowitzProd_Step == 0))
                {
                    if (OldMarkowitzProd_Step == 0)
                        matrix.Singletons--;
                    else
                        matrix.Singletons++;
                }
            }
            return;
        }

        public static void spcRowExchange(this Matrix matrix, int Row1, int Row2)
        {
            MatrixElement Row1Ptr, Row2Ptr;
            int Column;
            MatrixElement Element1, Element2;

            if (Row1 > Row2)
                SparseDefinitions.SWAP(ref Row1, ref Row2);

            Row1Ptr = matrix.FirstInRow[Row1];
            Row2Ptr = matrix.FirstInRow[Row2];
            while (Row1Ptr != null || Row2Ptr != null)
            {
                // Exchange elements in rows while traveling from left to right. 
                if (Row1Ptr == null)
                {
                    Column = Row2Ptr.Col;
                    Element1 = null;
                    Element2 = Row2Ptr;
                    Row2Ptr = Row2Ptr.NextInRow;
                }
                else if (Row2Ptr == null)
                {
                    Column = Row1Ptr.Col;
                    Element1 = Row1Ptr;
                    Element2 = null;
                    Row1Ptr = Row1Ptr.NextInRow;
                }
                else if (Row1Ptr.Col < Row2Ptr.Col)
                {
                    Column = Row1Ptr.Col;
                    Element1 = Row1Ptr;
                    Element2 = null;
                    Row1Ptr = Row1Ptr.NextInRow;
                }
                else if (Row1Ptr.Col > Row2Ptr.Col)
                {
                    Column = Row2Ptr.Col;
                    Element1 = null;
                    Element2 = Row2Ptr;
                    Row2Ptr = Row2Ptr.NextInRow;
                }
                else   // Row1Ptr.Col == Row2Ptr.Col 
                {
                    Column = Row1Ptr.Col;
                    Element1 = Row1Ptr;
                    Element2 = Row2Ptr;
                    Row1Ptr = Row1Ptr.NextInRow;
                    Row2Ptr = Row2Ptr.NextInRow;
                }

                ExchangeColElements(matrix, Row1, Element1, Row2, Element2, Column);
            }  // end of while(Row1Ptr != null ||  Row2Ptr != null) 

            if (matrix.InternalVectorsAllocated)
                SparseDefinitions.SWAP(ref matrix.MarkowitzRow[Row1], ref matrix.MarkowitzRow[Row2]);
            SparseDefinitions.SWAP(ref matrix.FirstInRow[Row1], ref matrix.FirstInRow[Row2]);
            SparseDefinitions.SWAP(ref matrix.IntToExtRowMap[Row1], ref matrix.IntToExtRowMap[Row2]);
            matrix.ExtToIntRowMap[matrix.IntToExtRowMap[Row1]] = Row1;
            matrix.ExtToIntRowMap[matrix.IntToExtRowMap[Row2]] = Row2;
        }

        public static void spcColExchange(Matrix matrix, int Col1, int Col2)
        {
            MatrixElement Col1Ptr, Col2Ptr;
            int Row;
            MatrixElement Element1, Element2;

            /* Begin `spcColExchange'. */
            if (Col1 > Col2)
                SparseDefinitions.SWAP(ref Col1, ref Col2);

            Col1Ptr = matrix.FirstInCol[Col1];
            Col2Ptr = matrix.FirstInCol[Col2];
            while (Col1Ptr != null || Col2Ptr != null)
            {
                /* Exchange elements in rows while traveling from top to bottom. */
                if (Col1Ptr == null)
                {
                    Row = Col2Ptr.Row;
                    Element1 = null;
                    Element2 = Col2Ptr;
                    Col2Ptr = Col2Ptr.NextInCol;
                }
                else if (Col2Ptr == null)
                {
                    Row = Col1Ptr.Row;
                    Element1 = Col1Ptr;
                    Element2 = null;
                    Col1Ptr = Col1Ptr.NextInCol;
                }
                else if (Col1Ptr.Row < Col2Ptr.Row)
                {
                    Row = Col1Ptr.Row;
                    Element1 = Col1Ptr;
                    Element2 = null;
                    Col1Ptr = Col1Ptr.NextInCol;
                }
                else if (Col1Ptr.Row > Col2Ptr.Row)
                {
                    Row = Col2Ptr.Row;
                    Element1 = null;
                    Element2 = Col2Ptr;
                    Col2Ptr = Col2Ptr.NextInCol;
                }
                else   /* Col1Ptr.Row == Col2Ptr.Row */
                {
                    Row = Col1Ptr.Row;
                    Element1 = Col1Ptr;
                    Element2 = Col2Ptr;
                    Col1Ptr = Col1Ptr.NextInCol;
                    Col2Ptr = Col2Ptr.NextInCol;
                }

                ExchangeRowElements(matrix, Col1, Element1, Col2, Element2, Row);
            }  /* end of while(Col1Ptr != null || Col2Ptr != null) */

            if (matrix.InternalVectorsAllocated)
                SparseDefinitions.SWAP(ref matrix.MarkowitzCol[Col1], ref matrix.MarkowitzCol[Col2]);

            SparseDefinitions.SWAP(ref matrix.FirstInCol[Col1], ref matrix.FirstInCol[Col2]);
            SparseDefinitions.SWAP(ref matrix.IntToExtColMap[Col1], ref matrix.IntToExtColMap[Col2]);
            matrix.ExtToIntColMap[matrix.IntToExtColMap[Col1]] = Col1;
            matrix.ExtToIntColMap[matrix.IntToExtColMap[Col2]] = Col2;

            return;
        }

        public static void ExchangeColElements(Matrix matrix, int Row1, MatrixElement Element1, int Row2, MatrixElement Element2, int Column)
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
                pElement = pElement.NextInCol; // *ElementAboveRow1;
            }
            if (Element1 != null)
            {
                ElementBelowRow1 = Element1.NextInCol;
                if (Element2 == null)
                {
                    // Element2 does not exist, move Element1 down to Row2. 
                    if (ElementBelowRow1 != null && ElementBelowRow1.Row < Row2)
                    {
                        // Element1 must be removed from linked list and moved.
                        if (ElementAboveRow1 != null)
                            ElementAboveRow1.NextInCol = ElementBelowRow1;
                        else
                            matrix.FirstInCol[Column] = ElementBelowRow1;
                        // *ElementAboveRow1 = ElementBelowRow1;

                        // Search column for Row2. 
                        pElement = ElementBelowRow1;
                        do
                        {
                            ElementAboveRow2 = pElement; // &(pElement.NextInCol);
                            pElement = pElement.NextInCol; // *ElementAboveRow2;
                        } while (pElement != null && pElement.Row < Row2);

                        // Place Element1 in Row2
                        if (ElementAboveRow2 != null)
                            ElementAboveRow2.NextInCol = Element1;
                        else
                            matrix.FirstInCol[Column] = Element1;
                        // *ElementAboveRow2 = Element1;
                        Element1.NextInCol = pElement;
                        if (ElementAboveRow1 != null)
                            ElementAboveRow1.NextInCol = ElementBelowRow1;
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
                        Element1.NextInCol = Element2.NextInCol;
                        Element2.NextInCol = Element1;
                        if (ElementAboveRow1 != null)
                            ElementAboveRow1.NextInCol = Element2;
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
                            pElement = pElement.NextInCol; // *ElementAboveRow2;
                        } while (pElement.Row < Row2);

                        ElementBelowRow2 = Element2.NextInCol;

                        // Switch Element1 and Element2
                        if (ElementAboveRow1 != null)
                            ElementAboveRow1.NextInCol = Element2;
                        else
                            matrix.FirstInCol[Column] = Element2;
                        // *ElementAboveRow1 = Element2;
                        Element2.NextInCol = ElementBelowRow1;
                        if (ElementAboveRow2 != null)
                            ElementAboveRow2.NextInCol = Element1;
                        else
                            matrix.FirstInCol[Column] = Element1;
                        // *ElementAboveRow2 = Element1;
                        Element1.NextInCol = ElementBelowRow2;
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
                        pElement = pElement.NextInCol; // *ElementAboveRow2;
                    } while (pElement.Row < Row2);

                    ElementBelowRow2 = Element2.NextInCol;

                    // Move Element2 to Row1
                    if (ElementAboveRow2 != null)
                        ElementAboveRow2.NextInCol = Element2.NextInCol;
                    else
                        matrix.FirstInCol[Column] = Element2.NextInCol;
                    // *ElementAboveRow2 = Element2.NextInCol;
                    if (ElementAboveRow1 != null)
                        ElementAboveRow1.NextInCol = Element2;
                    else
                        matrix.FirstInCol[Column] = Element2;
                    // *ElementAboveRow1 = Element2;
                    Element2.NextInCol = ElementBelowRow1;
                }
                Element2.Row = Row1;
            }
            return;
        }

        public static void ExchangeRowElements(Matrix matrix, int Col1, MatrixElement Element1, int Col2, MatrixElement Element2, int Row)
        {
            MatrixElement ElementLeftOfCol1, ElementLeftOfCol2;
            MatrixElement ElementRightOfCol1, ElementRightOfCol2;
            MatrixElement pElement;

            // Begin `ExchangeRowElements'. 
            // Search to find the ElementLeftOfCol1. 
            ElementLeftOfCol1 = null; // &(matrix.FirstInRow[Row]);
            pElement = matrix.FirstInRow[Row]; // *ElementLeftOfCol1;
            while (pElement.Col < Col1)
            {
                ElementLeftOfCol1 = pElement; // ElementLeftOfCol1 = &(pElement.NextInRow);
                pElement = pElement.NextInRow; // pElement = *ElementLeftOfCol1;
            }
            if (Element1 != null)
            {
                ElementRightOfCol1 = Element1.NextInRow;
                if (Element2 == null)
                {
                    // Element2 does not exist, move Element1 to right to Col2. 
                    if (ElementRightOfCol1 != null && ElementRightOfCol1.Col < Col2)
                    {
                        // Element1 must be removed from linked list and moved.
                        if (ElementLeftOfCol1 != null)
                            ElementLeftOfCol1.NextInRow = ElementRightOfCol1;
                        else
                            matrix.FirstInRow[Row] = ElementRightOfCol1;
                        // *ElementLeftOfCol1 = ElementRightOfCol1;

                        // Search Row for Col2. 
                        pElement = ElementRightOfCol1;
                        do
                        {
                            ElementLeftOfCol2 = pElement; // &(pElement.NextInRow);
                            pElement = pElement.NextInRow; // *ElementLeftOfCol2;
                        } while (pElement != null && pElement.Col < Col2);

                        // Place Element1 in Col2. 
                        if (ElementLeftOfCol2 != null)
                            ElementLeftOfCol2.NextInRow = Element1;
                        else
                            matrix.FirstInRow[Row] = Element1;
                        // *ElementLeftOfCol2 = Element1;
                        Element1.NextInRow = pElement;

                        if (ElementLeftOfCol1 != null)
                            ElementLeftOfCol1.NextInRow = ElementRightOfCol1;
                        else
                            matrix.FirstInRow[Row] = ElementRightOfCol1;
                        // *ElementLeftOfCol1 = ElementRightOfCol1;
                    }
                    Element1.Col = Col2;
                }
                else
                {
                    // Element2 does exist, and the two elements must be exchanged. 
                    if (ElementRightOfCol1.Col == Col2)
                    {
                        // Element2 is just right of Element1, exchange them. 
                        Element1.NextInRow = Element2.NextInRow;
                        Element2.NextInRow = Element1;

                        if (ElementLeftOfCol1 != null)
                            ElementLeftOfCol1.NextInRow = Element2;
                        else
                            matrix.FirstInRow[Row] = Element2;
                        // *ElementLeftOfCol1 = Element2;
                    }
                    else
                    {
                        // Element2 is not just right of Element1 and must be searched for. 
                        pElement = ElementRightOfCol1;
                        do
                        {
                            ElementLeftOfCol2 = pElement; // &(pElement.NextInRow);
                            pElement = pElement.NextInRow; // *ElementLeftOfCol2;
                        } while (pElement.Col < Col2);

                        ElementRightOfCol2 = Element2.NextInRow;

                        // Switch Element1 and Element2. 
                        if (ElementLeftOfCol1 != null)
                            ElementLeftOfCol1.NextInRow = Element2;
                        else
                            matrix.FirstInRow[Row] = Element2;
                        // *ElementLeftOfCol1 = Element2;
                        Element2.NextInRow = ElementRightOfCol1;

                        if (ElementLeftOfCol2 != null)
                            ElementLeftOfCol2.NextInRow = Element1;
                        else
                            matrix.FirstInRow[Row] = Element1;
                        // *ElementLeftOfCol2 = Element1;
                        Element1.NextInRow = ElementRightOfCol2;
                    }
                    Element1.Col = Col2;
                    Element2.Col = Col1;
                }
            }
            else
            {
                // Element1 does not exist. 
                ElementRightOfCol1 = pElement;

                // Find Element2. 
                if (ElementRightOfCol1.Col != Col2)
                {
                    do
                    {
                        ElementLeftOfCol2 = pElement; // &(pElement.NextInRow);
                        pElement = pElement.NextInRow; // *ElementLeftOfCol2;
                    } while (pElement.Col < Col2);

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
                Element2.Col = Col1;
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

                pSub = pUpper.NextInCol;
                pLower = pPivot.NextInCol;
                while (pLower != null)
                {
                    Row = pLower.Row;

                    // Find element in row that lines up with current lower triangular element. 
                    while (pSub != null && pSub.Row < Row)
                        pSub = pSub.NextInCol;

                    // Test to see if desired element was not found, if not, create fill-in. 
                    if (pSub == null || pSub.Row > Row)
                    {
                        pSub = CreateFillin(matrix, Row, pUpper.Col);
                    }
                    pSub.Value.Real -= pUpper.Value.Real * pLower.Value.Real;
                    pSub = pSub.NextInCol;
                    pLower = pLower.NextInCol;
                }
                pUpper = pUpper.NextInRow;
            }
        }

        /// <summary>
        /// Eliminate a row with complex numbers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="pPivot">Current pivot</param>
        public static void ComplexRowColElimination(Matrix matrix, MatrixElement pPivot)
        {
            MatrixElement pSub;
            int Row;
            MatrixElement pLower, pUpper;

            // Test for zero pivot. 
            if (SparseDefinitions.ELEMENT_MAG(pPivot) == 0.0)
            {
                MatrixIsSingular(matrix, pPivot.Row);
                return;
            }
            SparseDefinitions.CMPLX_RECIPROCAL(ref pPivot.Value, pPivot);

            pUpper = pPivot.NextInRow;
            while (pUpper != null)
            {
                // Calculate upper triangular element. 
                // Cmplx expr: *pUpper = *pUpper * (1.0 / *pPivot)
                SparseDefinitions.CMPLX_MULT_ASSIGN(ref pUpper.Value, pPivot);

                pSub = pUpper.NextInCol;
                pLower = pPivot.NextInCol;
                while (pLower != null)
                {
                    Row = pLower.Row;

                    // Find element in row that lines up with current lower triangular element. 
                    while (pSub != null && pSub.Row < Row)
                        pSub = pSub.NextInCol;

                    // Test to see if desired element was not found, if not, create fill-in. 
                    if (pSub == null || pSub.Row > Row)
                    {
                        pSub = CreateFillin(matrix, Row, pUpper.Col);
                    }

                    // Cmplx expr: pElement -= *pUpper * pLower
                    SparseDefinitions.CMPLX_MULT_SUBT_ASSIGN(ref pSub.Value, pUpper, pLower);
                    pSub = pSub.NextInCol;
                    pLower = pLower.NextInCol;
                }
                pUpper = pUpper.NextInRow;
            }
        }

        public static void UpdateMarkowitzNumbers(Matrix matrix, MatrixElement pPivot)
        {
            int Row, Col;
            MatrixElement ColPtr, RowPtr;
            int[] MarkoRow = matrix.MarkowitzRow, MarkoCol = matrix.MarkowitzCol;
            double Product;

            // Update Markowitz numbers. 
            for (ColPtr = pPivot.NextInCol; ColPtr != null; ColPtr = ColPtr.NextInCol)
            {
                Row = ColPtr.Row;
                --MarkoRow[Row];

                // Form Markowitz product while being cautious of overflows. 
                if ((MarkoRow[Row] > short.MaxValue && MarkoCol[Row] != 0) ||
                    (MarkoCol[Row] > short.MaxValue && MarkoRow[Row] != 0))
                {
                    Product = MarkoCol[Row] * MarkoRow[Row];
                    if (Product >= long.MaxValue)
                        matrix.MarkowitzProd[Row] = long.MaxValue;
                    else
                        matrix.MarkowitzProd[Row] = (long)Product;
                }
                else matrix.MarkowitzProd[Row] = MarkoRow[Row] * MarkoCol[Row];
                if (MarkoRow[Row] == 0)
                    matrix.Singletons++;
            }

            for (RowPtr = pPivot.NextInRow; RowPtr != null; RowPtr = RowPtr.NextInRow)
            {
                Col = RowPtr.Col;
                --MarkoCol[Col];

                // Form Markowitz product while being cautious of overflows. 
                if ((MarkoRow[Col] > short.MaxValue && MarkoCol[Col] != 0) ||
                    (MarkoCol[Col] > short.MaxValue && MarkoRow[Col] != 0))
                {
                    Product = MarkoCol[Col] * MarkoRow[Col];
                    if (Product >= long.MaxValue)
                        matrix.MarkowitzProd[Col] = long.MaxValue;
                    else
                        matrix.MarkowitzProd[Col] = (long)Product;
                }
                else matrix.MarkowitzProd[Col] = MarkoRow[Col] * MarkoCol[Col];
                if ((MarkoCol[Col] == 0) && (MarkoRow[Col] != 0))
                    matrix.Singletons++;
            }
            return;
        }

        public static MatrixElement CreateFillin(Matrix matrix, int Row, int Col)
        {
            MatrixElement pElement, aboveElement;

            // Find Element above fill-in. 
            // ppElementAbove = &matrix.FirstInCol[Col];
            aboveElement = null;
            pElement = matrix.FirstInCol[Col];
            while (pElement != null)
            {
                if (pElement.Row < Row)
                {
                    aboveElement = pElement;
                    pElement = pElement.NextInCol;
                }
                else break; // while loop 
            }

            // End of search, create the element. 
            pElement = SparseBuild.spcCreateElement(matrix, Row, Col, aboveElement, true);

            // Update Markowitz counts and products. 
            matrix.MarkowitzProd[Row] = ++matrix.MarkowitzRow[Row] * matrix.MarkowitzCol[Row];
            if ((matrix.MarkowitzRow[Row] == 1) && (matrix.MarkowitzCol[Row] != 0))
                matrix.Singletons--;
            matrix.MarkowitzProd[Col] = ++matrix.MarkowitzCol[Col] * matrix.MarkowitzRow[Col];
            if ((matrix.MarkowitzRow[Col] != 0) && (matrix.MarkowitzCol[Col] == 1))
                matrix.Singletons--;

            return pElement;
        }

        /// <summary>
        /// We have a singular matrix
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="Step">Current step</param>
        /// <returns></returns>
        public static SparseError MatrixIsSingular(Matrix matrix, int Step)
        {
            matrix.SingularRow = matrix.IntToExtRowMap[Step];
            matrix.SingularCol = matrix.IntToExtColMap[Step];
            return (matrix.Error = SparseError.Singular);
        }

        /// <summary>
        /// We have a zero on the diagonal
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="Step">Current step</param>
        /// <returns></returns>
        public static SparseError ZeroPivot(Matrix matrix, int Step)
        {
            matrix.SingularRow = matrix.IntToExtRowMap[Step];
            matrix.SingularCol = matrix.IntToExtColMap[Step];
            return (matrix.Error = SparseError.ZeroDiagonal);
        }
    }
}
