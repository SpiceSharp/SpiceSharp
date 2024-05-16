using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A vector that uses sparse storage methods with doubly-linked elements.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="ISparseVector{T}" />
    /// <remarks>
    /// <para>The element at index 0 is considered a "trashcan" element under the hood, consistent to <see cref="SparseMatrix{T}" />.
    /// This doesn't really make a difference for indexing the vector though.
    /// </para>
    /// <para>This vector automatically expands size if necessary.</para>
    /// </remarks>
    public partial class SparseVector<T> : ISparseVector<T>
    {
        private Element _firstInVector, _lastInVector;
        private readonly Element _trashCan;

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get; private set; }

        /// <inheritdoc/>
        public int ElementCount { get; private set; }

        /// <inheritdoc/>
        public T this[int index]
        {
            get => GetVectorValue(index);
            set => SetVectorValue(index, value);
        }

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
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="length"/> is negative.</exception>
        public SparseVector(int length)
        {
            Length = length.GreaterThanOrEquals(nameof(length), 0);
            _trashCan = new Element(0);
            ElementCount = 1;
        }

        /// <inheritdoc/>
        public Element<T> GetElement(int index)
        {
            index.GreaterThanOrEquals(nameof(index), 0);
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

        /// <inheritdoc/>
        public bool RemoveElement(int index)
        {
            if (index < 1 || index > Length)
                return false;

            // Find the element
            var element = _firstInVector;
            while (element != null)
            {
                if (element.Index == index)
                    break;
                if (element.Index > index)
                    return false;
                element = element.NextInVector;
            }

            if (element == null)
                return false;
            Remove(element);
            return true;
        }

        /// <inheritdoc/>
        public Element<T> FindElement(int index)
        {
            index.GreaterThanOrEquals(nameof(index), 0);
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

        /// <inheritdoc/>
        public ISparseVectorElement<T> GetFirstInVector() => _firstInVector;

        /// <inheritdoc/>
        public ISparseVectorElement<T> GetLastInVector() => _lastInVector;

        /// <inheritdoc/>
        public void CopyTo(IVector<T> target)
        {
            target.ThrowIfNull(nameof(target));
            if (Length != target.Length)
                throw new ArgumentException(Properties.Resources.Algebra_VectorLengthMismatch.FormatString(target.Length, Length), nameof(target));
            if (target == this)
                return;
            for (int i = 1; i <= Length; i++)
                target[i] = GetVectorValue(i);
        }

        /// <inheritdoc/>
        public void SwapElements(int index1, int index2)
        {
            index1.GreaterThan(nameof(index1), 0);
            index2.GreaterThan(nameof(index2), 0);
            if (index1 == index2)
                return;
            if (index2 < index1)
            {
                (index2, index1) = (index1, index2);
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
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
        public override string ToString() => "Sparse vector ({0})".FormatString(Length);

        private T GetVectorValue(int index)
        {
            var element = FindElement(index);
            if (element == null)
                return default;
            return element.Value;
        }
        private void SetVectorValue(int index, T value)
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
                    (second.PreviousInVector, first.PreviousInVector) = (first.PreviousInVector, second.PreviousInVector);
                    (second.NextInVector, first.NextInVector) = (first.NextInVector, second.NextInVector);
                    first.Index = index2;
                    second.Index = index1;
                }
            }
        }
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
    }
}
