using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Utility functions for sparse matrices
    /// </summary>
    public static class SparseUtilities
    {
        /// <summary>
        /// Preordering for Modified Nodal Analysis
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public static void PreorderMNA(this Matrix matrix)
        {
            int J, Size;
            MatrixElement pTwin1 = null, pTwin2 = null;
            int Twins, StartAt = 1;
            bool Swapped, AnotherPassNeeded;

            // Begin `spMNA_Preorder'. 
            if (matrix.Factored)
                throw new SparseException("Matrix is factored");

            if (matrix.RowsLinked)
                return;
            Size = matrix.IntSize;
            matrix.Reordered = true;

            do
            {
                AnotherPassNeeded = Swapped = false;

                // Search for zero diagonals with lone twins. 
                for (J = StartAt; J <= Size; J++)
                {
                    if (matrix.Diag[J] == null)
                    {
                        Twins = CountTwins(matrix, J, ref pTwin1, ref pTwin2);
                        if (Twins == 1)
                        {
                            // Lone twins found, swap rows. 
                            SwapCols(matrix, pTwin1, pTwin2);
                            Swapped = true;
                        }
                        else if ((Twins > 1) && !AnotherPassNeeded)
                        {
                            AnotherPassNeeded = true;
                            StartAt = J;
                        }
                    }
                }

                // All lone twins are gone, look for zero diagonals with multiple twins. 
                if (AnotherPassNeeded)
                {
                    for (J = StartAt; !Swapped && (J <= Size); J++)
                    {
                        if (matrix.Diag[J] == null)
                        {
                            Twins = CountTwins(matrix, J, ref pTwin1, ref pTwin2);
                            SwapCols(matrix, pTwin1, pTwin2);
                            Swapped = true;
                        }
                    }
                }
            } while (AnotherPassNeeded);
            return;
        }

        /// <summary>
        /// Cound the number of twins
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="Col"></param>
        /// <param name="ppTwin1"></param>
        /// <param name="ppTwin2"></param>
        /// <returns></returns>
        private static int CountTwins(Matrix matrix, int Col, ref MatrixElement ppTwin1, ref MatrixElement ppTwin2)
        {
            int Row, Twins = 0;
            MatrixElement pTwin1, pTwin2;

            // Begin `CountTwins'. 

            pTwin1 = matrix.FirstInCol[Col];
            while (pTwin1 != null)
            {
                if (Math.Abs(pTwin1.Value.Real) == 1.0)
                {
                    Row = pTwin1.Row;
                    pTwin2 = matrix.FirstInCol[Row];
                    while ((pTwin2 != null) && (pTwin2.Row != Col))
                        pTwin2 = pTwin2.NextInCol;
                    if ((pTwin2 != null) && (Math.Abs(pTwin2.Value.Real) == 1.0))
                    {
                        // Found symmetric twins. 
                        if (++Twins >= 2) return Twins;
                        (ppTwin1 = pTwin1).Col = Col;
                        (ppTwin2 = pTwin2).Col = Row;
                    }
                }
                pTwin1 = pTwin1.NextInCol;
            }
            return Twins;
        }

        /// <summary>
        /// Swap columns
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="pTwin1"></param>
        /// <param name="pTwin2"></param>
        private static void SwapCols(Matrix matrix, MatrixElement pTwin1, MatrixElement pTwin2)
        {
            int Col1 = pTwin1.Col, Col2 = pTwin2.Col;


            SparseDefinitions.SWAP(ref matrix.FirstInCol[Col1], ref matrix.FirstInCol[Col2]);
            SparseDefinitions.SWAP(ref matrix.Translation.IntToExtColMap[Col1], ref matrix.Translation.IntToExtColMap[Col2]);
            matrix.Translation.ExtToIntColMap[matrix.Translation.IntToExtColMap[Col2]] = Col2;
            matrix.Translation.ExtToIntColMap[matrix.Translation.IntToExtColMap[Col1]] = Col1;

            matrix.Diag[Col1] = pTwin2;
            matrix.Diag[Col2] = pTwin1;
            matrix.NumberOfInterchangesIsOdd = !matrix.NumberOfInterchangesIsOdd;
            return;
        }

        /// <summary>
        /// Calculate the determinant of a matrix
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="pExponent">The logarithm base 10 for the determinant</param>
        /// <param name="pDeterminant">The real portion of the determinant. It is scaled between 1 and 10.</param>
        /// <param name="piDeterminant">The imaginary portion of the determinant. It is scaled between 1 and 10.</param>
        public static void Determinant(this Matrix matrix, out int pExponent, out double pDeterminant, out double piDeterminant)
        {
            int I, Size;
            double Norm, nr, ni;
            ElementValue Pivot = new ElementValue(), cDeterminant = new ElementValue();
            piDeterminant = 0.0;
            pDeterminant = 0.0;
            pExponent = 0;

            // Begin `spDeterminant'. 
            if (!matrix.Factored)
                throw new SparseException("Matrix is not factored");

            pExponent = 0;

            if (matrix.Error == SparseError.Singular)
            {
                pDeterminant = 0.0;
                piDeterminant = 0.0;
                return;
            }

            Size = matrix.IntSize;
            I = 0;

            if (matrix.Complex)        // Complex Case. 
            {
                cDeterminant.Real = 1.0;
                cDeterminant.Imag = 0.0;

                while (++I <= Size)
                {
                    SparseDefinitions.CMPLX_RECIPROCAL(ref Pivot, matrix.Diag[I]);
                    SparseDefinitions.CMPLX_MULT_ASSIGN(ref cDeterminant, Pivot);

                    // Scale Determinant.
                    nr = Math.Abs(cDeterminant.Real);
                    ni = Math.Abs(cDeterminant.Imag);
                    Norm = Math.Max(nr, ni);
                    if (Norm != 0.0)
                    {
                        while (Norm >= 1.0e12)
                        {
                            cDeterminant.Real *= 1.0e-12;
                            cDeterminant.Imag *= 1.0e-12;
                            pExponent += 12;
                            nr = Math.Abs(cDeterminant.Real);
                            ni = Math.Abs(cDeterminant.Imag);
                            Norm = Math.Max(nr, ni);
                        }
                        while (Norm < 1.0e-12)
                        {
                            cDeterminant.Real *= 1.0e12;
                            cDeterminant.Imag *= 1.0e12;
                            pExponent -= 12;
                            nr = Math.Abs(cDeterminant.Real);
                            ni = Math.Abs(cDeterminant.Imag);
                            Norm = Math.Max(nr, ni);
                        }
                    }
                }

                // Scale Determinant again, this time to be between 1.0 <= x < 10.0. 
                nr = Math.Abs(cDeterminant.Real);
                ni = Math.Abs(cDeterminant.Imag);
                Norm = Math.Max(nr, ni);
                if (Norm != 0.0)
                {
                    while (Norm >= 10.0)
                    {
                        cDeterminant.Real *= 0.1;
                        cDeterminant.Imag *= 0.1;
                        pExponent++;
                        nr = Math.Abs(cDeterminant.Real);
                        ni = Math.Abs(cDeterminant.Imag);
                        Norm = Math.Max(nr, ni);
                    }
                    while (Norm < 1.0)
                    {
                        cDeterminant.Real *= 10.0;
                        cDeterminant.Imag *= 10.0;
                        pExponent--;
                        nr = Math.Abs(cDeterminant.Real);
                        ni = Math.Abs(cDeterminant.Imag);
                        Norm = Math.Max(nr, ni);
                    }
                }
                if (matrix.NumberOfInterchangesIsOdd)
                    SparseDefinitions.CMPLX_NEGATE(ref cDeterminant);

                pDeterminant = cDeterminant.Real;
                piDeterminant = cDeterminant.Imag;
            }
            else
            {
                // Real Case. 
                pDeterminant = 1.0;

                while (++I <= Size)
                {
                    pDeterminant /= matrix.Diag[I].Value.Real;

                    // Scale Determinant. 
                    if (pDeterminant != 0.0)
                    {
                        while (Math.Abs(pDeterminant) >= 1.0e12)
                        {
                            pDeterminant *= 1.0e-12;
                            pExponent += 12;
                        }
                        while (Math.Abs(pDeterminant) < 1.0e-12)
                        {
                            pDeterminant *= 1.0e12;
                            pExponent -= 12;
                        }
                    }
                }

                // Scale Determinant again, this time to be between 1.0 <= x < 10.0. 
                if (pDeterminant != 0.0)
                {
                    while (Math.Abs(pDeterminant) >= 10.0)
                    {
                        pDeterminant *= 0.1;
                        pExponent++;
                    }
                    while (Math.Abs(pDeterminant) < 1.0)
                    {
                        pDeterminant *= 10.0;
                        pExponent--;
                    }
                }
                if (matrix.NumberOfInterchangesIsOdd)
                    pDeterminant = -pDeterminant;
            }
        }

        /// <summary>
        /// Get an error message
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="Originator">Name of the source</param>
        /// <returns></returns>
        public static string ErrorMessage(this Matrix matrix, string Originator)
        {
            int Row = 0, Col = 0;
            SparseError Error;
            StringBuilder sb = new StringBuilder();

            Error = matrix.Error;
            if (Error == SparseError.Okay)
                return null;
            if (Originator == null)
                Originator = "sparse";
            if (Originator != null)
                sb.Append($"{Originator.ToString()}: ");
            if ((int)Error >= (int)SparseError.Fatal)
                sb.Append("fatal error, ");
            else
                sb.Append("warning, ");
            
            // Print particular error message.
            // Do not use switch statement because error codes may not be unique.
            if (Error == SparseError.Panic)
                sb.Append("Sparse called improperly.");
            else if (Error == SparseError.Singular)
            {
                matrix.SingularAt(out Row, out Col);
                sb.Append($"singular matrix detected at row {Row} and column {Col}.");
            }
            else if (Error == SparseError.ZeroDiagonal)
            {
                matrix.SingularAt(out Row, out Col);
                sb.Append($"zero diagonal detected at row {Row} and column {Col}.");
            }
            else if (Error == SparseError.SmallPivot)
            {
                sb.Append("unable to find a pivot that is larger than absolute threshold.");
            }
            else
                throw new SparseException("Unrecognized error");
            return sb.ToString();
        }
    }
}
