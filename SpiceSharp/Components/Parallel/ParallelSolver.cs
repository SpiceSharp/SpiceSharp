using SpiceSharp.Algebra;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An <see cref="ISparseSolver{T}"/> that only allows direct access to an element once. All subsequent calls will be
    /// through a local element.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <seealso cref="ISparseSolver{T}" />
    public partial class ParallelSolver<T> : ISparseSolver<T> where T : IFormattable
    {
        private readonly ISparseSolver<T> _parent;
        private readonly HashSet<int> _shared = new HashSet<int>();
        private readonly List<BridgeElement> _bridgeElements = new List<BridgeElement>();

        /// <summary>
        /// Gets or sets the degeneracy of the matrix. For example, specifying 1 will let the solver know that one equation is
        /// expected to be linearly dependent on the others.
        /// </summary>
        /// <value>
        /// The degeneracy.
        /// </value>
        /// <exception cref="ArgumentException">Thrown when trying to write.</exception>
        public int Degeneracy { get => _parent.Degeneracy; set => throw new ArgumentException(); }

        /// <summary>
        /// Gets or sets the region for reordering the matrix. For example, specifying 1 will avoid a pivot from being chosen from
        /// the last row or column.
        /// </summary>
        /// <value>
        /// The pivot search reduction.
        /// </value>
        /// <exception cref="ArgumentException">Thrown when trying to write.</exception>
        public int PivotSearchReduction { get => _parent.PivotSearchReduction; set => throw new ArgumentException(); }

        /// <summary>
        /// Gets the size of the matrix and right-hand side vector.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int Size => _parent.Size;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelSolver{T}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public ParallelSolver(ISparseSolver<T> parent)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
        }

        /// <summary>
        /// Clears the solver of any elements. The size of the solver becomes 0.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when executed.</exception>
        public void Clear()
        {
            _shared.Clear();
            _bridgeElements.Clear();
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
        /// This method can save time when factoring similar matrices in succession.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the factoring was successful; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="SpiceSharpException">Thrown when executed.</exception>
        public bool Factor()
        {
            throw new SpiceSharpException();
        }

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
            var elt = _parent.FindElement(row, column);
            if (elt == null)
                return null;
            var index = row * Size + column;
            if (_shared.Contains(index))
            {
                // We only allow one reference to an element
                var bridge = new BridgeElement(elt);
                _bridgeElements.Add(bridge);
                return bridge;
            }
            else
                _shared.Contains(index);
            return elt;
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
            var elt = _parent.FindElement(row);
            if (elt == null)
                return null;
            if (_shared.Contains(row))
            {
                var bridge = new BridgeElement(elt);
                _bridgeElements.Add(bridge);
                return bridge;
            }
            else
                _shared.Add(row);
            return elt;
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
            var elt = _parent.GetElement(row, column);
            var index = row * Size + column;
            if (_shared.Contains(index))
            {
                var bridge = new BridgeElement(elt);
                _bridgeElements.Add(bridge);
                return bridge;
            }
            else
                _shared.Add(index);
            return elt;
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
            var elt = _parent.GetElement(row);
            if (_shared.Contains(row))
            {
                var bridge = new BridgeElement(elt);
                _bridgeElements.Add(bridge);
                return bridge;
            }
            else
                _shared.Add(row);
            return elt;
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
        /// This method will reorder the matrix as it sees fit.
        /// </summary>
        /// <returns>
        /// The number of rows that were successfully eliminated.
        /// </returns>
        /// <exception cref="SpiceSharpException">Thrown when executed.</exception>
        public int OrderAndFactor()
        {
            throw new SpiceSharpException();
        }

        /// <summary>
        /// Preconditions the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <exception cref="SpiceSharpException">Thrown when executed.</exception>
        public void Precondition(PreconditionMethod<T> method)
        {
            throw new SpiceSharpException();
        }

        /// <summary>
        /// Clears all matrix and vector elements.
        /// </summary>
        public void Reset()
        {
            foreach (var bridge in _bridgeElements)
                bridge.Value = default;
        }

        /// <summary>
        /// Resets the matrix.
        /// </summary>
        /// <exception cref="SpiceSharpException">Thrown when executed.</exception>
        public void ResetMatrix()
        {
            throw new SpiceSharpException();
        }

        /// <summary>
        /// Resets the right-hand side vector.
        /// </summary>
        /// <exception cref="SpiceSharpException">Thrown when executed.</exception>
        public void ResetVector()
        {
            throw new SpiceSharpException();
        }

        /// <summary>
        /// Solves the equations using the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <exception cref="SpiceSharpException">Thrown when executed.</exception>
        public void Solve(IVector<T> solution)
        {
            throw new SpiceSharpException();
        }

        /// <summary>
        /// Solves the equations using the transposed Y-matrix.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <exception cref="SpiceSharpException">Thrown when executed.</exception>
        public void SolveTransposed(IVector<T> solution)
        {
            throw new SpiceSharpException();
        }

        /// <summary>
        /// Applies all bridge elements.
        /// </summary>
        public void Apply()
        {
            foreach (var bridge in _bridgeElements)
                bridge.Apply();
        }
    }
}