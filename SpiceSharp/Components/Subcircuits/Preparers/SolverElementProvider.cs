using SpiceSharp.Algebra;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An <see cref="ISparseSolver{T}"/> that can be used to make a wrapper around each element. These wrapped
    /// elements can be accessed in multithreaded environments, and can be applied once when calling the method
    /// <see cref="ApplyElements"/>.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="ISparseSolver{T}" />
    public class SolverElementProvider<T> : ISparseSolver<T> where T : IFormattable
    {
        private ISparseSolver<T> _parent;
        private Dictionary<MatrixLocation, LocalElement> _elements = new Dictionary<MatrixLocation, LocalElement>();
        private Dictionary<int, LocalElement> _rhsElements = new Dictionary<int, LocalElement>();

        /// <summary>
        /// A local element that can be accessed within the task.
        /// </summary>
        /// <seealso cref="ISparseSolver{T}" />
        protected class LocalElement : Element<T>
        {
            /// <summary>
            /// Gets the parent element.
            /// </summary>
            /// <value>
            /// The parent.
            /// </value>
            public Element<T> Parent { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="LocalElement"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public LocalElement(Element<T> parent)
            {
                Parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <summary>
            /// Applies the value to the parent element.
            /// </summary>
            public void Apply()
                => Parent.Add(Value);
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
            set => throw new ArgumentException("Cannot set order in a parallel entity"); 
        }

        /// <summary>
        /// Gets the size of the matrix and right-hand side vector.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int Size => _parent.Size;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolverElementProvider{T}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public SolverElementProvider(ISparseSolver<T> parent)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
        }

        /// <summary>
        /// Applies all elements to the parent matrix.
        /// </summary>
        public void ApplyElements()
        {
            foreach (var elt in _elements.Values)
                elt.Apply();
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
        public Element<T> FindElement(int row, int column)
        {
            var loc = new MatrixLocation(row, column);
            if (!_elements.TryGetValue(loc, out var result))
            {
                var elt = _parent.FindElement(row, column);
                result = new LocalElement(elt);
                _elements.Add(loc, result);
            }
            return result;
        }

        /// <summary>
        /// Finds the element at the specified position in the right-hand side vector.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>
        /// The element if it exists; otherwise <c>null</c>.
        /// </returns>
        public Element<T> FindElement(int row)
        {
            if (!_rhsElements.TryGetValue(row, out var result))
            {
                var elt = _parent.FindElement(row);
                if (elt == null)
                    return null;
                result = new LocalElement(elt);
                _rhsElements.Add(row, result);
            }
            return result;
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
        public Element<T> GetElement(int row, int column)
        {
            var loc = new MatrixLocation(row, column);
            if (!_elements.TryGetValue(loc, out var result))
            {
                var elt = _parent.GetElement(row, column);
                result = new LocalElement(elt);
                _elements.Add(loc, result);
            }
            return result;
        }

        /// <summary>
        /// Gets the element at the specified position in the right-hand side vector.
        /// A new element is created if it doesn't exist yet.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>
        /// The vector element.
        /// </returns>
        public Element<T> GetElement(int row)
        {
            if (!_rhsElements.TryGetValue(row, out var result))
            {
                var elt = _parent.GetElement(row);
                result = new LocalElement(elt);
                _rhsElements.Add(row, result);
            }
            return result;
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
            => throw new CircuitException("Cannot order and factor a {0}.".FormatString(this));

        /// <summary>
        /// Preconditions the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <exception cref="CircuitException">Cannot precondition a {0}.".FormatString(this)</exception>
        public void Precondition(PreconditionMethod<T> method)
            => throw new CircuitException("Cannot precondition a {0}.".FormatString(this));

        /// <summary>
        /// Clears all matrix and vector elements.
        /// </summary>
        public void Reset()
        {
            foreach (var elt in _elements.Values)
                elt.Value = default;
            foreach (var elt in _rhsElements.Values)
                elt.Value = default;
        }

        /// <summary>
        /// Resets the matrix.
        /// </summary>
        public void ResetMatrix()
        {
            foreach (var elt in _elements.Values)
                elt.Value = default;
        }

        /// <summary>
        /// Resets the right-hand side vector.
        /// </summary>
        public void ResetVector()
        {
            foreach (var elt in _rhsElements.Values)
                elt.Value = default;
        }

        /// <summary>
        /// Solves the equations using the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="solution">The solution.</param>
        public void Solve(IVector<T> solution)
            => throw new CircuitException("Cannot solve a {0}.".FormatString(this));

        /// <summary>
        /// Solves the equations using the transposed Y-matrix.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <exception cref="CircuitException">Cannot solve a {0}.".FormatString(this)</exception>
        public void SolveTransposed(IVector<T> solution)
            => throw new CircuitException("Cannot solve a {0}.".FormatString(this));
    }
}
