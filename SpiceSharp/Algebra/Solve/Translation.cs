using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// This class can map external to internal indices and vice-versa.
    /// </summary>
    public class Translation
    {
        /// <summary>
        /// Constants
        /// </summary>
        private const float ExpansionFactor = 1.5f;

        /// <summary>
        /// Private variable
        /// </summary>
        private int[] _extToInt;
        private int[] _intToExt;
        private int _allocated;

        /// <summary>
        /// Gets the current length of the translation vector.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Translation"/> class.
        /// </summary>
        /// <param name="size">The number of translations to be allocated.</param>
        public Translation(int size)
        {
            _extToInt = new int[size + 1];
            _intToExt = new int[size + 1];
            for (var i = 1; i <= size; i++)
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
            : this(4)
        {
        }

        /// <summary>
        /// Gets the internal index.
        /// </summary>
        /// <param name="index">The external index.</param>
        /// <returns>The internal index.</returns>
        public int this[int index]
        {
            get
            {
                // Zero is mapped to zero
                if (index == 0)
                    return 0;
                if (index > _allocated)
                    ExpandTranslation(index);
                return _extToInt[index];
            }
        }

        /// <summary>
        /// Gets the external index.
        /// </summary>
        /// <param name="index">The internal index</param>
        /// <returns>The external index.</returns>
        public int Reverse(int index)
        {
            if (index == 0)
                return 0;
            if (index > _allocated)
                ExpandTranslation(index);
            return _intToExt[index];
        }

        /// <summary>
        /// Swap two (internal) indices.
        /// </summary>
        /// <param name="index1">First index.</param>
        /// <param name="index2">Second index.</param>
        public void Swap(int index1, int index2)
        {
            if (index1 > Length || index2 > Length)
                ExpandTranslation(Math.Max(index1, index2));

            // Get the matching external indices
            var tmp = _intToExt[index1];
            _intToExt[index1] = _intToExt[index2];
            _intToExt[index2] = tmp;

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
        public void Scramble<T>(Vector<T> source, Vector<T> target) where T : IFormattable
        {
            source.ThrowIfNull(nameof(source));
            target.ThrowIfNull(nameof(target));
            if (source.Length != target.Length)
                throw new ArgumentException("Length of inputs does not match");

            // Expand translation vectors if necessary
            if (_allocated < source.Length || _allocated < target.Length)
                ExpandTranslation(Math.Max(source.Length, target.Length));

            for (var i = 1; i < _extToInt.Length; i++)
                target[_extToInt[i]] = source[i];
        }

        /// <summary>
        /// Unscramble a vector. The first index of the array is ignored.
        /// </summary>
        /// <typeparam name="T">The value type of the vector.</typeparam>
        /// <param name="source">The source vector.</param>
        /// <param name="target">The target vector.</param>
        public void Unscramble<T>(T[] source, Vector<T> target) where T : IFormattable
        {
            source.ThrowIfNull(nameof(source));
            target.ThrowIfNull(nameof(target));
            if (source.Length != target.Length + 1)
                throw new ArgumentException("Length of inputs does not match");

            // Expand translation vectors if necessary
            if (_allocated < source.Length || _allocated < target.Length)
                ExpandTranslation(Math.Max(source.Length - 1, target.Length));

            for (var i = 1; i < source.Length; i++)
                target[_intToExt[i]] = source[i];
        }

        /// <summary>
        /// Expand the translation map.
        /// </summary>
        /// <param name="newLength">The new length.</param>
        private void ExpandTranslation(int newLength)
        {
            // No need to reallocate vector
            if (newLength <= _allocated)
            {
                Length = newLength;
                return;
            }

            // Reallocate
            var oldAllocated = _allocated;
            _allocated = Math.Max(newLength, (int)(_allocated * ExpansionFactor));

            Array.Resize(ref _extToInt, _allocated + 1);
            Array.Resize(ref _intToExt, _allocated + 1);
            for (var i = oldAllocated + 1; i <= _allocated; i++)
            {
                _extToInt[i] = i;
                _intToExt[i] = i;
            }
            Length = newLength;
        }
    }
}
