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
            /// <value>
            /// The first element in the column.
            /// </value>
            public Element FirstInColumn { get; private set; }

            /// <summary>
            /// Gets the last element in the column.
            /// </summary>
            /// <value>
            /// The last element in the column.
            /// </value>
            public Element LastInColumn { get; private set; }

            /// <summary>
            /// Insert an element in the column. This method assumes an element does not exist at its indices!
            /// </summary>
            /// <param name="newElement">The new element to insert.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="newElement"/> is <c>null</c>.</exception>
            public void Insert(Element newElement)
            {
                newElement.ThrowIfNull(nameof(newElement));

                int row = newElement.Row;
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
            /// Clears all matrix elements in the column.
            /// </summary>
            public void Clear()
            {
                LastInColumn = null;
                FirstInColumn = null;
            }

            /// <summary>
            /// Swap two elements in the row, <paramref name="first"/> and <paramref name="rowFirst"/> 
            /// are supposed to come first in the row. Does not update row pointers!
            /// </summary>
            /// <param name="first">The first matrix element.</param>
            /// <param name="second">The second matrix element.</param>
            /// <param name="rowFirst">The first row.</param>
            /// <param name="rowSecond">The second row.</param>
            /// <exception cref="ArgumentNullException">Thrown if both <paramref name="first"/> and <paramref name="second"/> are <c>null</c>.</exception>
            public void Swap(Element first, Element second, int rowFirst, int rowSecond)
            {
                if (first == null && second == null)
                    throw new ArgumentNullException(nameof(first) + ", " + nameof(second));

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
                        (second.Above, first.Above) = (first.Above, second.Above);
                        (second.Below, first.Below) = (first.Below, second.Below);
                        first.Row = rowSecond;
                        second.Row = rowFirst;
                    }
                }
            }
        }
    }
}
