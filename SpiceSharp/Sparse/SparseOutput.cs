using System;
using System.Numerics;
using System.Text;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Methods used to output matrix data
    /// </summary>
    public static class SparseOutput
    {
        /// <summary>
        /// Maximum width for strings
        /// </summary>
        public static int PrinterWidth { get; set; } = 80;

        /// <summary>
        /// Nicely print the matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static string Print<T>(this Matrix<T> matrix)
        {
            return Print(matrix, false, true, true);
        }

        /// <summary>
        /// Nicely print the matrix
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="printReordered">True if the internal order is shown</param>
        /// <param name="data">True if the actual values should be shown</param>
        /// <param name="header">True if the header is shown</param>
        /// <returns></returns>
        public static string Print<T>(this Matrix<T> matrix, bool printReordered, bool data, bool header)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            int J = 0;
            int I, Row, Col, Size, Top, StartCol = 1, StopCol, Columns, ElementCount = 0;
            double Magnitude, SmallestDiag, SmallestElement;
            double LargestElement = 0.0, LargestDiag = 0.0;
            MatrixElement<T> pElement;
            MatrixElement<T>[] pImagElements;
            int[] PrintOrdToIntRowMap, PrintOrdToIntColMap;

            StringBuilder sb = new StringBuilder();
            Size = matrix.IntSize;
            pImagElements = new MatrixElement<T>[PrinterWidth / 10 + 1];

            // Create a packed external to internal row and column translation array
            Top = matrix.AllocatedExtSize;

            PrintOrdToIntRowMap = new int[Top + 1];
            PrintOrdToIntColMap = new int[Top + 1];
            for (I = 1; I <= Size; I++)
            {
                PrintOrdToIntRowMap[matrix.Translation.IntToExtRowMap[I]] = I;
                PrintOrdToIntColMap[matrix.Translation.IntToExtColMap[I]] = I;
            }

            // Pack the arrays
            for (J = 1, I = 1; I <= Top; I++)
            {
                if (PrintOrdToIntRowMap[I] != 0)
                    PrintOrdToIntRowMap[J++] = PrintOrdToIntRowMap[I];
            }
            for (J = 1, I = 1; I <= Top; I++)
            {
                if (PrintOrdToIntColMap[I] != 0)
                    PrintOrdToIntColMap[J++] = PrintOrdToIntColMap[I];
            }

            // Print header
            if (header)
            {
                sb.Append("MATRIX SUMMARY" + Environment.NewLine + Environment.NewLine);
                sb.Append("Size of matrix = {0} x {0}.{1}".FormatString(Size, Environment.NewLine));
                if (matrix.Reordered && printReordered)
                    sb.Append("Matrix has been reordered." + Environment.NewLine);
                sb.Append(Environment.NewLine);

                if (matrix.Factored)
                    sb.Append("Matrix after factorization:" + Environment.NewLine);
                else
                    sb.Append("Matrix before factorization:" + Environment.NewLine);
            }
            SmallestElement = double.MaxValue;
            SmallestDiag = SmallestElement;

            // Determine how many columns to use
            Columns = PrinterWidth;
            if (header) Columns -= 5;
            if (data) Columns = (Columns + 1) / 10;

            // Print matrix by printing groups of complete columns until all the columns
            // are printed.
            J = 0;
            while (J <= Size)
            {
                // Calculate index of last column to printed in this group
                StopCol = StartCol + Columns - 1;
                if (StopCol > Size)
                    StopCol = Size;

                // Label the columns
                if (header)
                {
                    if (data)
                    {
                        sb.Append("    ");
                        for (I = StartCol; I <= StopCol; I++)
                        {
                            if (printReordered)
                                Col = I;
                            else
                                Col = PrintOrdToIntColMap[I];
                            sb.AppendFormat(" {0,9}", matrix.Translation.IntToExtColMap[Col]);
                        }
                        sb.Append(Environment.NewLine + Environment.NewLine);
                    }
                    else
                    {
                        if (printReordered)
                            sb.Append("Columns {0} to {1}.{2}".FormatString(StartCol, StopCol, Environment.NewLine));
                        else
                            sb.Append("Columns {0} to {1}.{2}".FormatString(
                                matrix.Translation.IntToExtColMap[PrintOrdToIntColMap[StartCol]],
                                matrix.Translation.IntToExtColMap[PrintOrdToIntColMap[StopCol]],
                                Environment.NewLine));
                    }
                }

                // Print every row ...
                for (I = 1; I <= Size; I++)
                {
                    if (printReordered)
                        Row = I;
                    else
                        Row = PrintOrdToIntRowMap[I];

                    if (header)
                    {
                        if (printReordered && !data)
                            sb.AppendFormat("{0,4}", I);
                        else
                            sb.AppendFormat("{0,4}", matrix.Translation.IntToExtRowMap[Row]);
                        if (!data)
                            sb.Append(' ');
                    }

                    // ... in each column of the group
                    for (J = StartCol; J <= StopCol; J++)
                    {
                        if (printReordered)
                            Col = J;
                        else
                            Col = PrintOrdToIntColMap[J];

                        pElement = matrix.FirstInCol[Col];
                        while (pElement != null && pElement.Row != Row)
                            pElement = pElement.NextInColumn;

                        if (data)
                            pImagElements[J - StartCol] = pElement;

                        if (pElement != null)
                        {

                            // Case where element exists
                            if (data)
                            {
                                if (typeof(T) == typeof(double))
                                    sb.AppendFormat("{0:G3,10}", pElement.Element.Value);
                            }
                            else
                                sb.Append('x');

                            // Update status variables
                            if ((Magnitude = pElement.Element.Magnitude) > LargestElement)
                                LargestElement = Magnitude;
                            if ((Magnitude < SmallestElement) && (Magnitude != 0.0))
                                SmallestElement = Magnitude;
                            ElementCount++;
                        }
                        else
                        {
                            // Case where element is structurally zero
                            if (data)
                                sb.Append("       ...");
                            else
                                sb.Append('.');
                        }
                    }
                    sb.Append(Environment.NewLine);

                    if (matrix.Complex && data)
                    {
                        sb.Append("     ");
                        for (J = StartCol; J <= StopCol; J++)
                        {
                            if (pImagElements[J - StartCol] != null)
                            {
                                if (pImagElements[J - StartCol].Element is Element<Complex> e)
                                    sb.AppendFormat("{0:G3,9}j", e.Value.Imaginary);
                            }
                            else sb.Append("          ");
                        }
                        sb.Append(Environment.NewLine);
                    }
                }

                // Calculate index of first column in next group
                StartCol = StopCol;
                StartCol++;
                sb.Append(Environment.NewLine);
            }
            if (header)
            {
                sb.Append("{1}Largest element in matrix = {0}.{1}".FormatString(LargestElement, Environment.NewLine));
                sb.Append("Smallest element in matrix = {0}.{1}".FormatString(SmallestElement, Environment.NewLine));

                // Search for largest and smallest diagonal values
                for (I = 1; I <= Size; I++)
                {
                    if (matrix.Diag[I] != null)
                    {
                        Magnitude = matrix.Diag[I].Element.Magnitude;
                        if (Magnitude > LargestDiag) LargestDiag = Magnitude;
                        if (Magnitude < SmallestDiag) SmallestDiag = Magnitude;
                    }
                }

                // Print the largest and smallest diagonal values
                if (matrix.Factored)
                {
                    sb.Append("{1}Largest diagonal element = {0}.{1}".FormatString(LargestDiag, Environment.NewLine));
                    sb.Append("Smallest diagonal element = {0}.{1}".FormatString(SmallestDiag, Environment.NewLine));
                }
                else
                {
                    sb.Append("{1}Largest pivot element = {0}.{1}".FormatString(LargestDiag, Environment.NewLine));
                    sb.Append("Smallest pivot element = {0}.{1}".FormatString(SmallestDiag, Environment.NewLine));
                }

                // Calculate and print sparsity and number of fill-ins created
                sb.Append("{1}Density = {0}%.{1}".FormatString(
                    ElementCount * 100.0 / (Size * Size),
                    Environment.NewLine
                    ));
                if (!matrix.NeedsOrdering)
                    sb.Append("Number of fill-ins = {0}.{1}".FormatString(matrix.Fillins, Environment.NewLine));
            }
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
    }
}
