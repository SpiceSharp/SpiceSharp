using System;
using System.Text;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A vector with real values
    /// </summary>
    [Serializable]
    public class DenseVector<T> : Vector<T>, IFormattable where T : IFormattable
    {
        /// <summary>
        /// Gets or sets a value
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public override T this[int index]
        {
            get
            {
                if (index < 0 || index > Length)
                    throw new ArgumentException("Invalid index {0}".FormatString(index));
                return values[index];
            }
            set
            {
                if (index < 0 || index > Length)
                    throw new ArgumentException("Invalid index {0}".FormatString(index));
                values[index] = value;
            }
        }

        /// <summary>
        /// Values
        /// </summary>
        T[] values;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        public DenseVector(int length)
            : base(length)
        {
            if (length < 0 && length > int.MaxValue - 1)
                throw new ArgumentException("Invalid length {0}".FormatString(length));
            values = new T[length + 1];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="values">Values</param>
        protected DenseVector(T[] values)
            : base(values?.Length - 1 ?? 0)
        {
            if (values == null)
                this.values = new T[1];
            else
                this.values = (T[])values.Clone();
        }

        /// <summary>
        /// Copy contents from one vector to another
        /// </summary>
        /// <param name="vector">Vector</param>
        public void CopyTo(DenseVector<T> vector)
        {
            if (vector == null)
                throw new ArgumentNullException(nameof(vector));
            if (vector.Length != Length)
                throw new SparseException("Vector lengths do not match");
            for (int i = 0; i < Length; i++)
                vector.values[i] = values[i];
        }

        /// <summary>
        /// Copy contents from another vector
        /// </summary>
        /// <param name="vector"></param>
        public void CopyFrom(DenseVector<T> vector)
        {
            if (vector == null)
                throw new ArgumentNullException(nameof(vector));
            if (vector.Length != Length)
                throw new SparseException("Vector lengths do not match");
            for (int i = 0; i < Length; i++)
                values[i] = vector.values[i];
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");
            for (int i = 1; i <= Length; i++)
                sb.AppendLine(values[i].ToString());
            sb.AppendLine("]");
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
            for (int i = 1; i <= Length; i++)
                sb.AppendLine(values[i].ToString(format, formatProvider));
            sb.AppendLine("]");
            return sb.ToString();
        }
    }
}
