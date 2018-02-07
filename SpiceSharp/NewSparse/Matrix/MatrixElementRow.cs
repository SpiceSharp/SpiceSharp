using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.NewSparse
{
    /// <summary>
    /// A class keeping track of a linked list of matrix elements
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MatrixElementRow<T> : IEnumerable<MatrixElement<T>>
    {
        /// <summary>
        /// Gets the first element in the row
        /// </summary>
        public MatrixElement<T> FirstInRow { get; private set; }

        /// <summary>
        /// Insert an element in the row
        /// This method assumes an element does not exist at its indices!
        /// </summary>
        /// <param name="newElement">New element</param>
        public void Insert(MatrixElement<T> newElement)
        {
            int column = newElement.Column;
            MatrixElement<T> element = FirstInRow, lastElement = null;
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
            if (element != null)
                element.PreviousInRow = newElement;
            newElement.NextInRow = element;
        }

        /// <summary>
        /// Create or get an element in the row
        /// </summary>
        /// <param name="row">Row (used for creating a new element)</param>
        /// <param name="column">Column</param>
        /// <param name="result">The found or created element</param>
        /// <returns>True if the element was found, false if it was created</returns>
        public bool CreateGetElement(int row, int column, out MatrixElement<T> result)
        {
            result = null;
            MatrixElement<T> element = FirstInRow, lastElement = null;
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
            result = new MatrixElement<T>(row, column);

            // Update links for last element
            if (lastElement == null)
                FirstInRow = result;
            else
                lastElement.NextInRow = result;
            result.PreviousInRow = lastElement;

            // Update links for next element
            if (element != null)
                element.PreviousInRow = result;
            result.NextInRow = element;

            // Could not find existing element
            return false;
        }

        /// <summary>
        /// Find an element in the row without creating it, returns null if it doesn't exist
        /// </summary>
        /// <param name="column">Column</param>
        public MatrixElement<T> Find(int column)
        {
            MatrixElement<T> element = FirstInRow;
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
        /// Remove an element from the row
        /// </summary>
        /// <param name="element">Element</param>
        public void Remove(MatrixElement<T> element)
        {
            if (element.PreviousInRow == null)
                FirstInRow = element.NextInRow;
            else
                element.PreviousInRow.NextInRow = element.NextInRow;
            if (element.NextInRow != null)
                element.NextInRow.PreviousInRow = element.PreviousInRow;
        }

        /// <summary>
        /// Swap two elements in the row, <paramref name="first"/> and <paramref name="columnFirst"/> are supposed to come first in the row.
        /// Does not update column pointers!
        /// </summary>
        /// <param name="first">First</param>
        /// <param name="second">Second</param>
        public void Swap(MatrixElement<T> first, MatrixElement<T> second, int columnFirst, int columnSecond)
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
                MatrixElement<T> element = second.PreviousInRow;
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
                }
                else
                {
                    // Move the element forward
                    MatrixElement<T> element = first.NextInRow;
                    Remove(first);
                    while (element.NextInRow != null && element.NextInRow.Column < columnSecond)
                        element = element.NextInRow;

                    // We now have the first element above the insertion point
                    if (element.NextInRow != null)
                        element.NextInRow.PreviousInRow = first;
                    first.NextInRow = element.NextInRow;
                    element.PreviousInRow = element;
                    element.NextInRow = first;
                    first.Column = columnSecond;
                }
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
                    if (second.NextInRow != null)
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
                    if (second.NextInRow != null)
                        second.NextInRow.PreviousInRow = first;
                    second.PreviousInRow.NextInRow = first;

                    // Swap element links
                    MatrixElement<T> element = first.PreviousInRow;
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

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<MatrixElement<T>> GetEnumerator()
        {
            MatrixElement<T> element = FirstInRow;
            while (element != null)
            {
                yield return element;
                element = element.NextInRow;
            }
        }

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
