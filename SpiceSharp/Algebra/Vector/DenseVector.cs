using System;
using System.Text;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A vector with real values
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="Algebra.Vector{T}" />
    /// <seealso cref="IFormattable" />
    /// <remarks>
    /// <para>The element at index 0 is considered a "trashcan" element under the hood, consistent to <see cref="SparseMatrix{T}" />.
    /// This doesn't really make a difference for indexing the vector, but it does give different meanings to the length of
    /// the vector.</para>
    /// <para>This vector does not automatically expand size if necessary. Under the hood it is basically just an array.</para>
    /// </remarks>
    public class DenseVector<T> : Vector<T>, IFormattable where T : IFormattable
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
                if (index < 0 || index > Length)
                    throw new ArgumentException("Invalid index {0}".FormatString(index));
                return _values[index];
            }
            set
            {
                if (index < 0 || index > Length)
                    throw new ArgumentException("Invalid index {0}".FormatString(index));
                _values[index] = value;
            }
        }

        /// <summary>
        /// Values
        /// </summary>
        private readonly T[] _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector{T}"/> class.
        /// </summary>
        /// <param name="length">The length of the vector.</param>
        public DenseVector(int length)
            : base(length)
        {
            if (length < 0 && length > int.MaxValue - 1)
                throw new ArgumentException("Invalid length {0}".FormatString(length));
            _values = new T[length + 1];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector{T}"/> class.
        /// </summary>
        /// <param name="values">Values of the vector.</param>
        /// <remarks>
        /// The value at index 0 is considered the trashcan element. The length of the vector
        /// is therefore considered to be the array length - 1.
        /// </remarks>
        protected DenseVector(T[] values)
            : base(values?.Length - 1 ?? 0)
        {
            if (values == null)
                _values = new T[1];
            else
                _values = (T[])values.Clone();
        }
        
        /// <summary>
        /// Copies contents to another vector.
        /// </summary>
        /// <param name="vector">The target vector.</param>
        public void CopyTo(DenseVector<T> vector)
        {
            vector.ThrowIfNull(nameof(vector));
            if (vector.Length != Length)
                throw new SparseException("Vector lengths do not match");
            for (var i = 0; i <= Length; i++)
                vector._values[i] = _values[i];
        }

        /// <summary>
        /// Copy contents from another vector.
        /// </summary>
        /// <param name="vector">Source vector.</param>
        public void CopyFrom(DenseVector<T> vector)
        {
            vector.ThrowIfNull(nameof(vector));
            if (vector.Length != Length)
                throw new SparseException("Vector lengths do not match");
            for (var i = 0; i <= Length; i++)
                _values[i] = vector._values[i];
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
    }
}
