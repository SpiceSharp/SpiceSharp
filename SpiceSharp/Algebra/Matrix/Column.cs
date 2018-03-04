using System;

namespace SpiceSharp.Algebra.Matrix
{
    /// <summary>
    /// Column class
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    class Column<T> where T : IFormattable
    {
        /// <summary>
        /// Gets the first element in the column
        /// </summary>
        public SparseMatrixElement<T> FirstInColumn { get; private set; }

        /// <summary>
        /// Gets the last element in the column
        /// </summary>
        public SparseMatrixElement<T> LastInColumn { get; private set; }

        /// <summary>
        /// Insert an element in the column
        /// This method assumes an element does not exist at its indices!
        /// </summary>
        /// <param name="newElement">New element</param>
        public void Insert(SparseMatrixElement<T> newElement)
        {
            int row = newElement.Row;
            SparseMatrixElement<T> element = FirstInColumn, lastElement = null;
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
        /// Create or get an element in the column
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column (used for creating a new element)</param>
        /// <param name="result">The found or created element</param>
        /// <returns>True if the element was found, false if it was created</returns>
        public bool CreateGetElement(int row, int column, out SparseMatrixElement<T> result)
        {
            SparseMatrixElement<T> element = FirstInColumn, lastElement = null;
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
            result = new SparseMatrixElement<T>(row, column);

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
        /// Find an element in the column without creating it, returns null if it doesn't exist
        /// </summary>
        /// <param name="row">Row</param>
        public SparseMatrixElement<T> Find(int row)
        {
            SparseMatrixElement<T> element = FirstInColumn;
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
        /// Remove an element from the row
        /// </summary>
        /// <param name="element">Element</param>
        public void Remove(SparseMatrixElement<T> element)
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
        /// Swap two elements in the column, <paramref name="first"/> and <paramref name="rowFirst"/> are supposed to come first in the column.
        /// Does not update row pointers!
        /// </summary>
        /// <param name="first">First matrix element</param>
        /// <param name="second">Second matrix element</param>
        /// <param name="rowFirst">First row</param>
        /// <param name="rowSecond">Second row</param>
        public void Swap(SparseMatrixElement<T> first, SparseMatrixElement<T> second, int rowFirst, int rowSecond)
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
                SparseMatrixElement<T> element = second.PreviousInColumn;
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
                SparseMatrixElement<T> element = first.NextInColumn;
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
                    SparseMatrixElement<T> element = first.PreviousInColumn;
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
