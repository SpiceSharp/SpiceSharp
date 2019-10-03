using SpiceSharp.Algebra;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Circuits.Entities.Local
{
    /// <summary>
    /// A solver that allows concurrent access to its elements.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="ISolver{T}" />
    public class LocalSolver<T> : ISolver<T> where T : IFormattable
    {
        private ISolver<T> _parent;
        private Dictionary<MatrixLocation, LocalSolverElement> _matrixElements = new Dictionary<MatrixLocation, LocalSolverElement>(new MatrixLocation.Comparer());
        private Dictionary<int, LocalSolverElement> _vectorElements = new Dictionary<int, LocalSolverElement>();

        /// <summary>
        /// A local solver element that accomodates concurrent add/subtract
        /// </summary>
        /// <seealso cref="ISolver{T}" />
        protected class LocalSolverElement : ISolverElement<T>
        {
            /// <summary>
            /// Gets or sets the lock.
            /// </summary>
            /// <value>
            /// The lock.
            /// </value>
            public object Lock { get; set; }

            private ISolverElement<T> _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="LocalSolverElement"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public LocalSolverElement(ISolverElement<T> parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <summary>
            /// Adds the specified value to the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void Add(T value)
            {
                if (Lock != null)
                {
                    lock (Lock)
                    {
                        _parent.Add(value);
                    }
                }
                else
                    _parent.Add(value);
            }

            /// <summary>
            /// Gets the value of the matrix element.
            /// </summary>
            /// <returns>
            /// The matrix element value.
            /// </returns>
            public T GetValue()
            {
                if (Lock != null)
                {
                    lock (Lock)
                    {
                        return _parent.GetValue();
                    }
                }
                return _parent.GetValue();
            }

            /// <summary>
            /// Sets the specified value for the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void SetValue(T value)
            {
                if (Lock != null)
                {
                    lock (Lock)
                        _parent.SetValue(value);
                }
                else
                    _parent.SetValue(value);
            }

            /// <summary>
            /// Subtracts the specified value from the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void Subtract(T value)
            {
                if (Lock != null)
                {
                    lock (Lock)
                    {
                        _parent.Subtract(value);
                    }
                }
                else
                    _parent.Subtract(value);
            }

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="String" /> that represents this instance.
            /// </returns>
            public override string ToString()
                => _parent.ToString();

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="formatProvider">The format provider.</param>
            /// <returns>
            /// A <see cref="String" /> that represents this instance.
            /// </returns>
            public string ToString(string format, IFormatProvider formatProvider)
                => _parent.ToString(format, formatProvider);
        }

        /// <summary>
        /// Gets or sets the order of the system that needs to be solved.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order 
        {
            get => _parent.Order;
            set => _parent.Order = value;
        }

        /// <summary>
        /// Gets the size of the matrix and right-hand side vector.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int Size => _parent.Size;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalSolver{T}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public LocalSolver(ISolver<T> parent)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
        }

        /// <summary>
        /// Maps an external row/column tuple to an internal one.
        /// </summary>
        /// <param name="indices">The external row/column indices.</param>
        /// <returns>
        /// The internal row/column indices.
        /// </returns>
        public MatrixLocation ExternalToInternal(MatrixLocation indices)
            => _parent.ExternalToInternal(indices);

        /// <summary>
        /// Factor the Y-matrix and Rhs-vector.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the factoring was successful; otherwise <c>false</c>.
        /// </returns>
        public bool Factor()
            => _parent.Factor();

        /// <summary>
        /// Finds the element at the specified position in the matrix.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The element if it exists; otherwise <c>null</c>.
        /// </returns>
        public ISolverElement<T> FindElement(int row, int column)
        {
            var elt = _parent.FindElement(row, column);
            if (elt == null)
                return null;

            // We don't care about concurrent writes to the trashcan element
            if (row == 0 || column == 0)
                return elt;

            var loc = new MatrixLocation(row, column);
            if (_matrixElements.TryGetValue(loc, out var le))
            {
                // Second time asking for this element, let's lock it
                if (le.Lock == null)
                    le.Lock = new object();
            }
            else
            {
                le = new LocalSolverElement(elt);
                _matrixElements.Add(loc, le);
            }
            return le;
        }

        /// <summary>
        /// Finds the element at the specified position in the right-hand side vector.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>
        /// The element if it exists; otherwise <c>null</c>.
        /// </returns>
        public ISolverElement<T> FindElement(int row)
        {
            var elt = _parent.FindElement(row);
            if (elt == null)
                return null;
            
            // We don't care about concurrent writes to the trashcan element
            if (row == 0)
                return elt;

            if (_vectorElements.TryGetValue(row, out var le))
            {
                // Second time asking for the element, let's lock it
                if (le.Lock == null)
                    le.Lock = new object();
            }
            else
            {
                le = new LocalSolverElement(elt);
                _vectorElements.Add(row, le);
            }
            return le;
        }

        /// <summary>
        /// Gets the element at the specified position in the matrix. A new element is
        /// created if it doesn't exist yet.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        public ISolverElement<T> GetElement(int row, int column)
        {
            var elt = _parent.GetElement(row, column);

            // We don't care about concurrent writes to the trashcan element
            if (row == 0 || column == 0)
                return elt;

            var loc = new MatrixLocation(row, column);
            if (_matrixElements.TryGetValue(loc, out var le))
            {
                // Second time asking for this element, let's lock it
                if (le.Lock == null)
                    le.Lock = new object();
            }
            else
            {
                le = new LocalSolverElement(elt);
                _matrixElements.Add(loc, le);
            }
            return le;
        }

        /// <summary>
        /// Gets the element at the specified position in the right-hand side vector.
        /// A new element is created if it doesn't exist yet.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>
        /// The vector element.
        /// </returns>
        public ISolverElement<T> GetElement(int row)
        {
            var elt = _parent.GetElement(row);

            // We don't care about concurrent writes to the trashcan element
            if (row == 0)
                return elt;

            if (_vectorElements.TryGetValue(row, out var le))
            {
                // Second time asking for this element, let's lock it
                if (le.Lock == null)
                    le.Lock = new object();
            }
            else
            {
                le = new LocalSolverElement(elt);
                _vectorElements.Add(row, le);
            }
            return le;
        }

        /// <summary>
        /// Maps an internal row/column tuple to an external one.
        /// </summary>
        /// <param name="indices">The internal row/column indices.</param>
        /// <returns>
        /// The external row/column indices.
        /// </returns>
        public MatrixLocation InternalToExternal(MatrixLocation indices)
            => _parent.InternalToExternal(indices);

        /// <summary>
        /// Order and factor the Y-matrix and Rhs-vector.
        /// </summary>
        public void OrderAndFactor()
            => _parent.OrderAndFactor();

        /// <summary>
        /// Preconditions the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        public void Precondition(PreconditionMethod<T> method)
            => _parent.Precondition(method);

        /// <summary>
        /// Clears all matrix and vector elements.
        /// </summary>
        public void Reset()
            => _parent.Reset();

        /// <summary>
        /// Resets the matrix.
        /// </summary>
        public void ResetMatrix()
            => _parent.ResetMatrix();

        /// <summary>
        /// Resets the right-hand side vector.
        /// </summary>
        public void ResetVector()
            => _parent.ResetVector();

        /// <summary>
        /// Solves the equations using the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="solution">The solution.</param>
        public void Solve(IVector<T> solution)
            => _parent.Solve(solution);

        /// <summary>
        /// Solves the equations using the transposed Y-matrix.
        /// </summary>
        /// <param name="solution">The solution.</param>
        public void SolveTransposed(IVector<T> solution)
            => _parent.SolveTransposed(solution);
    }
}
