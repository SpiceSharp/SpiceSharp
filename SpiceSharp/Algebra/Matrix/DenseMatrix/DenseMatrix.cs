using System;
using System.Collections.Generic;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A square matrix using a dense representation.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IMatrix{T}"/>
    public class DenseMatrix<T> : IMatrix<T>
    {
        private T[] _array;
        private T _trashCan;
        private int _allocatedSize;

        private const int _initialSize = 4;
        private const float _expansionFactor = 1.25f; // expansion for dense matrices allocates ~1.25^2 (1.55X) more memory.

        /// <inheritdoc/>
        public int Size { get; private set; }

        /// <inheritdoc/>
        public T this[int row, int column]
        {
            get => GetMatrixValue(new MatrixLocation(row, column));
            set => SetMatrixValue(new MatrixLocation(row, column), value);
        }

        /// <inheritdoc/>
        public T this[MatrixLocation location]
        {
            get => GetMatrixValue(location);
            set => SetMatrixValue(location, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseMatrix{T}"/> class.
        /// </summary>
        public DenseMatrix()
        {
            Size = 0;
            _allocatedSize = _initialSize;
            _array = new T[_allocatedSize * _allocatedSize];
            _trashCan = default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseMatrix{T}"/> class.
        /// </summary>
        /// <param name="size">The matrix size.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="size"/> is negative.</exception>
        public DenseMatrix(int size)
        {
            Size = size.GreaterThanOrEquals(nameof(size), 0);
            _allocatedSize = Math.Max(_initialSize, size);
            _array = new T[_allocatedSize * _allocatedSize];
            _trashCan = default;
        }

        /// <inheritdoc/>
        public void SwapRows(int row1, int row2)
        {
            row1.GreaterThan(nameof(row1), 0);
            row2.GreaterThan(nameof(row2), 0);
            if (row1 == row2 || (row1 > Size && row2 > Size))
                return;

            // Expand the matrix if necessary
            int needed = Math.Max(row1, row2);
            if (needed > Size)
                Expand(needed);

            int offset1 = (row1 - 1) * _allocatedSize;
            int offset2 = (row2 - 1) * _allocatedSize;
            for (int i = 0; i < Size; i++)
            {
                (_array[offset2 + i], _array[offset1 + i]) = (_array[offset1 + i], _array[offset2 + i]);
            }
        }

        /// <inheritdoc/>
        public void SwapColumns(int column1, int column2)
        {
            column1.GreaterThan(nameof(column1), 0);
            column2.GreaterThan(nameof(column2), 0);
            if (column1 == column2 || (column1 > Size && column2 > Size))
                return;

            // Expand the matrix if necessary
            int needed = Math.Max(column1, column2);
            if (needed > Size)
                Expand(needed);

            column1--;
            column2--;
            for (int i = 0; i < _allocatedSize * _allocatedSize; i += _allocatedSize)
            {
                (_array[column2 + i], _array[column1 + i]) = (_array[column1 + i], _array[column2 + i]);
            }
        }

        /// <inheritdoc/>
        public void Reset()
        {
            for (int i = 0; i < _array.Length; i++)
                _array[i] = default;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _trashCan = default;
            _array = new T[_initialSize * _initialSize];
            _allocatedSize = _initialSize;
            Size = 0;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => "DenseMatrix ({0}x{0})".FormatString(Size);

        private T GetMatrixValue(MatrixLocation location)
        {
            if (location.Row == 0 || location.Column == 0)
                return _trashCan;
            if (location.Row > Size || location.Column > Size)
                return default;
            return _array[(location.Row - 1) * _allocatedSize + location.Column - 1];
        }
        private void SetMatrixValue(MatrixLocation location, T value)
        {
            if (location.Row == 0 || location.Column == 0)
                _trashCan = value;
            else
            {
                if (!EqualityComparer<T>.Default.Equals(value, default) && (location.Row > Size || location.Column > Size))
                    Expand(Math.Max(location.Row, location.Column));
                _array[(location.Row - 1) * _allocatedSize + location.Column - 1] = value;
            }
        }
        private void Expand(int newSize)
        {
            int oldSize = Size;
            Size = newSize;
            if (newSize <= _allocatedSize)
                return;
            newSize = Math.Max(newSize, (int)(_allocatedSize * _expansionFactor));

            // Create a new array and copy its contents
            var nArray = new T[newSize * newSize];
            for (int r = 0; r < oldSize; r++)
                for (int c = 0; c < oldSize; c++)
                    nArray[r * newSize + c] = _array[r * _allocatedSize + c];
            _array = nArray;
            _allocatedSize = newSize;
        }
    }
}
