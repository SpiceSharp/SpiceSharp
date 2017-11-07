namespace SpiceSharp.Sparse
{
    public static class SparseSolve
    {
        /// <summary>
        /// spSolve
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="RHS"></param>
        /// <param name="Solution"></param>
        /// <param name="iRHS"></param>
        /// <param name="iSolution"></param>
        public static void spSolve(Matrix matrix, double[] RHS, double[] Solution, double[] iRHS, double[] iSolution)
        {
            MatrixElement pElement;
            ElementValue[] Intermediate;
            double Temp;
            int I, Size;
            int[] pExtOrder;
            MatrixElement pPivot;

            // Begin `spSolve'. 
            if (!(matrix.Factored && !matrix.NeedsOrdering))
                throw new SparseException("Matrix is not refactored or needs ordering");

            if (matrix.Complex)
            {
                SolveComplexMatrix(matrix, RHS, Solution, iRHS, iSolution);
                return;
            }

            Intermediate = matrix.Intermediate;
            Size = matrix.Size;

            // Initialize Intermediate vector. 
            pExtOrder = matrix.IntToExtRowMap;
            for (I = Size; I > 0; I--)
                Intermediate[I].Real = RHS[pExtOrder[I]];

            // Forward elimination. Solves Lc = b.
            for (I = 1; I <= Size; I++)
            {
                // This step of the elimination is skipped if Temp equals zero. 
                if ((Temp = Intermediate[I].Real) != 0.0)
                {
                    pPivot = matrix.Diag[I];
                    Intermediate[I].Real = (Temp *= pPivot.Value.Real);

                    pElement = pPivot.NextInCol;
                    while (pElement != null)
                    {
                        Intermediate[pElement.Row].Real -= Temp * pElement.Value.Real;
                        pElement = pElement.NextInCol;
                    }
                }
            }

            // Backward Substitution. Solves Ux = c.
            for (I = Size; I > 0; I--)
            {
                Temp = Intermediate[I];
                pElement = matrix.Diag[I].NextInRow;
                while (pElement != null)
                {
                    Temp -= pElement.Value.Real * Intermediate[pElement.Col];
                    pElement = pElement.NextInRow;
                }
                Intermediate[I].Real = Temp;
            }

            // Unscramble Intermediate vector while placing data in to Solution vector. 
            pExtOrder = matrix.IntToExtColMap;
            for (I = Size; I > 0; I--)
                Solution[pExtOrder[I]] = Intermediate[I];

            return;
        }

        /// <summary>
        /// SolveComplexMatrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="RHS"></param>
        /// <param name="Solution"></param>
        /// <param name="iRHS"></param>
        /// <param name="iSolution"></param>
        private static void SolveComplexMatrix(Matrix matrix, double[] RHS, double[] Solution, double[] iRHS, double[] iSolution)
        {
            MatrixElement pElement;
            ElementValue[] Intermediate;
            int I, Size;
            int[] pExtOrder;
            MatrixElement pPivot;
            ElementValue Temp;

            Size = matrix.Size;
            Intermediate = matrix.Intermediate;

            // Initialize Intermediate vector. 
            pExtOrder = matrix.IntToExtRowMap;
            for (I = Size; I > 0; I--)
            {
                Intermediate[I].Real = RHS[pExtOrder[I]];
                Intermediate[I].Imag = iRHS[pExtOrder[I]];
            }

            // Forward substitution. Solves Lc = b.
            for (I = 1; I <= Size; I++)
            {
                Temp = Intermediate[I];

                // This step of the substitution is skipped if Temp equals zero. 
                if ((Temp.Real != 0.0) || (Temp.Imag != 0.0))
                {
                    pPivot = matrix.Diag[I];
                    // Cmplx expr: Temp *= (1.0 / Pivot). 
                    SparseDefinitions.CMPLX_MULT_ASSIGN(ref Temp, pPivot);
                    Intermediate[I] = Temp;
                    pElement = pPivot.NextInCol;
                    while (pElement != null)
                    {
                        // Cmplx expr: Intermediate[Element.Row] -= Temp * *Element. 
                        SparseDefinitions.CMPLX_MULT_SUBT_ASSIGN(ref Intermediate[pElement.Row], Temp, pElement);
                        pElement = pElement.NextInCol;
                    }
                }
            }

            // Backward Substitution. Solves Ux = c.
            for (I = Size; I > 0; I--)
            {
                Temp = Intermediate[I];
                pElement = matrix.Diag[I].NextInRow;

                while (pElement != null)
                {
                    // Cmplx expr: Temp -= *Element * Intermediate[Element.Col]. 
                    SparseDefinitions.CMPLX_MULT_SUBT_ASSIGN(ref Temp, pElement, Intermediate[pElement.Col]);
                    pElement = pElement.NextInRow;
                }
                Intermediate[I] = Temp;
            }

            // Unscramble Intermediate vector while placing data in to Solution vector.
            pExtOrder = matrix.IntToExtColMap;
            for (I = Size; I > 0; I--)
            {
                Solution[pExtOrder[I]] = Intermediate[I].Real;
                iSolution[pExtOrder[I]] = Intermediate[I].Imag;
            }

            return;
        }

        /// <summary>
        /// spSolveTransposed
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="RHS"></param>
        /// <param name="Solution"></param>
        /// <param name="iRHS"></param>
        /// <param name="iSolution"></param>
        public static void spSolveTransposed(Matrix matrix, double[] RHS, double[] Solution, double[] iRHS, double[] iSolution)
        {
            MatrixElement pElement;
            ElementValue[] Intermediate;
            int I, Size;
            int[] pExtOrder;
            MatrixElement pPivot;
            double Temp;

            if (!matrix.Factored)
                throw new SparseException("Matrix is not factored");

            if (matrix.Complex)
            {
                SolveComplexTransposedMatrix(matrix, RHS, Solution, iRHS, iSolution);
                return;
            }

            Size = matrix.Size;
            Intermediate = matrix.Intermediate;

            // Initialize Intermediate vector. 
            pExtOrder = matrix.IntToExtColMap;
            for (I = Size; I > 0; I--)
                Intermediate[I].Real = RHS[pExtOrder[I]];

            // Forward elimination. 
            for (I = 1; I <= Size; I++)
            {
                // This step of the elimination is skipped if Temp equals zero. 
                if ((Temp = Intermediate[I]) != 0.0)
                {
                    pElement = matrix.Diag[I].NextInRow;
                    while (pElement != null)
                    {
                        Intermediate[pElement.Col].Real -= Temp * pElement.Value.Real;
                        pElement = pElement.NextInRow;
                    }

                }
            }

            // Backward Substitution. 
            for (I = Size; I > 0; I--)
            {
                pPivot = matrix.Diag[I];
                Temp = Intermediate[I];
                pElement = pPivot.NextInCol;
                while (pElement != null)
                {
                    Temp -= pElement.Value.Real * Intermediate[pElement.Row];
                    pElement = pElement.NextInCol;
                }
                Intermediate[I].Real = Temp * pPivot.Value.Real;
            }

            // Unscramble Intermediate vector while placing data in to Solution vector. 
            pExtOrder = matrix.IntToExtRowMap;
            for (I = Size; I > 0; I--)
                Solution[pExtOrder[I]] = Intermediate[I];

            return;
        }

        /// <summary>
        /// SolveComplexTransposedMatrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="RHS"></param>
        /// <param name="Solution"></param>
        /// <param name="iRHS"></param>
        /// <param name="iSolution"></param>
        public static void SolveComplexTransposedMatrix(Matrix matrix, double[] RHS, double[] Solution, double[] iRHS, double[] iSolution)
        {
            MatrixElement pElement;
            ElementValue[] Intermediate;
            int I, Size;
            int[] pExtOrder;
            MatrixElement pPivot;
            ElementValue Temp;

            // Begin `SolveComplexTransposedMatrix'. 

            Size = matrix.Size;
            Intermediate = matrix.Intermediate;

            // Initialize Intermediate vector. 
            pExtOrder = matrix.IntToExtColMap;
            for (I = Size; I > 0; I--)
            {
                Intermediate[I].Real = RHS[pExtOrder[I]];
                Intermediate[I].Imag = iRHS[pExtOrder[I]];
            }

            // Forward elimination. 
            for (I = 1; I <= Size; I++)
            {
                Temp = Intermediate[I];

                // This step of the elimination is skipped if Temp equals zero. 
                if ((Temp.Real != 0.0) || (Temp.Imag != 0.0))
                {
                    pElement = matrix.Diag[I].NextInRow;
                    while (pElement != null)
                    {
                        // Cmplx expr: Intermediate[Element.Col] -= Temp * *Element. 
                        SparseDefinitions.CMPLX_MULT_SUBT_ASSIGN(ref Intermediate[pElement.Col], Temp, pElement);
                        pElement = pElement.NextInRow;
                    }
                }
            }

            // Backward Substitution. 
            for (I = Size; I > 0; I--)
            {
                pPivot = matrix.Diag[I];
                Temp = Intermediate[I];
                pElement = pPivot.NextInCol;

                while (pElement != null)
                {
                    // Cmplx expr: Temp -= Intermediate[Element.Row] * *Element. 
                    SparseDefinitions.CMPLX_MULT_SUBT_ASSIGN(ref Temp, Intermediate[pElement.Row], pElement);

                    pElement = pElement.NextInCol;
                }
                // Cmplx expr: Intermediate = Temp * (1.0 / *pPivot). 
                SparseDefinitions.CMPLX_MULT(ref Intermediate[I], Temp, pPivot);
            }

            // Unscramble Intermediate vector while placing data in to Solution vector. 
            pExtOrder = matrix.IntToExtRowMap;
            for (I = Size; I > 0; I--)
            {
                Solution[pExtOrder[I]] = Intermediate[I].Real;
                iSolution[pExtOrder[I]] = Intermediate[I].Imag;
            }

            return;
        }
    }
}
