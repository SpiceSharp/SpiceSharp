using System;

namespace SpiceSharp.Algebra
{
    public partial class SparseMatrix<T>
    {
        /// <summary>
        /// A class that keeps track of a linked list of matrix elements for a column.
        /// </summary>
        protected class Column
        {
            /// <summary>
            /// Gets the first element in the column.
            /// </summary>
            public SparseMatrixElement FirstInColumn { get; private set; }

            /// <summary>
            /// Gets the last element in the column.
            /// </summary>
            public SparseMatrixElement LastInColumn { get; private set; }

            /// <summary>
            /// Insert an element in the column. This method assumes an element does not exist at its indices!
            /// </summary>
            /// <param name="newElement">The new element to insert.</param>
            public void Insert(SparseMatrixElement newElement)
            {
                var row = newElement.Row;
                SparseMatrixElement element = FirstInColumn, lastElement = null;
                while (element != null)
                {
                    if (element.Row > row)
                        break;
                    lastElement = element;
                    element = element.NextInColumn;
                }

                // Update links for last element
                if (lastElement == null)
                    FirstInColumn = newElement;
                else
                    lastElement.NextInColumn = newElement;
                newElement.PreviousInColumn = lastElement;

                // Update links for next element
                if (element == null)
                    LastInColumn = newElement;
                else
                    element.PreviousInColumn = newElement;
                newElement.NextInColumn = element;
            }

            /// <summary>
            /// Create or get an element in the column.
            /// </summary>
            /// <param name="row">The row index used for creating a new element</param>
            /// <param name="column">The column index.</param>
            /// <param name="result">The found or created element.</param>
            /// <returns>True if the element was found, false if it was created.</returns>
            public bool CreateGetElement(int row, int column, out SparseMatrixElement result)
            {
                SparseMatrixElement element = FirstInColumn, lastElement = null;
                while (element != null)
                {
                    if (element.Row > row)
                        break;
                    if (element.Row == row)
                    {
                        result = element;
                        return true;
                    }

                    lastElement = element;
                    element = element.NextInColumn;
                }

                // Create a new element
                result = new SparseMatrixElement(row, column);

                // Update links for last element
                if (lastElement == null)
                    FirstInColumn = result;
                else
                    lastElement.NextInColumn = result;
                result.PreviousInColumn = lastElement;

                // Update links for next element
                if (element == null)
                    LastInColumn = result;
                else
                    element.PreviousInColumn = result;
                result.NextInColumn = element;

                // Did not find element
                return false;
            }

            /// <summary>
            /// Find an element in the row without creating it.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <returns>The element at the specified row, or null if it doesn't exist.</returns>
            public SparseMatrixElement Find(int row)
            {
                var element = FirstInColumn;
                while (element != null)
                {
                    if (element.Row == row)
                        return element;
                    if (element.Row > row)
                        return null;
                    element = element.NextInColumn;
                }

                return null;
            }
            
            /// <summary>
            /// Remove an element from the column.
            /// </summary>
            /// <param name="element">The element to be removed.</param>
            public void Remove(SparseMatrixElement element)
            {
                if (element.PreviousInColumn == null)
                    FirstInColumn = element.NextInColumn;
                else
                    element.PreviousInColumn.NextInColumn = element.NextInColumn;
                if (element.NextInColumn == null)
                    LastInColumn = element.PreviousInColumn;
                else
                    element.NextInColumn.PreviousInColumn = element.PreviousInColumn;
            }

            /// <summary>
            /// Swap two elements in the row, <paramref name="first"/> and <paramref name="rowFirst"/> 
            /// are supposed to come first in the row. Does not update row pointers!
            /// </summary>
            /// <param name="first">The first matrix element.</param>
            /// <param name="second">The second matrix element.</param>
            /// <param name="rowFirst">The first row.</param>
            /// <param name="rowSecond">The second row.</param>
            public void Swap(SparseMatrixElement first, SparseMatrixElement second, int rowFirst, int rowSecond)
            {
                if (first == null && second == null)
                    throw new ArgumentException("Both matrix elements cannot be null");

                if (first == null)
                {
                    // Do we need to move the element?
                    if (second.PreviousInColumn == null || second.PreviousInColumn.Row < rowFirst)
                    {
                        second.Row = rowFirst;
                        return;
                    }

                    // Move the element back
                    var element = second.PreviousInColumn;
                    Remove(second);
                    while (element.PreviousInColumn != null && element.PreviousInColumn.Row > rowFirst)
                        element = element.PreviousInColumn;

                    // We now have the first element below the insertion point
                    if (element.PreviousInColumn == null)
                        FirstInColumn = second;
                    else
                        element.PreviousInColumn.NextInColumn = second;
                    second.PreviousInColumn = element.PreviousInColumn;
                    element.PreviousInColumn = second;
                    second.NextInColumn = element;
                    second.Row = rowFirst;
                }
                else if (second == null)
                {
                    // Do we need to move the element?
                    if (first.NextInColumn == null || first.NextInColumn.Row > rowSecond)
                    {
                        first.Row = rowSecond;
                        return;
                    }

                    // Move the element forward
                    var element = first.NextInColumn;
                    Remove(first);
                    while (element.NextInColumn != null && element.NextInColumn.Row < rowSecond)
                        element = element.NextInColumn;

                    // We now have the first element above the insertion point
                    if (element.NextInColumn == null)
                        LastInColumn = first;
                    else
                        element.NextInColumn.PreviousInColumn = first;
                    first.NextInColumn = element.NextInColumn;
                    element.NextInColumn = first;
                    first.PreviousInColumn = element;
                    first.Row = rowSecond;
                }
                else
                {
                    // Are they adjacent or not?
                    if (first.NextInColumn == second)
                    {
                        // Correct surrounding links
                        if (first.PreviousInColumn == null)
                            FirstInColumn = second;
                        else
                            first.PreviousInColumn.NextInColumn = second;
                        if (second.NextInColumn == null)
                            LastInColumn = first;
                        else
                            second.NextInColumn.PreviousInColumn = first;

                        // Correct element links
                        first.NextInColumn = second.NextInColumn;
                        second.PreviousInColumn = first.PreviousInColumn;
                        first.PreviousInColumn = second;
                        second.NextInColumn = first;
                        first.Row = rowSecond;
                        second.Row = rowFirst;
                    }
                    else
                    {
                        // Swap surrounding links
                        if (first.PreviousInColumn == null)
                            FirstInColumn = second;
                        else
                            first.PreviousInColumn.NextInColumn = second;
                        first.NextInColumn.PreviousInColumn = second;
                        if (second.NextInColumn == null)
                            LastInColumn = first;
                        else
                            second.NextInColumn.PreviousInColumn = first;
                        second.PreviousInColumn.NextInColumn = first;

                        // Correct element links
                        var element = first.PreviousInColumn;
                        first.PreviousInColumn = second.PreviousInColumn;
                        second.PreviousInColumn = element;
                        element = first.NextInColumn;
                        first.NextInColumn = second.NextInColumn;
                        second.NextInColumn = element;
                        first.Row = rowSecond;
                        second.Row = rowFirst;
                    }
                }
            }
        }
    }
}
