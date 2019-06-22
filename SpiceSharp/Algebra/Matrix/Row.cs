using System;

namespace SpiceSharp.Algebra
{
    public partial class SparseMatrix<T>
    {
        /// <summary>
        /// A class that keeps track of a linked list of matrix elements for a row.
        /// </summary>
        protected class Row
        {
            /// <summary>
            /// Gets the first element in the row.
            /// </summary>
            public SparseMatrixElement FirstInRow { get; private set; }

            /// <summary>
            /// Gets the last element in the row.
            /// </summary>
            public SparseMatrixElement LastInRow { get; private set; }

            /// <summary>
            /// Insert an element in the row. This method assumes an element does not exist at its indices!
            /// </summary>
            /// <param name="newElement">The new element to insert.</param>
            public void Insert(SparseMatrixElement newElement)
            {
                var column = newElement.Column;
                SparseMatrixElement element = FirstInRow, lastElement = null;
                while (element != null)
                {
                    if (element.Column > column)
                        break;
                    lastElement = element;
                    element = element.NextInRow;
                }

                // Update links for last element
                if (lastElement == null)
                    FirstInRow = newElement;
                else
                    lastElement.NextInRow = newElement;
                newElement.PreviousInRow = lastElement;

                // Update links for next element
                if (element == null)
                    LastInRow = newElement;
                else
                    element.PreviousInRow = newElement;
                newElement.NextInRow = element;
            }

            /// <summary>
            /// Create or get an element in the row.
            /// </summary>
            /// <param name="row">The row index used for creating a new element</param>
            /// <param name="column">The column index.</param>
            /// <param name="result">The found or created element.</param>
            /// <returns>True if the element was found, false if it was created.</returns>
            public bool CreateGetElement(int row, int column, out SparseMatrixElement result)
            {
                SparseMatrixElement element = FirstInRow, lastElement = null;
                while (element != null)
                {
                    if (element.Column > column)
                        break;
                    if (element.Column == column)
                    {
                        // Found the element
                        result = element;
                        return true;
                    }

                    lastElement = element;
                    element = element.NextInRow;
                }

                // Create a new element
                result = new SparseMatrixElement(row, column);

                // Update links for last element
                if (lastElement == null)
                    FirstInRow = result;
                else
                    lastElement.NextInRow = result;
                result.PreviousInRow = lastElement;

                // Update links for next element
                if (element == null)
                    LastInRow = result;
                else
                    element.PreviousInRow = result;
                result.NextInRow = element;

                // Could not find existing element
                return false;
            }

            /// <summary>
            /// Find an element in the row without creating it.
            /// </summary>
            /// <param name="column">The column index.</param>
            /// <returns>The element at the specified column, or null if it doesn't exist.</returns>
            public SparseMatrixElement Find(int column)
            {
                var element = FirstInRow;
                while (element != null)
                {
                    if (element.Column == column)
                        return element;
                    if (element.Column > column)
                        return null;
                    element = element.NextInRow;
                }

                return null;
            }

            /// <summary>
            /// Remove an element from the row.
            /// </summary>
            /// <param name="element">The element to be removed.</param>
            public void Remove(SparseMatrixElement element)
            {
                if (element.PreviousInRow == null)
                    FirstInRow = element.NextInRow;
                else
                    element.PreviousInRow.NextInRow = element.NextInRow;
                if (element.NextInRow == null)
                    LastInRow = element.PreviousInRow;
                else
                    element.NextInRow.PreviousInRow = element.PreviousInRow;
            }

            /// <summary>
            /// Swap two elements in the row, <paramref name="first"/> and <paramref name="columnFirst"/> 
            /// are supposed to come first in the row. Does not update column pointers!
            /// </summary>
            /// <param name="first">The first matrix element.</param>
            /// <param name="second">The second matrix element.</param>
            /// <param name="columnFirst">The first column.</param>
            /// <param name="columnSecond">The second column.</param>
            public void Swap(SparseMatrixElement first, SparseMatrixElement second, int columnFirst,
                int columnSecond)
            {
                if (first == null && second == null)
                    throw new ArgumentException("Both matrix elements cannot be null");

                if (first == null)
                {
                    // Do we need to move the element?
                    if (second.PreviousInRow == null || second.PreviousInRow.Column < columnFirst)
                    {
                        second.Column = columnFirst;
                        return;
                    }

                    // Move the element back
                    var element = second.PreviousInRow;
                    Remove(second);
                    while (element.PreviousInRow != null && element.PreviousInRow.Column > columnFirst)
                        element = element.PreviousInRow;

                    // We now have the first element below the insertion point
                    if (element.PreviousInRow == null)
                        FirstInRow = second;
                    else
                        element.PreviousInRow.NextInRow = second;
                    second.PreviousInRow = element.PreviousInRow;
                    element.PreviousInRow = second;
                    second.NextInRow = element;
                    second.Column = columnFirst;
                }
                else if (second == null)
                {
                    // Do we need to move the element?
                    if (first.NextInRow == null || first.NextInRow.Column > columnSecond)
                    {
                        first.Column = columnSecond;
                        return;
                    }

                    // Move the element forward
                    var element = first.NextInRow;
                    Remove(first);
                    while (element.NextInRow != null && element.NextInRow.Column < columnSecond)
                        element = element.NextInRow;

                    // We now have the first element above the insertion point
                    if (element.NextInRow == null)
                        LastInRow = first;
                    else
                        element.NextInRow.PreviousInRow = first;
                    first.NextInRow = element.NextInRow;
                    element.NextInRow = first;
                    first.PreviousInRow = element;
                    first.Column = columnSecond;
                }
                else
                {
                    // Are they adjacent or not?
                    if (first.NextInRow == second)
                    {
                        // Correct surrounding links
                        if (first.PreviousInRow == null)
                            FirstInRow = second;
                        else
                            first.PreviousInRow.NextInRow = second;
                        if (second.NextInRow == null)
                            LastInRow = first;
                        else
                            second.NextInRow.PreviousInRow = first;

                        // Correct element links
                        first.NextInRow = second.NextInRow;
                        second.PreviousInRow = first.PreviousInRow;
                        first.PreviousInRow = second;
                        second.NextInRow = first;
                        first.Column = columnSecond;
                        second.Column = columnFirst;
                    }
                    else
                    {
                        // Swap surrounding links
                        if (first.PreviousInRow == null)
                            FirstInRow = second;
                        else
                            first.PreviousInRow.NextInRow = second;
                        first.NextInRow.PreviousInRow = second;
                        if (second.NextInRow == null)
                            LastInRow = first;
                        else
                            second.NextInRow.PreviousInRow = first;
                        second.PreviousInRow.NextInRow = first;

                        // Swap element links
                        var element = first.PreviousInRow;
                        first.PreviousInRow = second.PreviousInRow;
                        second.PreviousInRow = element;

                        element = first.NextInRow;
                        first.NextInRow = second.NextInRow;
                        second.NextInRow = element;
                        first.Column = columnSecond;
                        second.Column = columnFirst;
                    }
                }
            }
        }
    }
}
