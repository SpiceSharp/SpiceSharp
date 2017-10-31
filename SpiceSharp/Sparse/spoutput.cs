using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Sparse
{
    public static class spoutput
    {
        public static int Printer_Width = 80;

        public static string spPrint(this Matrix matrix, bool PrintReordered, bool Data, bool Header)
        {
            int J = 0;
            int I, Row, Col, Size, Top, StartCol = 1, StopCol, Columns, ElementCount = 0;
            double Magnitude, SmallestDiag, SmallestElement;
            double LargestElement = 0.0, LargestDiag = 0.0;
            MatrixElement pElement;
            MatrixElement[] pImagElements;
            int[] PrintOrdToIntRowMap, PrintOrdToIntColMap;

            StringBuilder sb = new StringBuilder();
            Size = matrix.Size;
            pImagElements = new MatrixElement[Printer_Width / 10 + 1];

            // Create a packed external to internal row and column translation array
            Top = matrix.AllocatedExtSize;

            PrintOrdToIntRowMap = new int[Top + 1];
            PrintOrdToIntColMap = new int[Top + 1];
            for (I = 1; I <= Size; I++)
            {
                PrintOrdToIntRowMap[matrix.IntToExtRowMap[I]] = I;
                PrintOrdToIntColMap[matrix.IntToExtColMap[I]] = I;
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
            if (Header)
            {
                sb.Append("MATRIX SUMMARY" + Environment.NewLine + Environment.NewLine);
                sb.Append($"Size of matrix = {Size} x {Size}.{Environment.NewLine}");
                if (matrix.Reordered && PrintReordered)
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
            Columns = Printer_Width;
            if (Header) Columns -= 5;
            if (Data) Columns = (Columns + 1) / 10;

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
                if (Header)
                {
                    if (Data)
                    {
                        sb.Append("    ");
                        for (I = StartCol; I <= StopCol; I++)
                        {
                            if (PrintReordered)
                                Col = I;
                            else
                                Col = PrintOrdToIntColMap[I];
                            sb.AppendFormat(" {0,9}", matrix.IntToExtColMap[Col]);
                        }
                        sb.Append(Environment.NewLine + Environment.NewLine);
                    }
                    else
                    {
                        if (PrintReordered)
                            sb.Append($"Columns {StartCol} to {StopCol}.{Environment.NewLine}");
                        else
                            sb.Append($"Columns {matrix.IntToExtColMap[PrintOrdToIntColMap[StartCol]]} to {matrix.IntToExtColMap[PrintOrdToIntColMap[StopCol]]}.{Environment.NewLine}");
                    }
                }

                // Print every row ...
                for (I = 1; I <= Size; I++)
                {
                    if (PrintReordered)
                        Row = I;
                    else
                        Row = PrintOrdToIntRowMap[I];

                    if (Header)
                    {
                        if (PrintReordered && !Data)
                            sb.AppendFormat("{0,4}", I);
                        else
                            sb.AppendFormat("{0,4}", matrix.IntToExtRowMap[Row]);
                        if (!Data)
                            sb.Append(' ');
                    }

                    // ... in each column of the group
                    for (J = StartCol; J <= StopCol; J++)
                    {
                        if (PrintReordered)
                            Col = J;
                        else
                            Col = PrintOrdToIntColMap[J];

                        pElement = matrix.FirstInCol[Col];
                        while (pElement != null && pElement.Row != Row)
                            pElement = pElement.NextInCol;

                        if (Data)
                            pImagElements[J - StartCol] = pElement;

                        if (pElement != null)
                        {

                            // Case where element exists
                            if (Data)
                                sb.AppendFormat("{0,10:G3}", pElement.Value.Real.ToString());
                            else
                                sb.Append('x');

                            // Update status variables
                            if ((Magnitude = (Math.Abs(pElement.Value.Real) + Math.Abs(pElement.Value.Imag))) > LargestElement)
                                LargestElement = Magnitude;
                            if ((Magnitude < SmallestElement) && (Magnitude != 0.0))
                                SmallestElement = Magnitude;
                            ElementCount++;
                        }
                        else
                        {
                            // Case where element is structurally zero
                            if (Data)
                                sb.Append("       ...");
                            else
                                sb.Append('.');
                        }
                    }
                    sb.Append(Environment.NewLine);

                    if (matrix.Complex && Data)
                    {
                        sb.Append("     ");
                        for (J = StartCol; J <= StopCol; J++)
                        {
                            if (pImagElements[J - StartCol] != null)
                            {
                                sb.AppendFormat("{0, 9:G3}j",pImagElements[J - StartCol].Value.Imag);
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
            if (Header)
            {
                sb.Append($"{Environment.NewLine}Largest element in matrix = {LargestElement}.{Environment.NewLine}");
                sb.Append($"Smallest element in matrix = {SmallestElement}.{Environment.NewLine}");

                // Search for largest and smallest diagonal values
                for (I = 1; I <= Size; I++)
                {
                    if (matrix.Diag[I] != null)
                    {
                        Magnitude = (Math.Abs(matrix.Diag[I].Value.Real) + Math.Abs(matrix.Diag[I].Value.Imag));
                        if (Magnitude > LargestDiag) LargestDiag = Magnitude;
                        if (Magnitude < SmallestDiag) SmallestDiag = Magnitude;
                    }
                }

                // Print the largest and smallest diagonal values
                if (matrix.Factored)
                {
                    sb.Append($"{Environment.NewLine}Largest diagonal element = {LargestDiag}.{Environment.NewLine}");
                    sb.Append($"Smallest diagonal element = {SmallestDiag}.{Environment.NewLine}");
                }
                else
                {
                    sb.Append($"{Environment.NewLine}Largest pivot element = {LargestDiag}.{Environment.NewLine}");
                    sb.Append($"Smallest pivot element = {SmallestDiag}.{Environment.NewLine}");
                }

                // Calculate and print sparsity and number of fill-ins created
                sb.Append($"{Environment.NewLine}Density = {(double)(ElementCount * 100) / (double)(Size * Size)}%.{Environment.NewLine}");
                if (!matrix.NeedsOrdering)
                    sb.Append($"Number of fill-ins = {matrix.Fillins}.{Environment.NewLine}");
            }
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
    }
}
