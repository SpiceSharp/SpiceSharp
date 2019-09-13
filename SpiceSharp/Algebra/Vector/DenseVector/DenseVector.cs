using System;
using System.Text;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A vector with real values
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <remarks>
    /// <para>The element at index 0 is considered a "trashcan" element under the hood, consistent to <see cref="SparseMatrix{T}" />.
    /// This doesn't really make a difference for indexing the vector, but it does give different meanings to the length of
    /// the vector.</para>
    /// <para>This vector does not automatically expand size if necessary. Under the hood it is basically just an array.</para>
    /// </remarks>
    public partial class DenseVector<T> : IPermutableVector<T>, IFormattable where T : IFormattable
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
        public int Length { get; }

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
        /// Values
        /// </summary>
        private readonly T[] _values;
        private readonly TrashCanElement _trashCan;

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector{T}"/> class.
        /// </summary>
        /// <param name="length">The length of the vector.</param>
        public DenseVector(int length)
        {
            if (length < 0 && length > int.MaxValue - 1)
                throw new ArgumentException("Invalid length {0}".FormatString(length));
            Length = length;
            _values = new T[length];
            _trashCan = new TrashCanElement();
        }

        /// <summary>
        /// Gets the value of the vector at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The value.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">index</exception>
        public T GetVectorValue(int index)
        {
            if (index < 0 || index > Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index == 0)
                return _trashCan.Value;
            return _values[index - 1];
        }


        /// <summary>
        /// Sets the value of the vector at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void SetVectorValue(int index, T value)
        {
            if (index < 0 || index > Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index == 0)
                _trashCan.Value = value;
            else
                _values[index - 1] = value;
        }

        /// <summary>
        /// Gets a vector element at the specified index. If
        /// it doesn't exist, a new one is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The vector element.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">index</exception>
        public IVectorElement<T> GetVectorElement(int index)
        {
            if (index < 0 || index > Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index == 0)
                return _trashCan;
            return new Element(this, index);
        }

        /// <summary>
        /// Finds a vector element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The vector element; otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">index</exception>
        public IVectorElement<T> FindVectorElement(int index)
            => GetVectorElement(index);

        /// <summary>
        /// Swaps two elements in the vector.
        /// </summary>
        /// <param name="index1">The first index.</param>
        /// <param name="index2">The second index.</param>
        public void SwapElements(int index1, int index2)
        {
            if (index1 < 0 || index1 > Length)
                throw new ArgumentOutOfRangeException(nameof(index1));
            if (index2 < 0 || index2 > Length)
                throw new ArgumentOutOfRangeException(nameof(index2));
            if (index1 == index2)
                return;
            var tmp = _values[index1];
            _values[index1] = _values[index2];
            _values[index2] = tmp;

            OnElementsSwapped(new PermutationEventArgs(index1, index2));
        }

        /// <summary>
        /// Resets all elements in the vector.
        /// </summary>
        public void ResetVector()
        {
            _trashCan.Value = default;
            for (var i = 0; i < _values.Length; i++)
                _values[i] = default;
        }

        /// <summary>
        /// Copies the contents of the vector to another one.
        /// </summary>
        /// <param name="target">The target vector.</param>
        public void CopyTo(IVector<T> target)
        {
            target.ThrowIfNull(nameof(target));
            if (target.Length != Length)
                throw new ArgumentException("Vector lengths do not match");
            for (var i = 1; i <= Length; i++)
                target[i] = this[i];
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
            sb.AppendLine("[");
            for (var i = 1; i <= Length; i++)
                sb.AppendLine(_values[i].ToString());
            sb.AppendLine("]");
            return sb.ToString();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <param name="format">The format for each element of the vector.</param>
        /// <param name="formatProvider">The format provider for each element of the vector.</param>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[");
            for (var i = 1; i <= Length; i++)
                sb.AppendLine(_values[i].ToString(format, formatProvider));
            sb.AppendLine("]");
            return sb.ToString();
        }

        /// <summary>
        /// Gets the first <see cref="IVectorElement{T}" /> in the vector.
        /// </summary>
        /// <returns>
        /// The vector element.
        /// </returns>
        public IVectorElement<T> GetFirstInVector()
            => GetVectorElement(1);

        /// <summary>
        /// Raises the <see cref="ElementsSwapped" /> event.
        /// </summary>
        /// <param name="args">The <see cref="PermutationEventArgs"/> instance containing the event data.</param>
        protected virtual void OnElementsSwapped(PermutationEventArgs args) => ElementsSwapped?.Invoke(this, args);
    }
}
