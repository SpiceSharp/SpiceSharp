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
            public Element FirstInColumn { get; private set; }

            /// <summary>
            /// Gets the last element in the column.
            /// </summary>
            public Element LastInColumn { get; private set; }

            /// <summary>
            /// Insert an element in the column. This method assumes an element does not exist at its indices!
            /// </summary>
            /// <param name="newElement">The new element to insert.</param>
            public void Insert(Element newElement)
            {
                var row = newElement.Row;
                Element element = FirstInColumn, lastElement = null;
                while (element != null)
                {
                    if (element.Row > row)
                        break;
                    lastElement = element;
                    element = element.Below;
                }

                // Update links for last element
                if (lastElement == null)
                    FirstInColumn = newElement;
                else
                    lastElement.Below = newElement;
                newElement.Above = lastElement;

                // Update links for next element
                if (element == null)
                    LastInColumn = newElement;
                else
                    element.Above = newElement;
                newElement.Below = element;
            }

            /// <summary>
            /// Create or get an element in the column.
            /// </summary>
            /// <param name="row">The row index used for creating a new element</param>
            /// <param name="column">The column index.</param>
            /// <param name="result">The found or created element.</param>
            /// <returns>True if the element was found, false if it was created.</returns>
            public bool CreateGetElement(int row, int column, out Element result)
            {
                Element element = FirstInColumn, lastElement = null;
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
                    element = element.Below;
                }

                // Create a new element
                result = new Element(row, column);

                // Update links for last element
                if (lastElement == null)
                    FirstInColumn = result;
                else
                    lastElement.Below = result;
                result.Above = lastElement;

                // Update links for next element
                if (element == null)
                    LastInColumn = result;
                else
                    element.Above = result;
                result.Below = element;

                // Did not find element
                return false;
            }

            /// <summary>
            /// Find an element in the row without creating it.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <returns>The element at the specified row, or null if it doesn't exist.</returns>
            public Element Find(int row)
            {
                var element = FirstInColumn;
                while (element != null)
                {
                    if (element.Row == row)
                        return element;
                    if (element.Row > row)
                        return null;
                    element = element.Below;
                }

                return null;
            }
            
            /// <summary>
            /// Remove an element from the column.
            /// </summary>
            /// <param name="element">The element to be removed.</param>
            public void Remove(Element element)
            {
                if (element.Above == null)
                    FirstInColumn = element.Below;
                else
                    element.Above.Below = element.Below;
                if (element.Below == null)
                    LastInColumn = element.Above;
                else
                    element.Below.Above = element.Above;
            }

            /// <summary>
            /// Swap two elements in the row, <paramref name="first"/> and <paramref name="rowFirst"/> 
            /// are supposed to come first in the row. Does not update row pointers!
            /// </summary>
            /// <param name="first">The first matrix element.</param>
            /// <param name="second">The second matrix element.</param>
            /// <param name="rowFirst">The first row.</param>
            /// <param name="rowSecond">The second row.</param>
            public void Swap(Element first, Element second, int rowFirst, int rowSecond)
            {
                if (first == null && second == null)
                    throw new ArgumentException("Both matrix elements cannot be null");

                if (first == null)
                {
                    // Do we need to move the element?
                    if (second.Above == null || second.Above.Row < rowFirst)
                    {
                        second.Row = rowFirst;
                        return;
                    }

                    // Move the element back
                    var element = second.Above;
                    Remove(second);
                    while (element.Above != null && element.Above.Row > rowFirst)
                        element = element.Above;

                    // We now have the first element below the insertion point
                    if (element.Above == null)
                        FirstInColumn = second;
                    else
                        element.Above.Below = second;
                    second.Above = element.Above;
                    element.Above = second;
                    second.Below = element;
                    second.Row = rowFirst;
                }
                else if (second == null)
                {
                    // Do we need to move the element?
                    if (first.Below == null || first.Below.Row > rowSecond)
                    {
                        first.Row = rowSecond;
                        return;
                    }

                    // Move the element forward
                    var element = first.Below;
                    Remove(first);
                    while (element.Below != null && element.Below.Row < rowSecond)
                        element = element.Below;

                    // We now have the first element above the insertion point
                    if (element.Below == null)
                        LastInColumn = first;
                    else
                        element.Below.Above = first;
                    first.Below = element.Below;
                    element.Below = first;
                    first.Above = element;
                    first.Row = rowSecond;
                }
                else
                {
                    // Are they adjacent or not?
                    if (first.Below == second)
                    {
                        // Correct surrounding links
                        if (first.Above == null)
                            FirstInColumn = second;
                        else
                            first.Above.Below = second;
                        if (second.Below == null)
                            LastInColumn = first;
                        else
                            second.Below.Above = first;

                        // Correct element links
                        first.Below = second.Below;
                        second.Above = first.Above;
                        first.Above = second;
                        second.Below = first;
                        first.Row = rowSecond;
                        second.Row = rowFirst;
                    }
                    else
                    {
                        // Swap surrounding links
                        if (first.Above == null)
                            FirstInColumn = second;
                        else
                            first.Above.Below = second;
                        first.Below.Above = second;
                        if (second.Below == null)
                            LastInColumn = first;
                        else
                            second.Below.Above = first;
                        second.Above.Below = first;

                        // Correct element links
                        var element = first.Above;
                        first.Above = second.Above;
                        second.Above = element;
                        element = first.Below;
                        first.Below = second.Below;
                        second.Below = element;
                        first.Row = rowSecond;
                        second.Row = rowFirst;
                    }
                }
            }
        }
    }
}
