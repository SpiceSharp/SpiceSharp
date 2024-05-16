using System;

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
    public partial class DenseVector<T> : IVector<T>
    {
        private const float _expansionFactor = 1.5f;
        private const int _initialSize = 4;

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get; private set; }

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
        private T[] _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector{T}"/> class.
        /// </summary>
        public DenseVector()
        {
            Length = 0;
            _values = new T[_initialSize];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector{T}"/> class.
        /// </summary>
        /// <param name="length">The length of the vector.</param>
        public DenseVector(int length)
        {
            if (length < 0 && length > int.MaxValue - 1)
                throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
            _values = new T[length + 1];
        }

        /// <summary>
        /// Gets the value of the vector at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The value.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative</exception>
        public T GetVectorValue(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index > Length)
                return default;
            return _values[index];
        }

        /// <summary>
        /// Sets the value of the vector at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void SetVectorValue(int index, T value)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index > Length)
                Expand(index);
            _values[index] = value;
        }

        /// <summary>
        /// Swaps two elements in the vector.
        /// </summary>
        /// <param name="index1">The first index.</param>
        /// <param name="index2">The second index.</param>
        public void SwapElements(int index1, int index2)
        {
            if (index1 < 0)
                throw new ArgumentOutOfRangeException(nameof(index1));
            if (index2 < 0)
                throw new ArgumentOutOfRangeException(nameof(index2));
            if (index1 > Length || index2 > Length)
                Expand(Math.Max(index1, index2));
            if (index1 == index2)
                return;
            (_values[index2], _values[index1]) = (_values[index1], _values[index2]);
        }

        /// <summary>
        /// Resets all elements in the vector.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < _values.Length; i++)
                _values[i] = default;
        }

        /// <summary>
        /// Clears all elements in the vector. The size of the vector becomes 0.
        /// </summary>
        public void Clear()
        {
            _values = new T[_initialSize + 1];
            Length = 0;
        }

        /// <summary>
        /// Copies the contents of the vector to another one.
        /// </summary>
        /// <param name="target">The target vector.</param>
        public void CopyTo(IVector<T> target)
        {
            target.ThrowIfNull(nameof(target));
            if (Length != target.Length)
                throw new ArgumentException(Properties.Resources.Algebra_VectorLengthMismatch.FormatString(target.Length, Length), nameof(target));
            if (target == this)
                return;
            for (int i = 1; i <= Length; i++)
                target[i] = this[i];
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => "Dense vector ({0})".FormatString(Length);

        private void Expand(int newSize)
        {
            Length = newSize;
            if (newSize + 1 <= _values.Length)
                return;
            newSize = Math.Max(newSize, (int)(_values.Length * _expansionFactor));
            Array.Resize(ref _values, newSize + 1);
        }
    }
}
