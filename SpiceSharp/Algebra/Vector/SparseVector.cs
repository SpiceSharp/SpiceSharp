using System;
using System.Text;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Sparse vector
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="Algebra.Vector{T}" />
    /// <seealso cref="IFormattable" />
    /// <remarks>
    /// <para>The element at index 0 is considered a "trashcan" element under the hood, consistent to <see cref="SparseMatrix{T}" />.
    /// This doesn't really make a difference for indexing the vector, but it does give different meanings to the length of
    /// the vector.</para>
    /// <para>This vector automatically expands size if necessary.</para>
    /// </remarks>
    public partial class SparseVector<T> : Vector<T>, IFormattable where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <remarks>
        /// The element at index 0 is considered a trash can element. Use indices ranging 1 to the vector length.
        /// </remarks>
        /// <param name="index">The index in the vector.</param>
        /// <returns>The value at the specified index.</returns>
        public override T this[int index]
        {
            get
            {
                var element = FindElement(index);
                if (element == null)
                    return default;
                return element.Value;
            }
            set
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
        }

        /// <summary>
        /// Gets the first element in the vector.
        /// </summary>
        public VectorElement<T> First => _firstInVector;

        /// <summary>
        /// Gets the last element in the vector.
        /// </summary>
        public VectorElement<T> Last => _lastInVector;

        /// <summary>
        /// Private variables
        /// </summary>
        private SparseVectorElement _firstInVector, _lastInVector;
        private readonly VectorElement<T> _trashCan;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVector{T}"/> class.
        /// </summary>
        public SparseVector()
            : base(1)
        {
            _trashCan = new SparseVectorElement(0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVector{T}"/> class.
        /// </summary>
        /// <param name="length">The length of the vector.</param>
        public SparseVector(int length)
            : base(length)
        {
            _trashCan = new SparseVectorElement(0);
        }

        /// <summary>
        /// Create or get an element in the vector.
        /// </summary>
        /// <param name="index">Index in the vector</param>
        /// <returns>The vector element at the specified index</returns>
        public VectorElement<T> GetElement(int index)
        {
            if (index < 0)
                throw new ArgumentException("Invalid index {0}".FormatString(index));
            if (index == 0)
                return _trashCan;

            // Expand the vector if it is necessary
            if (index > Length)
                Length = index;

            // Find the element
            SparseVectorElement element = _firstInVector, lastElement = null;
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
            var result = new SparseVectorElement(index);

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
            return result;
        }

        /// <summary>
        /// Find an element in the vector without creating it.
        /// </summary>
        /// <param name="index">The index in the vector.</param>
        /// <returns>The element at the specified index, or null if the element does not exist.</returns>
        public VectorElement<T> FindElement(int index)
        {
            if (index < 0)
                throw new ArgumentException("Invalid index {0}".FormatString(index));
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
        /// Remove an element.
        /// </summary>
        /// <param name="element">Element to be removed.</param>
        private void Remove(SparseVectorElement element)
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
        }

        /// <summary>
        /// Swap two elements.
        /// </summary>
        /// <param name="index1">The index of the first element.</param>
        /// <param name="index2">The index of the second element.</param>
        public void Swap(int index1, int index2)
        {
            if (index1 < 0 || index2 < 0)
                throw new SparseException("Invalid indices {0} and {1}".FormatString(index1, index2));
            if (index1 == index2)
                return;
            if (index2 < index1)
            {
                var tmp = index1;
                index1 = index2;
                index2 = tmp;
            }

            // Get the two elements
            SparseVectorElement first = null, second = null;

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

        /// <summary>
        /// Swaps the specified elements.
        /// </summary>
        /// <param name="first">The first element.</param>
        /// <param name="second">The second element.</param>
        /// <param name="index1">The index of the first element.</param>
        /// <param name="index2">The index of the second element.</param>
        private void Swap(SparseVectorElement first, SparseVectorElement second, int index1, int index2)
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
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[");
            var element = First;
            for (var i = 1; i <= Length; i++)
            {
                if (element.Index < i)
                    element = element.Below;
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
            var element = First;
            for (var i = 1; i <= Length; i++)
            {
                if (element.Index < i)
                    element = element.Below;
                sb.AppendLine(element.Index == i ? element.Value.ToString(format, formatProvider) : "...");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
