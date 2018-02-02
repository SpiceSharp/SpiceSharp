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
        public static void PreorderModifiedNodalAnalysis<T>(this Matrix<T> matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            int J, Size;
            MatrixElement<T> pTwin1 = null, pTwin2 = null;
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
                            CountTwins(matrix, J, ref pTwin1, ref pTwin2);
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
        static int CountTwins<T>(Matrix<T> matrix, int Col, ref MatrixElement<T> ppTwin1, ref MatrixElement<T> ppTwin2)
        {
            int Row, Twins = 0;
            MatrixElement<T> pTwin1, pTwin2;

            // Begin `CountTwins'. 

            pTwin1 = matrix.FirstInCol[Col];
            while (pTwin1 != null)
            {
                // if (Math.Abs(pTwin1.Element.Magnitude) == 1.0)
                if (pTwin1.Element.EqualsOne())
                {
                    Row = pTwin1.Row;
                    pTwin2 = matrix.FirstInCol[Row];
                    while ((pTwin2 != null) && (pTwin2.Row != Col))
                        pTwin2 = pTwin2.NextInColumn;
                    if ((pTwin2 != null) && pTwin2.Element.EqualsOne()) // (Math.Abs(pTwin2.Element.Magnitude) == 1.0))
                    {
                        // Found symmetric twins. 
                        if (++Twins >= 2) return Twins;
                        (ppTwin1 = pTwin1).Column = Col;
                        (ppTwin2 = pTwin2).Column = Row;
                    }
                }
                pTwin1 = pTwin1.NextInColumn;
            }
            return Twins;
        }

        /// <summary>
        /// Swap columns
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="pTwin1"></param>
        /// <param name="pTwin2"></param>
        static void SwapCols<T>(Matrix<T> matrix, MatrixElement<T> pTwin1, MatrixElement<T> pTwin2)
        {
            int Col1 = pTwin1.Column, Col2 = pTwin2.Column;

            SparseDefinitions.Swap(ref matrix.FirstInCol[Col1], ref matrix.FirstInCol[Col2]);
            SparseDefinitions.Swap(ref matrix.Translation.IntToExtColMap[Col1], ref matrix.Translation.IntToExtColMap[Col2]);
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
        /// <param name="exponent">The logarithm base 10 for the determinant</param>
        /// <param name="determinant">The determinant. It is scaled between 1 and 10.</param>
        public static void Determinant<T>(this Matrix<T> matrix, out int exponent, out Element<T> determinant)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            int I, Size;
            double Norm;
            Element<T> Pivot = ElementFactory.Create<T>();
            determinant = ElementFactory.Create<T>();
            exponent = 0;

            // Begin `spDeterminant'. 
            if (!matrix.Factored)
                throw new SparseException("Matrix is not factored");

            exponent = 0;

            if (matrix.Error == SparseError.Singular)
            {
                determinant.Clear();
                return;
            }

            Size = matrix.IntSize;
            I = 0;

            determinant.Value = determinant.One;

            while (++I <= Size)
            {
                Pivot.AssignReciprocal(matrix.Diag[I].Element);
                determinant.Multiply(Pivot);

                // Scale Determinant.
                Norm = determinant.Magnitude;
                if (!Norm.Equals(0))
                {
                    while (Norm >= 1.0e12)
                    {
                        determinant.Scalar(1e-12);
                        exponent += 12;
                        Norm = determinant.Magnitude;
                    }
                    while (Norm < 1.0e-12)
                    {
                        determinant.Scalar(1e12);
                        exponent -= 12;
                        Norm = determinant.Magnitude;
                    }
                }
            }

            // Scale Determinant again, this time to be between 1.0 <= x < 10.0. 
            Norm = determinant.Magnitude;
            if (!Norm.Equals(0))
            {
                while (Norm >= 10.0)
                {
                    determinant.Scalar(0.1);
                    exponent++;
                    Norm = determinant.Magnitude;
                }
                while (Norm < 1.0)
                {
                    determinant.Scalar(10.0);
                    exponent--;
                    Norm = determinant.Magnitude;
                }
            }
            if (matrix.NumberOfInterchangesIsOdd)
                determinant.Negate();
        }

        /// <summary>
        /// Get an error message
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="originator">Name of the source</param>
        /// <returns></returns>
        public static string ErrorMessage<T>(this Matrix<T> matrix, string originator)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            int Row = 0, Col = 0;
            SparseError Error;
            StringBuilder sb = new StringBuilder();

            Error = matrix.Error;
            if (Error == SparseError.Okay)
                return null;
            if (originator == null)
                originator = "sparse";
            if (originator != null)
                sb.Append("{0}: ".FormatString(originator));
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
                sb.Append("singular matrix detected at row {0} and column {1}.".FormatString(Row, Col));
            }
            else if (Error == SparseError.ZeroDiagonal)
            {
                matrix.SingularAt(out Row, out Col);
                sb.Append("zero diagonal detected at row {0} and column {1}.".FormatString(Row, Col));
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
