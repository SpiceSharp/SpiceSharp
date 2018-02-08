using System;
using System.Text;

namespace SpiceSharp.NewSparse
{
    /// <summary>
    /// A vector with real values
    /// </summary>
    [Serializable]
    public class Vector<T> : ICloneable
    {
        /// <summary>
        /// Gets or sets a value
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentException("Invalid index {0}".FormatString(index));
                return values[index];
            }
            set
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentException("Invalid index {0}".FormatString(index));
                values[index] = value;
            }
        }

        /// <summary>
        /// Gets the length of the vector
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Values
        /// </summary>
        T[] values;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        public Vector(int length)
        {
            if (length < 1)
                throw new Exception("Invalid vector length {0}".FormatString(length));
            Length = length;
            values = new T[length];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="values">Values</param>
        protected Vector(T[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            Length = values.Length;
            this.values = (T[])values.Clone();
        }

        /// <summary>
        /// Copy contents from one vector to another
        /// </summary>
        /// <param name="vector">Vector</param>
        public void CopyTo(Vector<T> vector)
        {
            if (vector == null)
                throw new ArgumentNullException(nameof(vector));
            if (vector.Length != Length)
                throw new Exception("Vector lengths do not match");
            for (int i = 0; i < Length; i++)
                vector.values[i] = values[i];
        }

        /// <summary>
        /// Copy contents from another vector
        /// </summary>
        /// <param name="vector"></param>
        public void CopyFrom(Vector<T> vector)
        {
            if (vector == null)
                throw new ArgumentNullException(nameof(vector));
            if (vector.Length != Length)
                throw new Exception("Vector lengths do not match");
            for (int i = 0; i < Length; i++)
                values[i] = vector.values[i];
        }

        /// <summary>
        /// Clone the vector
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Vector<T>(values);
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");
            for (int i = 1; i < Length; i++)
                sb.AppendLine(values[i].ToString());
            sb.AppendLine("]");
            return sb.ToString();
        }
    }
}
