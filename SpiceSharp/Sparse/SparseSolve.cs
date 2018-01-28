using System;
using System.Numerics;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Methods used to solve matrix equations
    /// </summary>
    public static class SparseSolve
    {
        /// <summary>
        /// Solve the matrix
        /// This method can solve in-place
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="rhs">The right hand side</param>
        /// <param name="solution">The solution</param>
        public static void Solve(this Matrix matrix, Vector<double> rhs, Vector<double> solution)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            MatrixElement pElement;
            ElementValue[] Intermediate;
            double Temp;
            int I, Size;
            int[] pExtOrder;
            MatrixElement pPivot;

            // Begin `spSolve'. 
            if (!(matrix.Factored && !matrix.NeedsOrdering))
                throw new SparseException("Matrix is not refactored or needs ordering");

            /* if (matrix.Complex)
            {
                SolveComplexMatrix(matrix, RHS, Solution, iRHS, iSolution);
                return;
            } */

            Intermediate = matrix.Pivoting.Intermediate;
            Size = matrix.IntSize;

            // Initialize Intermediate vector. 
            pExtOrder = matrix.Translation.IntToExtRowMap;
            for (I = Size; I > 0; I--)
                Intermediate[I].Real = rhs[pExtOrder[I]];

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
            pExtOrder = matrix.Translation.IntToExtColMap;
            for (I = Size; I > 0; I--)
                solution[pExtOrder[I]] = Intermediate[I];

            return;
        }

        /// <summary>
        /// Solve the matrix using complex numbers
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="rhs">The right hand side</param>
        /// <param name="solution">The solution</param>
        public static void Solve(this Matrix matrix, Vector<Complex> rhs, Vector<Complex> solution)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            MatrixElement pElement;
            ElementValue[] Intermediate;
            int I, Size;
            int[] pExtOrder;
            MatrixElement pPivot;
            ElementValue Temp;

            Size = matrix.IntSize;
            Intermediate = matrix.Pivoting.Intermediate;

            // Initialize Intermediate vector. 
            pExtOrder = matrix.Translation.IntToExtRowMap;
            for (I = Size; I > 0; I--)
                Intermediate[I].Cplx = rhs[pExtOrder[I]];

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
            pExtOrder = matrix.Translation.IntToExtColMap;
            for (I = Size; I > 0; I--)
                solution[pExtOrder[I]] = Intermediate[I].Cplx;

            return;
        }

        /// <summary>
        /// spSolveTransposed
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="rhs">The right hand side</param>
        /// <param name="solution">The solution</param>
        public static void SolveTransposed(this Matrix matrix, Vector<double> rhs, Vector<double> solution)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            MatrixElement pElement;
            ElementValue[] Intermediate;
            int I, Size;
            int[] pExtOrder;
            MatrixElement pPivot;
            double Temp;

            if (!matrix.Factored)
                throw new SparseException("Matrix is not factored");

            /* if (matrix.Complex)
            {
                SolveComplexTransposedMatrix(matrix, RHS, Solution, iRHS, iSolution);
                return;
            } */

            Size = matrix.IntSize;
            Intermediate = matrix.Pivoting.Intermediate;

            // Initialize Intermediate vector. 
            pExtOrder = matrix.Translation.IntToExtColMap;
            for (I = Size; I > 0; I--)
                Intermediate[I].Real = rhs[pExtOrder[I]];

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
            pExtOrder = matrix.Translation.IntToExtRowMap;
            for (I = Size; I > 0; I--)
                solution[pExtOrder[I]] = Intermediate[I];

            return;
        }

        /// <summary>
        /// Solve transposed matrix using complex numbers
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="rhs">The right hand side</param>
        /// <param name="solution">The solution</param>
        public static void SolveTransposed(this Matrix matrix, Vector<Complex> rhs, Vector<Complex> solution)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            MatrixElement pElement;
            ElementValue[] Intermediate;
            int I, Size;
            int[] pExtOrder;
            MatrixElement pPivot;
            ElementValue Temp;

            // Begin `SolveComplexTransposedMatrix'. 

            Size = matrix.IntSize;
            Intermediate = matrix.Pivoting.Intermediate;

            // Initialize Intermediate vector. 
            pExtOrder = matrix.Translation.IntToExtColMap;
            for (I = Size; I > 0; I--)
                Intermediate[I].Cplx = rhs[pExtOrder[I]];

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
            pExtOrder = matrix.Translation.IntToExtRowMap;
            for (I = Size; I > 0; I--)
                solution[pExtOrder[I]] = Intermediate[I].Cplx;

            return;
        }
    }
}
