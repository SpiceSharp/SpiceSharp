using System;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Methods used to solve matrix equations
    /// </summary>
    public static class SparseSolve
    {
        /// <summary>
        /// Solve the matrix using complex numbers
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="rhs">The right hand side</param>
        /// <param name="solution">The solution</param>
        public static void Solve<T>(this Matrix<T> matrix, Vector<T> rhs, Vector<T> solution)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            MatrixElement<T> pElement;
            Element<T>[] Intermediate;
            int I, Size;
            int[] pExtOrder;
            MatrixElement<T> pPivot;
            Element<T> Temp = ElementFactory.Create<T>();

            Size = matrix.IntSize;
            Intermediate = matrix.Pivoting.Intermediate;

            // Initialize Intermediate vector. 
            pExtOrder = matrix.Translation.IntToExtRowMap;
            for (I = Size; I > 0; I--)
                Intermediate[I].Value = rhs[pExtOrder[I]];

            // Forward substitution. Solves Lc = b.
            for (I = 1; I <= Size; I++)
            {
                Temp.CopyFrom(Intermediate[I]);

                // This step of the substitution is skipped if Temp equals zero. 
                // if ((Temp.Real != 0.0) || (Temp.Imaginary != 0.0))
                if (!Temp.EqualsZero())
                {
                    pPivot = matrix.Diag[I];
                    // Cmplx expr: Temp *= (1.0 / Pivot). 
                    Temp.Multiply(pPivot.Element);
                    Intermediate[I].CopyFrom(Temp);
                    pElement = pPivot.NextInColumn;
                    while (pElement != null)
                    {
                        // Cmplx expr: Intermediate[Element.Row] -= Temp * *Element
                        Intermediate[pElement.Row].SubtractMultiply(Temp, pElement.Element);
                        pElement = pElement.NextInColumn;
                    }
                }
            }

            // Backward Substitution. Solves Ux = c.
            for (I = Size; I > 0; I--)
            {
                Temp.CopyFrom(Intermediate[I]);
                pElement = matrix.Diag[I].NextInRow;

                while (pElement != null)
                {
                    // Cmplx expr: Temp -= *Element * Intermediate[Element.Col]
                    Temp.SubtractMultiply(pElement.Element, Intermediate[pElement.Column]);
                    pElement = pElement.NextInRow;
                }
                Intermediate[I].CopyFrom(Temp);
            }

            // Unscramble Intermediate vector while placing data in to Solution vector.
            pExtOrder = matrix.Translation.IntToExtColMap;
            for (I = Size; I > 0; I--)
                solution[pExtOrder[I]] = Intermediate[I].Value;

            return;
        }

        /// <summary>
        /// Solve transposed matrix using complex numbers
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="rhs">The right hand side</param>
        /// <param name="solution">The solution</param>
        public static void SolveTransposed<T>(this Matrix<T> matrix, Vector<T> rhs, Vector<T> solution)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            MatrixElement<T> pElement;
            Element<T>[] Intermediate;
            int I, Size;
            int[] pExtOrder;
            MatrixElement<T> pPivot;
            Element<T> Temp = ElementFactory.Create<T>();

            // Begin `SolveComplexTransposedMatrix'. 

            Size = matrix.IntSize;
            Intermediate = matrix.Pivoting.Intermediate;

            // Initialize Intermediate vector. 
            pExtOrder = matrix.Translation.IntToExtColMap;
            for (I = Size; I > 0; I--)
                Intermediate[I].Value = rhs[pExtOrder[I]];

            // Forward elimination. 
            for (I = 1; I <= Size; I++)
            {
                Temp.CopyFrom(Intermediate[I]);

                // This step of the elimination is skipped if Temp equals zero. 
                // if ((Temp.Real != 0.0) || (Temp.Imaginary != 0.0))
                if (!Temp.EqualsZero())
                {
                    pElement = matrix.Diag[I].NextInRow;
                    while (pElement != null)
                    {
                        // Cmplx expr: Intermediate[Element.Col] -= Temp * *Element
                        Intermediate[pElement.Column].SubtractMultiply(Temp, pElement.Element);
                        pElement = pElement.NextInRow;
                    }
                }
            }

            // Backward Substitution. 
            for (I = Size; I > 0; I--)
            {
                pPivot = matrix.Diag[I];
                Temp.CopyFrom(Intermediate[I]);
                pElement = pPivot.NextInColumn;

                while (pElement != null)
                {
                    // Cmplx expr: Temp -= Intermediate[Element.Row] * *Element
                    Temp.SubtractMultiply(Intermediate[pElement.Row], pElement.Element);

                    pElement = pElement.NextInColumn;
                }
                // Cmplx expr: Intermediate = Temp * (1.0 / *pPivot).
                Intermediate[I].AssignMultiply(Temp, pPivot.Element);
            }

            // Unscramble Intermediate vector while placing data in to Solution vector. 
            pExtOrder = matrix.Translation.IntToExtRowMap;
            for (I = Size; I > 0; I--)
                solution[pExtOrder[I]] = Intermediate[I].Value;

            return;
        }
    }
}
