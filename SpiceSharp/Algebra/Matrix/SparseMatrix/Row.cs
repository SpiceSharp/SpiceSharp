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
            /// <value>
            /// The first element in the row.
            /// </value>
            public Element FirstInRow { get; private set; }

            /// <summary>
            /// Gets the last element in the row.
            /// </summary>
            /// <value>
            /// The last element in the row.
            /// </value>
            public Element LastInRow { get; private set; }

            /// <summary>
            /// Gets an element in the row, or creates it if it doesn't exist yet.
            /// </summary>
            /// <param name="location">The location of the element.</param>
            /// <param name="result">The found or created element.</param>
            /// <returns><c>true</c> if the element was found, <c>false</c> if it was created.</returns>
            public bool CreateOrGetElement(MatrixLocation location, out Element result)
            {
                Element element = FirstInRow, lastElement = null;
                while (element != null)
                {
                    if (element.Column > location.Column)
                        break;
                    if (element.Column == location.Column)
                    {
                        // Found the element
                        result = element;
                        return true;
                    }

                    lastElement = element;
                    element = element.Right;
                }

                // Create a new element
                result = new Element(location);

                // Update links for last element
                if (lastElement == null)
                    FirstInRow = result;
                else
                    lastElement.Right = result;
                result.Left = lastElement;

                // Update links for next element
                if (element == null)
                    LastInRow = result;
                else
                    element.Left = result;
                result.Right = element;

                // Could not find existing element
                return false;
            }

            /// <summary>
            /// Find an element in the row without creating it.
            /// </summary>
            /// <param name="column">The column index.</param>
            /// <returns>The element at the specified column, or <c>null</c> if the element doesn't exist.</returns>
            public Element Find(int column)
            {
                var element = FirstInRow;
                while (element != null)
                {
                    if (element.Column == column)
                        return element;
                    if (element.Column > column)
                        return null;
                    element = element.Right;
                }

                return null;
            }

            /// <summary>
            /// Remove an element from the row.
            /// </summary>
            /// <param name="element">The element to be removed.</param>
            public void Remove(Element element)
            {
                if (element.Left == null)
                    FirstInRow = element.Right;
                else
                    element.Left.Right = element.Right;
                if (element.Right == null)
                    LastInRow = element.Left;
                else
                    element.Right.Left = element.Left;
            }

            /// <summary>
            /// Clears all matrix elements in the row.
            /// </summary>
            public void Clear()
            {
                FirstInRow = null;
                LastInRow = null;
            }

            /// <summary>
            /// Swap two elements in the row, <paramref name="first"/> and <paramref name="columnFirst"/> 
            /// are supposed to come first in the row.
            /// </summary>
            /// <param name="first">The first matrix element.</param>
            /// <param name="second">The second matrix element.</param>
            /// <param name="columnFirst">The first column.</param>
            /// <param name="columnSecond">The second column.</param>
            public void Swap(Element first, Element second, int columnFirst,
                int columnSecond)
            {
                if (first == null && second == null)
                    throw new ArgumentNullException(nameof(first) + ", " + nameof(second));

                if (first == null)
                {
                    // Do we need to move the element?
                    if (second.Left == null || second.Left.Column < columnFirst)
                    {
                        second.Column = columnFirst;
                        return;
                    }

                    // Move the element back
                    var element = second.Left;
                    Remove(second);
                    while (element.Left != null && element.Left.Column > columnFirst)
                        element = element.Left;

                    // We now have the first element below the insertion point
                    if (element.Left == null)
                        FirstInRow = second;
                    else
                        element.Left.Right = second;
                    second.Left = element.Left;
                    element.Left = second;
                    second.Right = element;
                    second.Column = columnFirst;
                }
                else if (second == null)
                {
                    // Do we need to move the element?
                    if (first.Right == null || first.Right.Column > columnSecond)
                    {
                        first.Column = columnSecond;
                        return;
                    }

                    // Move the element forward
                    var element = first.Right;
                    Remove(first);
                    while (element.Right != null && element.Right.Column < columnSecond)
                        element = element.Right;

                    // We now have the first element above the insertion point
                    if (element.Right == null)
                        LastInRow = first;
                    else
                        element.Right.Left = first;
                    first.Right = element.Right;
                    element.Right = first;
                    first.Left = element;
                    first.Column = columnSecond;
                }
                else
                {
                    // Are they adjacent or not?
                    if (first.Right == second)
                    {
                        // Correct surrounding links
                        if (first.Left == null)
                            FirstInRow = second;
                        else
                            first.Left.Right = second;
                        if (second.Right == null)
                            LastInRow = first;
                        else
                            second.Right.Left = first;

                        // Correct element links
                        first.Right = second.Right;
                        second.Left = first.Left;
                        first.Left = second;
                        second.Right = first;
                        first.Column = columnSecond;
                        second.Column = columnFirst;
                    }
                    else
                    {
                        // Swap surrounding links
                        if (first.Left == null)
                            FirstInRow = second;
                        else
                            first.Left.Right = second;
                        first.Right.Left = second;
                        if (second.Right == null)
                            LastInRow = first;
                        else
                            second.Right.Left = first;
                        second.Left.Right = first;

                        // Swap element links
                        (second.Left, first.Left) = (first.Left, second.Left);
                        (second.Right, first.Right) = (first.Right, second.Right);
                        first.Column = columnSecond;
                        second.Column = columnFirst;
                    }
                }
            }
        }
    }
}
