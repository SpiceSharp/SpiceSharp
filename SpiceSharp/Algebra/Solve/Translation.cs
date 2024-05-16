using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// This class can map external to internal indices and vice-versa.
    /// </summary>
    public class Translation
    {
        private int[] _extToInt;
        private int[] _intToExt;
        private int _allocated;

        private const float _expansionFactor = 1.5f;
        private const int _initialSize = 4;

        /// <summary>
        /// Gets the current length of the translation vector.
        /// </summary>
        /// <value>
        /// The length of the translation vector.
        /// </value>
        public int Length { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Translation"/> class.
        /// </summary>
        /// <param name="size">The number of translations to be allocated.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="size"/> is negative.</exception>
        public Translation(int size)
        {
            size.GreaterThanOrEquals(nameof(size), 0);

            _extToInt = new int[size + 1];
            _intToExt = new int[size + 1];
            for (int i = 1; i <= size; i++)
            {
                _extToInt[i] = i;
                _intToExt[i] = i;
            }
            _allocated = size;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Translation"/> class.
        /// </summary>
        public Translation()
            : this(_initialSize)
        {
        }

        /// <summary>
        /// Gets the internal index from an external index.
        /// </summary>
        /// <value>
        /// The internal index.
        /// </value>
        /// <param name="index">The external index.</param>
        /// <returns>
        /// The internal index.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index" /> is negative.</exception>
        public int this[int index]
        {
            get
            {
                index.GreaterThanOrEquals(nameof(index), 0);

                // Zero is mapped to zero
                if (index == 0)
                    return 0;
                if (index > _allocated)
                    ExpandTranslation(index);
                return _extToInt[index];
            }
        }

        /// <summary>
        /// Gets the external index from an internal index.
        /// </summary>
        /// <param name="index">The internal index</param>
        /// <returns>
        /// The external index.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index" /> is negative.</exception>
        public int Reverse(int index)
        {
            index.GreaterThanOrEquals(nameof(index), 0);

            if (index == 0)
                return 0;
            if (index > _allocated)
                ExpandTranslation(index);
            return _intToExt[index];
        }

        /// <summary>
        /// Swaps two (internal) indices, such that the external indices
        /// still point to the right ones.
        /// </summary>
        /// <param name="index1">First index.</param>
        /// <param name="index2">Second index.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index1" /> or <paramref name="index2" /> is negative.</exception>
        public void Swap(int index1, int index2)
        {
            index1.GreaterThanOrEquals(nameof(index1), 0);
            index2.GreaterThanOrEquals(nameof(index2), 0);

            if (index1 > Length || index2 > Length)
                ExpandTranslation(Math.Max(index1, index2));

            // Get the matching external indices
            (_intToExt[index2], _intToExt[index1]) = (_intToExt[index1], _intToExt[index2]);

            // Update the external indices
            _extToInt[_intToExt[index1]] = index1;
            _extToInt[_intToExt[index2]] = index2;
        }

        /// <summary>
        /// Scramble a vector according to the map.
        /// </summary>
        /// <typeparam name="T">The value type of the vector.</typeparam>
        /// <param name="source">The source vector.</param>
        /// <param name="target">The target vector.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="target" /> does not have the same length as <paramref name="source" />.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source" /> or <paramref name="target" /> is <c>null</c>.</exception>
        public void Scramble<T>(IVector<T> source, IVector<T> target)
        {
            source.ThrowIfNull(nameof(source));
            target.ThrowIfNull(nameof(target));
            if (source.Length != target.Length)
                throw new ArgumentException(Properties.Resources.Algebra_VectorLengthMismatch.FormatString(target.Length, source.Length), nameof(target));

            // Expand translation vectors if necessary
            if (_allocated < source.Length || _allocated < target.Length)
                ExpandTranslation(Math.Max(source.Length, target.Length));

            for (int i = 1; i < _extToInt.Length; i++)
                target[_extToInt[i]] = source[i];
        }

        /// <summary>
        /// Unscramble a vector. The first index of the array is ignored.
        /// </summary>
        /// <typeparam name="T">The value type of the vector.</typeparam>
        /// <param name="source">The source vector.</param>
        /// <param name="target">The target vector.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="target" /> (including the trashcan element)
        /// does not have the same number of elements as <paramref name="source" />.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source" /> or <paramref name="target" /> is <c>null</c>.</exception>
        public void Unscramble<T>(T[] source, IVector<T> target)
        {
            source.ThrowIfNull(nameof(source));
            target.ThrowIfNull(nameof(target));
            if (source.Length != target.Length + 1)
                throw new ArgumentException(Properties.Resources.Algebra_VectorLengthMismatch.FormatString(target.Length, source.Length - 1), nameof(target));

            // Expand translation vectors if necessary
            if (_allocated < source.Length || _allocated < target.Length)
                ExpandTranslation(Math.Max(source.Length - 1, target.Length));

            for (int i = 1; i < source.Length; i++)
                target[_intToExt[i]] = source[i];
        }

        /// <summary>
        /// Clears all translations.
        /// </summary>
        public void Clear()
        {
            int size = _initialSize;
            _extToInt = new int[size + 1];
            _intToExt = new int[size + 1];
            for (int i = 1; i <= size; i++)
            {
                _extToInt[i] = i;
                _intToExt[i] = i;
            }
            _allocated = size;
        }

        private void ExpandTranslation(int newLength)
        {
            // No need to reallocate vector
            if (newLength <= _allocated)
            {
                Length = newLength;
                return;
            }

            // Reallocate
            int oldAllocated = _allocated;
            _allocated = Math.Max(newLength, (int)(_allocated * _expansionFactor));

            Array.Resize(ref _extToInt, _allocated + 1);
            Array.Resize(ref _intToExt, _allocated + 1);
            for (int i = oldAllocated + 1; i <= _allocated; i++)
            {
                _extToInt[i] = i;
                _intToExt[i] = i;
            }
            Length = newLength;
        }
    }
}
