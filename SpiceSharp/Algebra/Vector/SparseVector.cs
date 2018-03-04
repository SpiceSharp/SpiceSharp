using System;
using System.Text;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Sparse vector
    /// </summary>
    [Serializable]
    public class SparseVector<T> : Vector<T>, IFormattable where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Constants
        /// </summary>
        const int InitialSize = 4;
        const float ExpansionFactor = 1.5f;

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
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
        /// Gets the first element in the vector
        /// </summary>
        public VectorElement<T> First { get => firstInVector; }

        /// <summary>
        /// Gets the last element in the vector
        /// </summary>
        public VectorElement<T> Last { get => lastInVector; }

        /// <summary>
        /// Private variables
        /// </summary>
        SparseVectorElement<T> firstInVector, lastInVector;
        VectorElement<T> trashCan;

        /// <summary>
        /// Constructor
        /// </summary>
        public SparseVector()
            : base(1)
        {
            trashCan = new SparseVectorElement<T>(0);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        public SparseVector(int length)
            : base(length)
        {
            trashCan = new SparseVectorElement<T>(0);
        }

        /// <summary>
        /// Create or get an element in the vector
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public VectorElement<T> GetElement(int index)
        {
            if (index < 0)
                throw new ArgumentException("Invalid index {0}".FormatString(index));
            if (index == 0)
                return trashCan;

            // Expand the vector if it is necessary
            if (index > Length)
                Length = index;

            // Find the element
            SparseVectorElement<T> element = firstInVector, lastElement = null;
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
            var result = new SparseVectorElement<T>(index);

            // Update links for last element
            if (lastElement == null)
                firstInVector = result;
            else
                lastElement.NextInVector = result;
            result.PreviousInVector = lastElement;

            // Update links for next element
            if (element == null)
                lastInVector = result;
            else
                element.PreviousInVector = result;
            result.NextInVector = element;
            return result;
        }

        /// <summary>
        /// Find an element in the vector without creating it
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public VectorElement<T> FindElement(int index)
        {
            if (index < 0)
                throw new ArgumentException("Invalid index {0}".FormatString(index));
            if (index > Length)
                return null;
            if (index == 0)
                return trashCan;

            // Find the element
            var element = firstInVector;
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
        /// Remove an element
        /// </summary>
        /// <param name="element">Element</param>
        void Remove(SparseVectorElement<T> element)
        {
            // Update surrounding links
            if (element.PreviousInVector == null)
                firstInVector = element.NextInVector;
            else
                element.PreviousInVector.NextInVector = element.NextInVector;
            if (element.NextInVector == null)
                lastInVector = element.PreviousInVector;
            else
                element.NextInVector.PreviousInVector = element.PreviousInVector;
        }

        /// <summary>
        /// Swap two elements
        /// </summary>
        /// <param name="index1">Index 1</param>
        /// <param name="index2">Index 2</param>
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
            SparseVectorElement<T> first = null, second = null;

            // Find first element
            var element = firstInVector;
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
        /// Swap two elements in the vector
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        void Swap(SparseVectorElement<T> first, SparseVectorElement<T> second, int index1, int index2)
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
                    firstInVector = second;
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
                    lastInVector = first;
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
                        firstInVector = second;
                    else
                        first.PreviousInVector.NextInVector = second;
                    if (second.NextInVector == null)
                        lastInVector = first;
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
                        firstInVector = second;
                    else
                        first.PreviousInVector.NextInVector = second;
                    first.NextInVector.PreviousInVector = second;
                    if (second.NextInVector == null)
                        lastInVector = first;
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
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            var element = First;
            for (int i = 1; i <= Length; i++)
            {
                if (element.Index < i)
                    element = element.Next;
                if (element.Index == i)
                    sb.AppendLine(element.Value.ToString());
                else
                    sb.AppendLine("...");
            }
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <param name="format">Format</param>
        /// <param name="formatProvider">Format provider</param>
        /// <returns></returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");
            var element = First;
            for (int i = 1; i <= Length; i++)
            {
                if (element.Index < i)
                    element = element.Next;
                if (element.Index == i)
                    sb.AppendLine(element.Value.ToString(format, formatProvider));
                else
                    sb.AppendLine("...");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
