using System;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// A class that handles finding pivots in the matrix
    /// </summary>
    public class SparsePivoting<T>
    {
        /// <summary>
        /// Constants
        /// </summary>
        const SparsePartition DEFAULT_PARTITION = SparsePartition.Auto;
        const int TIES_MULTIPLIER = 5;

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

        /// <summary>
        /// Absolute threshold for pivoting
        /// </summary>
        public double AbsThreshold { get; set; } = 1e-13;

        /// <summary>
        /// Relative threshold for pivoting
        /// </summary>
        public double RelThreshold { get; set; } = 1e-3;

        // This kind of should not be here...
        internal Element<T>[] Intermediate;

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
            {
                Intermediate = new Element<T>[size + 1];
                for (int i = 0; i < Intermediate.Length; i++)
                    Intermediate[i] = ElementFactory.Create<T>();
            }
            InternalVectorsAllocated = true;
        }

        /// <summary>
        /// Partition the matrix
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="mode">The mode</param>
        public void Partition(Matrix<T> matrix, SparsePartition mode)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            MatrixElement<T> pElement, pColumn;
            int Step, Size;
            int[] Nc, No;
            long[] Nm;

            if (Partitioned)
                return;
            Size = matrix.IntSize;
            Partitioned = true;

            // If partition is specified by the user, this is easy
            if (mode == SparsePartition.Default)
                mode = DEFAULT_PARTITION;
            if (mode == SparsePartition.Direct)
            {
                for (Step = 1; Step <= Size; Step++)
                {
                    DoRealDirect[Step] = true;
                    DoCmplxDirect[Step] = true;
                }
                return;
            }
            else if (mode == SparsePartition.Indirect)
            {
                for (Step = 1; Step <= Size; Step++)
                {
                    DoRealDirect[Step] = false;
                    DoCmplxDirect[Step] = false;
                }
                return;
            }
            else if (mode != SparsePartition.Auto)
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
                    pElement = pElement.NextInColumn;
                }

                pColumn = matrix.FirstInCol[Step];
                while (pColumn.Row < Step)
                {
                    pElement = matrix.Diag[pColumn.Row];
                    Nm[Step]++;
                    while ((pElement = pElement.NextInColumn) != null)
                        No[Step]++;
                    pColumn = pColumn.NextInColumn;
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
        /// <param name="step">Current step</param>
        public void CountMarkowitz(Matrix<T> matrix, Vector<double> rhs, int step)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            int Count, I, Size = matrix.IntSize;
            MatrixElement<T> pElement;
            int ExtRow;

            // Generate MarkowitzRow Count for each row
            for (I = step; I <= Size; I++)
            {
                // Set Count to -1 initially to remove count due to pivot element
                Count = -1;
                pElement = matrix.FirstInRow[I];
                while (pElement != null && pElement.Column < step)
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
            for (I = step; I <= Size; I++)
            {
                // Set Count to -1 initially to remove count due to pivot element
                Count = -1;
                pElement = matrix.FirstInCol[I];
                while (pElement != null && pElement.Row < step)
                    pElement = pElement.NextInColumn;
                while (pElement != null)
                {
                    Count++;
                    pElement = pElement.NextInColumn;
                }
                MarkowitzCol[I] = Count;
            }
        }

        /// <summary>
        /// Calculate markowitz products
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="step"></param>
        public void MarkowitzProducts(Matrix<T> matrix, int step)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            int I;
            long Product;
            int Size = matrix.IntSize;
            double fProduct;

            Singletons = 0;

            for (I = step; I <= Size; I++)
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
        /// <param name="step">Step</param>
        /// <param name="diagonalPivoting">Use the diagonal for searching a pivot</param>
        /// <returns></returns>
        public MatrixElement<T> SearchForPivot(Matrix<T> matrix, int step, bool diagonalPivoting)
        {
            MatrixElement<T> ChosenPivot;

            // If singletons exist, look for an acceptable one to use as pivot. 
            if (Singletons != 0)
            {
                ChosenPivot = SearchForSingleton(matrix, step);
                if (ChosenPivot != null)
                {
                    PivotSelectionMethod = 's';
                    return ChosenPivot;
                }
            }

            if (diagonalPivoting)
            {

                // Either no singletons exist or they weren't acceptable.  Take quick first
                // pass at searching diagonal.  First search for element on diagonal of 
                // remaining submatrix with smallest Markowitz product, then check to see
                // if it okay numerically.  If not, QuicklySearchDiagonal fails.

                ChosenPivot = QuicklySearchDiagonal(matrix, step);
                if (ChosenPivot != null)
                {
                    PivotSelectionMethod = 'q';
                    return ChosenPivot;
                }

                // Quick search of diagonal failed, carefully search diagonal and check each
                // pivot candidate numerically before even tentatively accepting it.

                ChosenPivot = SearchDiagonal(matrix, step);
                if (ChosenPivot != null)
                {
                    PivotSelectionMethod = 'd';
                    return ChosenPivot;
                }
            }

            // No acceptable pivot found yet, search entire matrix. 
            ChosenPivot = SearchEntireMatrix(matrix, step);
            PivotSelectionMethod = 'e';

            return ChosenPivot;
        }

        /// <summary>
        /// Search for singletons
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="Step">The current step</param>
        /// <returns></returns>
        MatrixElement<T> SearchForSingleton(Matrix<T> matrix, int Step)
        {
            var pivoting = matrix.Pivoting;
            MatrixElement<T> ChosenPivot;
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
                    PivotMag = ChosenPivot.Element.Magnitude;
                    if (PivotMag > pivoting.AbsThreshold && PivotMag > pivoting.RelThreshold * FindBiggestInColExclude(matrix, ChosenPivot, Step))
                        return ChosenPivot;
                }
                else
                {
                    // Singleton does not lie on diagonal, find it. 
                    if (MarkowitzCol[I] == 0)
                    {
                        ChosenPivot = matrix.FirstInCol[I];
                        while ((ChosenPivot != null) && (ChosenPivot.Row < Step))
                            ChosenPivot = ChosenPivot.NextInColumn;
                        if (ChosenPivot != null)
                        {
                            // Reduced column has no elements, matrix is singular. 
                            break;
                        }
                        PivotMag = ChosenPivot.Element.Magnitude;
                        if (PivotMag > pivoting.AbsThreshold && PivotMag > pivoting.RelThreshold * FindBiggestInColExclude(matrix, ChosenPivot, Step))
                            return ChosenPivot;
                        else
                        {
                            if (MarkowitzRow[I] == 0)
                            {
                                ChosenPivot = matrix.FirstInRow[I];
                                while ((ChosenPivot != null) && (ChosenPivot.Column < Step))
                                    ChosenPivot = ChosenPivot.NextInRow;
                                if (ChosenPivot != null)
                                {
                                    // Reduced row has no elements, matrix is singular. 
                                    break;
                                }
                                PivotMag = ChosenPivot.Element.Magnitude;
                                if (PivotMag > pivoting.AbsThreshold && PivotMag > pivoting.RelThreshold * FindBiggestInColExclude(matrix, ChosenPivot, Step))
                                    return ChosenPivot;
                            }
                        }
                    }
                    else
                    {
                        ChosenPivot = matrix.FirstInRow[I];
                        while ((ChosenPivot != null) && (ChosenPivot.Column < Step))
                            ChosenPivot = ChosenPivot.NextInRow;
                        if (ChosenPivot != null)
                        {   // Reduced row has no elements, matrix is singular. 
                            break;
                        }
                        PivotMag = ChosenPivot.Element.Magnitude;
                        if (PivotMag > pivoting.AbsThreshold && PivotMag > pivoting.RelThreshold * FindBiggestInColExclude(matrix, ChosenPivot, Step))
                            return ChosenPivot;
                    }
                }
                // Singleton not acceptable (too small), try another. 
            } // end of while(lSingletons>0) 

            // All singletons were unacceptable.  Restore matrix.Singletons count.
            // Initial assumption that an acceptable singleton would be found was wrong.
            Singletons++;
            return null;
        }

        /// <summary>
        /// Quickly search the diagonal
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="Step">The current step</param>
        /// <returns></returns>
        MatrixElement<T> QuicklySearchDiagonal(Matrix<T> matrix, int Step)
        {
            long MinMarkowitzProduct;
            // long pMarkowitzProduct;
            MatrixElement<T> pDiag;
            int I;
            MatrixElement<T> ChosenPivot, pOtherInRow, pOtherInCol;
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
                if ((Magnitude = pDiag.Element.Magnitude) <= AbsThreshold)
                    continue; // Endless for loop 

                if (MarkowitzProd[index] == 1)
                {
                    // Case where only one element exists in row and column other than diagonal. 

                    // Find off-diagonal elements. 
                    pOtherInRow = pDiag.NextInRow;
                    pOtherInCol = pDiag.NextInColumn;
                    if (pOtherInRow == null && pOtherInCol == null)
                    {
                        pOtherInRow = matrix.FirstInRow[I];
                        while (pOtherInRow != null)
                        {
                            if (pOtherInRow.Column >= Step && pOtherInRow.Column != I)
                                break;
                            pOtherInRow = pOtherInRow.NextInRow;
                        }
                        pOtherInCol = matrix.FirstInCol[I];
                        while (pOtherInCol != null)
                        {
                            if (pOtherInCol.Row >= Step && pOtherInCol.Row != I)
                                break;
                            pOtherInCol = pOtherInCol.NextInColumn;
                        }
                    }

                    /* Accept diagonal as pivot if diagonal is larger than off-diagonals and the
                    // off-diagonals are placed symmetricly. */
                    if (pOtherInRow != null && pOtherInCol != null)
                    {
                        if (pOtherInRow.Column == pOtherInCol.Row)
                        {
                            LargestOffDiagonal = Math.Max(pOtherInRow.Element.Magnitude, pOtherInCol.Element.Magnitude);
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
                if (ChosenPivot.Element.Magnitude <= RelThreshold * LargestInCol)
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
        MatrixElement<T> SearchDiagonal(Matrix<T> matrix, int Step)
        {
            int J;
            long MinMarkowitzProduct;
            //, *pMarkowitzProduct;
            int I;
            MatrixElement<T> pDiag;
            int NumberOfTies = 0, Size = matrix.IntSize;
            MatrixElement<T> ChosenPivot;
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
                if ((Magnitude = pDiag.Element.Magnitude) <= AbsThreshold)
                    continue; // for loop 

                // Test to see if diagonal's magnitude is acceptable. 
                LargestInCol = FindBiggestInColExclude(matrix, pDiag, Step);
                if (Magnitude <= RelThreshold * LargestInCol)
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
                    if (NumberOfTies >= MinMarkowitzProduct * TIES_MULTIPLIER)
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
        MatrixElement<T> SearchEntireMatrix(Matrix<T> matrix, int Step)
        {
            int I, Size = matrix.IntSize;
            MatrixElement<T> pElement;
            int NumberOfTies = 0;
            long Product, MinMarkowitzProduct;
            MatrixElement<T> ChosenPivot, pLargestElement = null;
            double Magnitude, LargestElementMag, Ratio, RatioOfAccepted = 0, LargestInCol;

            ChosenPivot = null;
            LargestElementMag = 0.0;
            MinMarkowitzProduct = long.MaxValue;

            // Start search of matrix on column by column basis. 
            for (I = Step; I <= Size; I++)
            {
                pElement = matrix.FirstInCol[I];

                while (pElement != null && pElement.Row < Step)
                    pElement = pElement.NextInColumn;

                if ((LargestInCol = FindLargestInCol(pElement)) == 0.0)
                    continue; // for loop 

                while (pElement != null)
                {
                    /* Check to see if element is the largest encountered so far.  If so, record
                       its magnitude and address. */
                    if ((Magnitude = pElement.Element.Magnitude) > LargestElementMag)
                    {
                        LargestElementMag = Magnitude;
                        pLargestElement = pElement;
                    }
                    // Calculate element's MarkowitzProduct. 
                    Product = MarkowitzRow[pElement.Row] * MarkowitzCol[pElement.Column];

                    // Test to see if element is acceptable as a pivot candidate. 
                    if ((Product <= MinMarkowitzProduct) 
                        && (Magnitude > RelThreshold * LargestInCol) 
                        && (Magnitude > AbsThreshold))
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
                            if (NumberOfTies >= MinMarkowitzProduct * TIES_MULTIPLIER)
                                return ChosenPivot;
                        }
                    }
                    pElement = pElement.NextInColumn;
                }  // End of while(pElement != null) 
            } // End of for(Step) 

            if (ChosenPivot != null) return ChosenPivot;

            if (LargestElementMag.Equals(0)) // Matrix is singular!
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
        /// <param name="element">Element where we need to start searching</param>
        /// <returns></returns>
        public static double FindLargestInCol(MatrixElement<T> element)
        {
            double Magnitude, Largest = 0.0;

            // Search column for largest element beginning at Element. 
            while (element != null)
            {
                if ((Magnitude = element.Element.Magnitude) > Largest)
                    Largest = Magnitude;
                element = element.NextInColumn;
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
        static double FindBiggestInColExclude(Matrix<T> matrix, MatrixElement<T> pElement, int Step)
        {
            int Row;
            int Col;
            double Largest, Magnitude;

            Row = pElement.Row;
            Col = pElement.Column;
            pElement = matrix.FirstInCol[Col];

            // Travel down column until reduced submatrix is entered. 
            while ((pElement != null) && (pElement.Row < Step))
                pElement = pElement.NextInColumn;

            // Initialize the variable Largest. 
            if (pElement.Row != Row)
                Largest = pElement.Element.Magnitude;
            else
                Largest = 0.0;

            // Search rest of column for largest element, avoiding excluded element. 
            while ((pElement = pElement.NextInColumn) != null)
            {
                if ((Magnitude = pElement.Element.Magnitude) > Largest)
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
        /// <param name="pivot">Pivot element</param>
        public void UpdateMarkowitzNumbers(Matrix<T> matrix, MatrixElement<T> pivot)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (pivot == null)
                throw new ArgumentNullException(nameof(pivot));


            int Row, Col;
            MatrixElement<T> ColPtr, RowPtr;
            double Product;

            // Update Markowitz numbers. 
            for (ColPtr = pivot.NextInColumn; ColPtr != null; ColPtr = ColPtr.NextInColumn)
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

            for (RowPtr = pivot.NextInRow; RowPtr != null; RowPtr = RowPtr.NextInRow)
            {
                Col = RowPtr.Column;
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
