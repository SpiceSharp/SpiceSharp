using System;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// A class that handles finding pivots in the matrix
    /// </summary>
    public class SparsePivoting
    {
        internal bool[] DoCmplxDirect = null;
        internal bool[] DoRealDirect = null;
        internal int[] MarkowitzRow = null;
        internal int[] MarkowitzCol = null;
        internal long[] MarkowitzProd = null;

        internal bool Partitioned = false;
        internal char PivotSelectionMethod = '\0';
        internal bool InternalVectorsAllocated = false;
        internal int Singletons = 0;
        internal int PivotsOriginalCol = 0;
        internal int PivotsOriginalRow = 0;

        // This kind of should not be here...
        internal ElementValue[] Intermediate;

        /// <summary>
        /// Clear all vectors
        /// </summary>
        public void Clear()
        {
            MarkowitzRow = null;
            MarkowitzCol = null;
            MarkowitzProd = null;
            DoRealDirect = null;
            DoCmplxDirect = null;
            Intermediate = null;
            InternalVectorsAllocated = false;
        }

        /// <summary>
        /// Create internal vectors
        /// </summary>
        public void CreateInternalVectors(int size)
        {
            if (size < 0)
                throw new SparseException("Invalid size {0}".FormatString(size));

            if (MarkowitzRow == null)
                MarkowitzRow = new int[size + 1];
            if (MarkowitzCol == null)
                MarkowitzCol = new int[size + 1];
            if (MarkowitzProd == null)
                MarkowitzProd = new long[size + 2];

            // Create DoDirect vectors for use in spFactor()
            if (DoRealDirect == null)
                DoRealDirect = new bool[size + 1];
            if (DoCmplxDirect == null)
                DoCmplxDirect = new bool[size + 1];

            // Create Intermediate vectors for use in MatrixSolve
            if (Intermediate == null)
                Intermediate = new ElementValue[size + 1];
            InternalVectorsAllocated = true;
        }

        /// <summary>
        /// Partition the matrix
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="Mode">The mode</param>
        public void Partition(Matrix matrix, SparsePartition Mode)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            MatrixElement pElement, pColumn;
            int Step, Size;
            int[] Nc, No;
            long[] Nm;

            if (Partitioned)
                return;
            Size = matrix.IntSize;
            Partitioned = true;

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
            Nc = MarkowitzRow;
            No = MarkowitzCol;
            Nm = MarkowitzProd;

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

        /// <summary>
        /// Count markowitz
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="rhs">Right hand side</param>
        /// <param name="Step">Current step</param>
        public void CountMarkowitz(Matrix matrix, Vector<double> rhs, int Step)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            int Count, I, Size = matrix.IntSize;
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
                ExtRow = matrix.Translation.IntToExtRowMap[I];

                if (rhs != null)
                {
                    if (rhs[ExtRow] != 0.0)
                        Count++;
                }
                MarkowitzRow[I] = Count;
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
                MarkowitzCol[I] = Count;
            }
        }

        /// <summary>
        /// Calculate markowitz products
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="Step"></param>
        public void MarkowitzProducts(Matrix matrix, int Step)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            int I;
            long Product;
            int Size = matrix.IntSize;
            double fProduct;

            Singletons = 0;

            for (I = Step; I <= Size; I++)
            {
                // If chance of overflow, use real numbers. 
                if ((MarkowitzRow[I] > short.MaxValue && MarkowitzCol[I] != 0) ||
                     (MarkowitzCol[I] > short.MaxValue && MarkowitzRow[I] != 0))
                {
                    fProduct = (double)MarkowitzRow[I] * MarkowitzCol[I];
                    if (fProduct >= long.MaxValue)
                        MarkowitzProd[I] = long.MaxValue;
                    else
                        MarkowitzProd[I] = (long)fProduct;
                }
                else
                {
                    Product = MarkowitzRow[I] * MarkowitzCol[I];
                    if ((MarkowitzProd[I] = Product) == 0)
                        Singletons++;
                }
            }
        }

        /// <summary>
        /// Search for a pivot in the matrix
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="Step">Step</param>
        /// <param name="DiagPivoting">Use the diagonal for searching a pivot</param>
        /// <returns></returns>
        public MatrixElement SearchForPivot(Matrix matrix, int Step, bool DiagPivoting)
        {
            MatrixElement ChosenPivot;

            // If singletons exist, look for an acceptable one to use as pivot. 
            if (Singletons != 0)
            {
                ChosenPivot = SearchForSingleton(matrix, Step);
                if (ChosenPivot != null)
                {
                    PivotSelectionMethod = 's';
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
                    PivotSelectionMethod = 'q';
                    return ChosenPivot;
                }

                // Quick search of diagonal failed, carefully search diagonal and check each
                // pivot candidate numerically before even tentatively accepting it.

                ChosenPivot = SearchDiagonal(matrix, Step);
                if (ChosenPivot != null)
                {
                    PivotSelectionMethod = 'd';
                    return ChosenPivot;
                }
            }

            // No acceptable pivot found yet, search entire matrix. 
            ChosenPivot = SearchEntireMatrix(matrix, Step);
            PivotSelectionMethod = 'e';

            return ChosenPivot;
        }

        /// <summary>
        /// Search for singletons
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="Step">The current step</param>
        /// <returns></returns>
        private MatrixElement SearchForSingleton(Matrix matrix, int Step)
        {
            MatrixElement ChosenPivot;
            int I;
            long[] pMarkowitzProduct;
            int singletons;
            double PivotMag;

            // Initialize pointer that is to scan through MarkowitzProduct vector. 
            int index = matrix.IntSize + 1;
            pMarkowitzProduct = MarkowitzProd;
            MarkowitzProd[matrix.IntSize + 1] = MarkowitzProd[Step];

            // Decrement the count of available singletons, on the assumption that an
            // acceptable one will be found
            singletons = Singletons--;

            // Assure that following while loop will always terminate, this is just
            // preventive medicine, if things are working right this should never
            // be needed.
            MarkowitzProd[Step - 1] = 0;

            while (singletons-- > 0)
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
                I = index + 1;

                // Assure that I is valid. 
                if (I < Step) break;  // while (Singletons-- > 0) 
                if (I > matrix.IntSize) I = Step;

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
                    if (MarkowitzCol[I] == 0)
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
                            if (MarkowitzRow[I] == 0)
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
            this.Singletons++;
            return null;
        }

        /// <summary>
        /// Quickly search the diagonal
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="Step">The current step</param>
        /// <returns></returns>
        private MatrixElement QuicklySearchDiagonal(Matrix matrix, int Step)
        {
            long MinMarkowitzProduct;
            // long pMarkowitzProduct;
            MatrixElement pDiag;
            int I;
            MatrixElement ChosenPivot, pOtherInRow, pOtherInCol;
            double Magnitude, LargestInCol, LargestOffDiagonal;

            ChosenPivot = null;
            MinMarkowitzProduct = long.MaxValue;
            int index = matrix.IntSize + 2;
            // pMarkowitzProduct = matrix.MarkowitzProd[index];
            MarkowitzProd[matrix.IntSize + 1] = MarkowitzProd[Step];

            // Assure that following while loop will always terminate. 
            MarkowitzProd[Step - 1] = -1;

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
                while (MarkowitzProd[--index] >= MinMarkowitzProduct)
                {
                    // Just passing through. 
                }

                I = index; // pMarkowitzProduct - matrix.MarkowitzProd; // NOTE: Weird way to calculate index?

                // Assure that I is valid; if I < Step, terminate search. 
                if (I < Step)
                    break; // Endless for loop 
                if (I > matrix.IntSize)
                    I = Step;

                if ((pDiag = matrix.Diag[I]) == null)
                    continue; // Endless for loop 
                if ((Magnitude = SparseDefinitions.ELEMENT_MAG(pDiag)) <= matrix.AbsThreshold)
                    continue; // Endless for loop 

                if (MarkowitzProd[index] == 1)
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

                MinMarkowitzProduct = MarkowitzProd[index]; // *pMarkowitzProduct;
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

        /// <summary>
        /// Search the diagonal
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="Step">Step</param>
        /// <returns></returns>
        private MatrixElement SearchDiagonal(Matrix matrix, int Step)
        {
            int J;
            long MinMarkowitzProduct;
            //, *pMarkowitzProduct;
            int I;
            MatrixElement pDiag;
            int NumberOfTies = 0, Size = matrix.IntSize;
            MatrixElement ChosenPivot;
            double Magnitude, Ratio, RatioOfAccepted = 0, LargestInCol;

            ChosenPivot = null;
            MinMarkowitzProduct = long.MaxValue;
            int index = Size + 2;
            // pMarkowitzProduct = &(matrix.MarkowitzProd[Size+2]);
            MarkowitzProd[Size + 1] = MarkowitzProd[Step];

            // Start search of diagonal. 
            for (J = Size + 1; J > Step; J--)
            {
                if (MarkowitzProd[--index] > MinMarkowitzProduct)
                    continue; // for loop 
                if (J > matrix.IntSize)
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

                if (MarkowitzProd[index] < MinMarkowitzProduct)
                {
                    // Notice strict inequality in test. This is a new smallest MarkowitzProduct. 
                    ChosenPivot = pDiag;
                    MinMarkowitzProduct = MarkowitzProd[index];
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

        /// <summary>
        /// Search the entire matrix!
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="Step">Current step</param>
        /// <returns></returns>
        private MatrixElement SearchEntireMatrix(Matrix matrix, int Step)
        {
            int I, Size = matrix.IntSize;
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
                    Product = MarkowitzRow[pElement.Row] * MarkowitzCol[pElement.Col];

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

        /// <summary>
        /// Find largest element in the column
        /// </summary>
        /// <param name="pElement">Element where we need to start searching</param>
        /// <returns></returns>
        public double FindLargestInCol(MatrixElement pElement)
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

        /// <summary>
        /// Find biggest value in column excluding
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="pElement">Element</param>
        /// <param name="Step">Step</param>
        /// <returns></returns>
        private double FindBiggestInColExclude(Matrix matrix, MatrixElement pElement, int Step)
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

        /// <summary>
        /// Update Markowitz numbers
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="pPivot">Pivot element</param>
        public void UpdateMarkowitzNumbers(Matrix matrix, MatrixElement pPivot)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (pPivot == null)
                throw new ArgumentNullException(nameof(pPivot));


            int Row, Col;
            MatrixElement ColPtr, RowPtr;
            double Product;

            // Update Markowitz numbers. 
            for (ColPtr = pPivot.NextInCol; ColPtr != null; ColPtr = ColPtr.NextInCol)
            {
                Row = ColPtr.Row;
                --MarkowitzRow[Row];

                // Form Markowitz product while being cautious of overflows. 
                if ((MarkowitzRow[Row] > short.MaxValue && MarkowitzCol[Row] != 0) ||
                    (MarkowitzCol[Row] > short.MaxValue && MarkowitzRow[Row] != 0))
                {
                    Product = MarkowitzCol[Row] * MarkowitzRow[Row];
                    if (Product >= long.MaxValue)
                        MarkowitzProd[Row] = long.MaxValue;
                    else
                        MarkowitzProd[Row] = (long)Product;
                }
                else
                    MarkowitzProd[Row] = MarkowitzRow[Row] * MarkowitzCol[Row];
                if (MarkowitzRow[Row] == 0)
                    Singletons++;
            }

            for (RowPtr = pPivot.NextInRow; RowPtr != null; RowPtr = RowPtr.NextInRow)
            {
                Col = RowPtr.Col;
                --MarkowitzCol[Col];

                // Form Markowitz product while being cautious of overflows. 
                if ((MarkowitzRow[Col] > short.MaxValue && MarkowitzCol[Col] != 0) ||
                    (MarkowitzCol[Col] > short.MaxValue && MarkowitzRow[Col] != 0))
                {
                    Product = MarkowitzCol[Col] * MarkowitzRow[Col];
                    if (Product >= long.MaxValue)
                        MarkowitzProd[Col] = long.MaxValue;
                    else
                        MarkowitzProd[Col] = (long)Product;
                }
                else
                    MarkowitzProd[Col] = MarkowitzRow[Col] * MarkowitzCol[Col];
                if ((MarkowitzCol[Col] == 0) && (MarkowitzRow[Col] != 0))
                    Singletons++;
            }
            return;
        }
    }
}
