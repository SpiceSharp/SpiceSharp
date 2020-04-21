using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Solve;
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
    public partial class ParallelSolver<T> : ISparsePivotingSolver<T> where T : IFormattable
    {
        // TODO: More verbosity for exceptions in the whole class.
        private readonly ISparsePivotingSolver<T> _parent;
        private readonly HashSet<int> _shared = new HashSet<int>();
        private readonly List<BridgeElement> _bridgeElements = new List<BridgeElement>();

        P IParameterized.GetParameterSet<P>() => _parent.GetParameterSet<P>();
        bool IParameterized.TryGetParameterSet<P>(out P value) => _parent.TryGetParameterSet(out value);
        IEnumerable<IParameterSet> IParameterized.ParameterSets => _parent.ParameterSets;
        ISolver<T> IImportParameterSet<ISolver<T>>.SetParameter(string name) { _parent.SetParameter(name); return this; }
        ISolver<T> IImportParameterSet<ISolver<T>>.SetParameter<P>(string name, P value) { _parent.SetParameter(name, value); return this; }
        void IImportParameterSet.SetParameter(string name) => _parent.SetParameter(name);
        void IImportParameterSet.SetParameter<P>(string name, P value) => _parent.SetParameter(name, value);
        bool IImportParameterSet.TrySetParameter(string name) => _parent.TrySetParameter(name);
        bool IImportParameterSet.TrySetParameter<P>(string name, P value) => _parent.TrySetParameter(name, value);
        Action<P> IImportParameterSet.CreateParameterSetter<P>(string name) => _parent.CreateParameterSetter<P>(name);

        /// <summary>
        /// Gets or sets the degeneracy of the matrix. For example, specifying 1 will let the solver know that one equation is
        /// expected to be linearly dependent on the others.
        /// </summary>
        /// <value>
        /// The degeneracy.
        /// </value>
        /// <exception cref="ArgumentException">Thrown when trying to write in a parallel solver.</exception>
        public int Degeneracy { get => _parent.Degeneracy; set => throw new ArgumentException(); }

        /// <summary>
        /// Gets or sets the pivot search reduction.
        /// </summary>
        /// <value>
        /// The pivot search reduction.
        /// </value>
        /// <exception cref="ArgumentException">Thrown when trying to write in a parallel solver.</exception>
        public int PivotSearchReduction { get => _parent.PivotSearchReduction; set => throw new ArgumentException(); }

        /// <summary>
        /// Gets a value indicating whether the solver needs to be reordered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the solver needs reordering; otherwise, <c>false</c>.
        /// </value>
        public bool NeedsReordering { get => _parent.NeedsReordering; set => throw new ArgumentException(); }

        /// <summary>
        /// Gets a value indicating whether this solver has been factored.
        /// A solver needs to be factored becore it can solve for a solution.
        /// </summary>
        /// <value>
        /// <c>true</c> if this solver is factored; otherwise, <c>false</c>.
        /// </value>
        public bool IsFactored => _parent.IsFactored;

        T ISolver<T>.this[int row, int column]
        {
            get => _parent[row, column];
            set => throw new ArgumentException(Properties.Resources.Parallel_AccessNotSupported.FormatString("this[row, column]"));
        }
        T ISolver<T>.this[MatrixLocation location]
        {
            get => _parent[location];
            set => throw new ArgumentException(Properties.Resources.Parallel_AccessNotSupported.FormatString("this[location]"));
        }
        T ISolver<T>.this[int row]
        {
            get => _parent[row];
            set => throw new ArgumentException(Properties.Resources.Parallel_AccessNotSupported.FormatString("this[row]"));
        }

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
        public ParallelSolver(ISparsePivotingSolver<T> parent)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
        }

        void IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>.Precondition(PreconditioningMethod<ISparseMatrix<T>, ISparseVector<T>, T> method) => throw new ArgumentException();
        void ISolver<T>.Clear()
        {
            _shared.Clear();
            _bridgeElements.Clear();
        }
        bool ISolver<T>.Factor() => throw new SpiceSharpException();
        int IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>.OrderAndFactor() => throw new SpiceSharpException();

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
        /// Clears all matrix and vector elements.
        /// </summary>
        public void Reset()
        {
            foreach (var bridge in _bridgeElements)
                bridge.Value = default;
        }

        void ISolver<T>.Solve(IVector<T> solution) => throw new SpiceSharpException();
        void ISolver<T>.SolveTransposed(IVector<T> solution) => throw new SpiceSharpException();
        MatrixLocation IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>.InternalToExternal(MatrixLocation location) => _parent.InternalToExternal(location);
        MatrixLocation IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>.ExternalToInternal(MatrixLocation location) => _parent.ExternalToInternal(location);

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