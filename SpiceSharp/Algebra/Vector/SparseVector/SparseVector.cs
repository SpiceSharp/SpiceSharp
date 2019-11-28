using System;
using System.Text;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A vector that uses sparse storage methods with doubly-linked elements.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IFormattable" />
    /// <remarks>
    /// <para>The element at index 0 is considered a "trashcan" element under the hood, consistent to <see cref="SparseMatrix{T}" />.
    /// This doesn't really make a difference for indexing the vector though.
    /// </para>
    /// <para>This vector automatically expands size if necessary.</para>
    /// </remarks>
    public partial class SparseVector<T> : IPermutableVector<T>, ISparseVector<T> where T : IFormattable
    {
        /// <summary>
        /// Occurs when two elements have swapped.
        /// </summary>
        public event EventHandler<PermutationEventArgs> ElementsSwapped;

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get; private set; }

        /// <summary>
        /// Gets the number of elements in the vector.
        /// </summary>
        /// <value>
        /// The element count.
        /// </value>
        public int ElementCount { get; private set; }

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The value.</returns>
        public T this[int index]
        {
            get => GetVectorValue(index);
            set => SetVectorValue(index, value);
        }

        /// <summary>
        /// Private variables
        /// </summary>
        private Element _firstInVector, _lastInVector;
        private readonly Element _trashCan;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVector{T}"/> class.
        /// </summary>
        public SparseVector()
        {
            Length = 0;
            _trashCan = new Element(0);
            ElementCount = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVector{T}"/> class.
        /// </summary>
        /// <param name="length">The length of the vector.</param>
        public SparseVector(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
            _trashCan = new Element(0);
            ElementCount = 1;
        }

        /// <summary>
        /// Gets the value of the vector at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public T GetVectorValue(int index)
        {
            var element = FindElement(index);
            if (element == null)
                return default;
            return element.Value;
        }

        /// <summary>
        /// Sets the value of the vector at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void SetVectorValue(int index, T value)
        {
            if (value.Equals(default))
            {
                // We don't need to create a new element unnecessarily
                var element = FindElement(index);
                if (element != null)
                    element.Value = default;
            }
            else
            {
                var element = GetElement(index);
                element.Value = value;
            }
        }

        /// <summary>
        /// Creates or get an element in the vector.
        /// </summary>
        /// <param name="index">Index in the vector</param>
        /// <returns>The vector element at the specified index</returns>
        public Element<T> GetElement(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index == 0)
                return _trashCan;

            // Expand the vector if it is necessary
            if (index > Length)
                Length = index;

            // Find the element
            Element element = _firstInVector, lastElement = null;
            while (element != null)
            {
                if (element.Index > index)
                    break;
                if (element.Index == index)
                    return element;
                lastElement = element;
                element = element.NextInVector;
            }

            // Create a new element
            var result = new Element(index);

            // Update links for last element
            if (lastElement == null)
                _firstInVector = result;
            else
                lastElement.NextInVector = result;
            result.PreviousInVector = lastElement;

            // Update links for next element
            if (element == null)
                _lastInVector = result;
            else
                element.PreviousInVector = result;
            result.NextInVector = element;

            ElementCount++;
            return result;
        }

        /// <summary>
        /// Find an element in the vector without creating it.
        /// </summary>
        /// <param name="index">The index in the vector.</param>
        /// <returns>The element at the specified index, or null if the element does not exist.</returns>
        public Element<T> FindElement(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index > Length)
                return null;
            if (index == 0)
                return _trashCan;

            // Find the element
            var element = _firstInVector;
            while (element != null)
            {
                if (element.Index == index)
                    return element;
                if (element.Index > index)
                    return null;
                element = element.NextInVector;
            }
            return null;
        }

        /// <summary>
        /// Gets the first <see cref="ISparseVectorElement{T}" /> in the vector.
        /// </summary>
        /// <returns>
        /// The vector element.
        /// </returns>
        public ISparseVectorElement<T> GetFirstInVector() => _firstInVector;

        /// <summary>
        /// Gets the last <see cref="ISparseVectorElement{T}" /> in the vector.
        /// </summary>
        /// <returns></returns>
        public ISparseVectorElement<T> GetLastInVector() => _lastInVector;

        /// <summary>
        /// Remove an element.
        /// </summary>
        /// <param name="element">Element to be removed.</param>
        private void Remove(Element element)
        {
            // Update surrounding links
            if (element.PreviousInVector == null)
                _firstInVector = element.NextInVector;
            else
                element.PreviousInVector.NextInVector = element.NextInVector;
            if (element.NextInVector == null)
                _lastInVector = element.PreviousInVector;
            else
                element.NextInVector.PreviousInVector = element.PreviousInVector;
            ElementCount--;
        }

        /// <summary>
        /// Copies the contents of the vector to another one.
        /// </summary>
        /// <param name="target">The target vector.</param>
        public void CopyTo(IVector<T> target)
        {
            target.ThrowIfNull(nameof(target));
            if (target.Length != Length)
                throw new SizeMismatchException(nameof(target), Length);
            for (var i = 1; i <= Length; i++)
                target[i] = GetVectorValue(i);
        }

        /// <summary>
        /// Swap two elements.
        /// </summary>
        /// <param name="index1">The index of the first element.</param>
        /// <param name="index2">The index of the second element.</param>
        public void SwapElements(int index1, int index2)
        {
            if (index1 < 0)
                throw new ArgumentOutOfRangeException(nameof(index1));
            if (index2 < 0)
                throw new ArgumentOutOfRangeException(nameof(index2));
            if (index1 == index2)
                return;
            if (index2 < index1)
            {
                var tmp = index1;
                index1 = index2;
                index2 = tmp;
            }

            // Get the two elements
            Element first = null, second = null;

            // Find first element
            var element = _firstInVector;
            while (element != null)
            {
                if (element.Index == index1)
                    first = element;
                if (element.Index > index1)
                    break;
                element = element.NextInVector;
            }

            // Find second element
            while (element != null)
            {
                if (element.Index == index2)
                    second = element;
                if (element.Index > index2)
                    break;
                element = element.NextInVector;
            }

            // Swap these elements
            Swap(first, second, index1, index2);

            OnElementsSwapped(new PermutationEventArgs(index1, index2));
        }

        /// <summary>
        /// Swaps the specified elements.
        /// </summary>
        /// <param name="first">The first element.</param>
        /// <param name="second">The second element.</param>
        /// <param name="index1">The index of the first element.</param>
        /// <param name="index2">The index of the second element.</param>
        private void Swap(Element first, Element second, int index1, int index2)
        {
            // Nothing to do
            if (first == null && second == null)
                return;

            // Swap the elements
            if (first == null)
            {
                // Do we need to move the element?
                if (second.PreviousInVector == null || second.PreviousInVector.Index < index1)
                {
                    second.Index = index1;
                    return;
                }

                // Move the element back
                var element = second.PreviousInVector;
                Remove(second);
                while (element.PreviousInVector != null && element.PreviousInVector.Index > index1)
                    element = element.PreviousInVector;

                // We now have the element below the insertion point
                if (element.PreviousInVector == null)
                    _firstInVector = second;
                else
                    element.PreviousInVector.NextInVector = second;
                second.PreviousInVector = element.PreviousInVector;
                element.PreviousInVector = second;
                second.NextInVector = element;
                second.Index = index1;
            }
            else if (second == null)
            {
                // Do we need to move the element?
                if (first.NextInVector == null || first.NextInVector.Index > index2)
                {
                    first.Index = index2;
                    return;
                }

                // Move element forward
                var element = first.NextInVector;
                Remove(first);
                while (element.NextInVector != null && element.NextInVector.Index < index2)
                    element = element.NextInVector;

                // We now have the first element above the insertion point
                if (element.NextInVector == null)
                    _lastInVector = first;
                else
                    element.NextInVector.PreviousInVector = first;
                first.NextInVector = element.NextInVector;
                element.NextInVector = first;
                first.PreviousInVector = element;
                first.Index = index2;
            }
            else
            {
                // Are they adjacent or not?
                if (first.NextInVector == second)
                {
                    // Correct surrounding links
                    if (first.PreviousInVector == null)
                        _firstInVector = second;
                    else
                        first.PreviousInVector.NextInVector = second;
                    if (second.NextInVector == null)
                        _lastInVector = first;
                    else
                        second.NextInVector.PreviousInVector = first;

                    // Correct element links
                    first.NextInVector = second.NextInVector;
                    second.PreviousInVector = first.PreviousInVector;
                    first.PreviousInVector = second;
                    second.NextInVector = first;
                    first.Index = index2;
                    second.Index = index1;
                }
                else
                {
                    // Swap surrounding links
                    if (first.PreviousInVector == null)
                        _firstInVector = second;
                    else
                        first.PreviousInVector.NextInVector = second;
                    first.NextInVector.PreviousInVector = second;
                    if (second.NextInVector == null)
                        _lastInVector = first;
                    else
                        second.NextInVector.PreviousInVector = first;
                    second.PreviousInVector.NextInVector = first;

                    // Swap element links
                    var element = first.PreviousInVector;
                    first.PreviousInVector = second.PreviousInVector;
                    second.PreviousInVector = element;

                    element = first.NextInVector;
                    first.NextInVector = second.NextInVector;
                    second.NextInVector = element;

                    first.Index = index2;
                    second.Index = index1;
                }
            }
        }

        /// <summary>
        /// Resets all elements in the vector to their default value.
        /// </summary>
        public void Reset()
        {
            _trashCan.Value = default;
            var elt = _firstInVector;
            while (elt != null)
            {
                elt.Value = default;
                elt = elt.NextInVector;
            }
        }

        /// <summary>
        /// Clears all elements in the vector. The size of the vector becomes 0.
        /// </summary>
        public void Clear()
        {
            _trashCan.Value = default;
            _firstInVector = null;
            _lastInVector = null;
            ElementCount = 1;
            Length = 0;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[");
            var element = _firstInVector;
            for (var i = 1; i <= Length; i++)
            {
                if (element.Index < i)
                    element = element.NextInVector;
                sb.AppendLine(element.Index == i ? element.Value.ToString() : "...");
            }
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[");
            var element = _firstInVector;
            for (var i = 1; i <= Length; i++)
            {
                if (element.Index < i)
                    element = element.NextInVector;
                sb.AppendLine(element.Index == i ? element.Value.ToString(format, formatProvider) : "...");
            }
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Raises the <see cref="ElementsSwapped" /> event.
        /// </summary>
        /// <param name="args">The <see cref="PermutationEventArgs"/> instance containing the event data.</param>
        protected virtual void OnElementsSwapped(PermutationEventArgs args) => ElementsSwapped?.Invoke(this, args);
    }
}
