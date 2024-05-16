using SpiceSharp.Algebra;
using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="ISparseSolver{T}"/> that only allows direct access to an element once. All subsequent calls will be
    /// through a local element.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <seealso cref="ISparseSolver{T}" />
    public partial class ParallelSolver<T> : ISparsePivotingSolver<T>
    {
        private readonly ISparsePivotingSolver<T> _parent;
        private readonly HashSet<MatrixLocation> _sharedMatrixElements = new HashSet<MatrixLocation>();
        private readonly HashSet<int> _sharedVectorElements = new HashSet<int>();
        private readonly List<BridgeElement> _bridgeElements = new List<BridgeElement>();

        /// <inheritdoc/>
        P IParameterSetCollection.GetParameterSet<P>() => _parent.GetParameterSet<P>();

        /// <inheritdoc/>
        bool IParameterSetCollection.TryGetParameterSet<P>(out P value) => _parent.TryGetParameterSet(out value);

        /// <inheritdoc/>
        IEnumerable<IParameterSet> IParameterSetCollection.ParameterSets => _parent.ParameterSets;

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">Thrown when trying to write in a parallel solver.</exception>
        public int Degeneracy { get => _parent.Degeneracy; set => throw new ArgumentException(Properties.Resources.Parallel_AccessNotSupported.FormatString(nameof(Degeneracy))); }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">Thrown when trying to write in a parallel solver.</exception>
        public int PivotSearchReduction { get => _parent.PivotSearchReduction; set => throw new ArgumentException(Properties.Resources.Parallel_AccessNotSupported.FormatString(nameof(PivotSearchReduction))); }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">Thrown when trying to write in a parallel solver.</exception>
        public bool NeedsReordering { get => _parent.NeedsReordering; set => throw new ArgumentException(Properties.Resources.Parallel_AccessNotSupported.FormatString(nameof(NeedsReordering))); }

        /// <inheritdoc/>
        public bool IsFactored => _parent.IsFactored;

        /// <inheritdoc/>
        T ISolver<T>.this[int row, int column]
        {
            get => _parent[row, column];
            set => throw new ArgumentException(Properties.Resources.Parallel_AccessNotSupported.FormatString("this[row, column]"));
        }

        /// <inheritdoc/>
        T ISolver<T>.this[MatrixLocation location]
        {
            get => _parent[location];
            set => throw new ArgumentException(Properties.Resources.Parallel_AccessNotSupported.FormatString("this[location]"));
        }

        /// <inheritdoc/>
        T ISolver<T>.this[int row]
        {
            get => _parent[row];
            set => throw new ArgumentException(Properties.Resources.Parallel_AccessNotSupported.FormatString("this[row]"));
        }

        /// <inheritdoc/>
        public int Size => _parent.Size;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelSolver{T}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public ParallelSolver(ISparsePivotingSolver<T> parent)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
        }

        /// <inheritdoc/>
        void IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>.Precondition(PreconditioningMethod<ISparseMatrix<T>, ISparseVector<T>, T> method)
            => throw new ArgumentException(Properties.Resources.Parallel_AccessNotSupported.FormatString(nameof(IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>.Precondition)));

        /// <inheritdoc/>
        void ISolver<T>.Clear()
        {
            _sharedMatrixElements.Clear();
            _bridgeElements.Clear();
        }

        /// <inheritdoc/>
        bool ISolver<T>.Factor()
            => throw new SpiceSharpException(Properties.Resources.Parallel_AccessNotSupported.FormatString(nameof(ISolver<T>.Factor)));

        /// <inheritdoc/>
        int IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>.OrderAndFactor()
            => throw new SpiceSharpException(Properties.Resources.Parallel_AccessNotSupported.FormatString(nameof(IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>.OrderAndFactor)));

        /// <inheritdoc/>
        public Element<T> FindElement(MatrixLocation location)
        {
            var elt = _parent.FindElement(location);
            if (elt == null)
                return null;
            if (_sharedMatrixElements.Contains(location))
            {
                // We only allow one reference to an element
                var bridge = new BridgeElement(elt);
                _bridgeElements.Add(bridge);
                return bridge;
            }
            else
                _sharedMatrixElements.Add(location);
            return elt;
        }

        /// <inheritdoc/>
        public Element<T> FindElement(int row)
        {
            var elt = _parent.FindElement(row);
            if (elt == null)
                return null;
            if (_sharedVectorElements.Contains(row))
            {
                var bridge = new BridgeElement(elt);
                _bridgeElements.Add(bridge);
                return bridge;
            }
            else
                _sharedVectorElements.Add(row);
            return elt;
        }

        /// <inheritdoc/>
        public Element<T> GetElement(MatrixLocation location)
        {
            var elt = _parent.GetElement(location);
            if (_sharedMatrixElements.Contains(location))
            {
                var bridge = new BridgeElement(elt);
                _bridgeElements.Add(bridge);
                return bridge;
            }
            else
                _sharedMatrixElements.Add(location);
            return elt;
        }

        /// <inheritdoc/>
        public bool RemoveElement(MatrixLocation location) =>
            throw new SpiceSharpException(Properties.Resources.Parallel_AccessNotSupported.FormatString(nameof(IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>.OrderAndFactor)));

        /// <inheritdoc/>
        public Element<T> GetElement(int row)
        {
            var elt = _parent.GetElement(row);
            if (_sharedVectorElements.Contains(row))
            {
                var bridge = new BridgeElement(elt);
                _bridgeElements.Add(bridge);
                return bridge;
            }
            else
                _sharedVectorElements.Add(row);
            return elt;
        }

        /// <inheritdoc/>
        public bool RemoveElement(int index)
            => throw new SpiceSharpException(Properties.Resources.Parallel_AccessNotSupported.FormatString(nameof(IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>.OrderAndFactor)));

        /// <summary>
        /// Clears all matrix and vector elements.
        /// </summary>
        public void Reset()
        {
            foreach (var bridge in _bridgeElements)
                bridge.Value = default;
        }

        /// <inheritdoc/>
        void ISolver<T>.ForwardSubstitute(IVector<T> solution)
            => throw new SpiceSharpException(Properties.Resources.Parallel_AccessNotSupported.FormatString(nameof(ISolver<T>.ForwardSubstitute)));

        /// <inheritdoc />
        void ISolver<T>.BackwardSubstitute(IVector<T> solution)
            => throw new SpiceSharpException(Properties.Resources.Parallel_AccessNotSupported.FormatString(nameof(ISolver<T>.BackwardSubstitute)));

        /// <inheritdoc />
        T ISolver<T>.ComputeDegenerateContribution(int index)
            => _parent.ComputeDegenerateContribution(index);
        /// <inheritdoc/>
        void ISolver<T>.ForwardSubstituteTransposed(IVector<T> solution)
            => throw new SpiceSharpException(Properties.Resources.Parallel_AccessNotSupported.FormatString(nameof(ISolver<T>.ForwardSubstituteTransposed)));

        /// <inheritdoc />
        void ISolver<T>.BackwardSubstituteTransposed(IVector<T> solution)
            => throw new SpiceSharpException(Properties.Resources.Parallel_AccessNotSupported.FormatString(nameof(ISolver<T>.BackwardSubstituteTransposed)));

        /// <inheritdoc />
        T ISolver<T>.ComputeDegenerateContributionTransposed(int index)
            => _parent.ComputeDegenerateContributionTransposed(index);

        /// <inheritdoc/>
        MatrixLocation IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>.InternalToExternal(MatrixLocation location)
            => _parent.InternalToExternal(location);

        /// <inheritdoc/>
        MatrixLocation IPivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>.ExternalToInternal(MatrixLocation location)
            => _parent.ExternalToInternal(location);

        /// <summary>
        /// Applies all bridge elements.
        /// </summary>
        public void Apply()
        {
            foreach (var bridge in _bridgeElements)
                bridge.Apply();
        }

        /// <inheritdoc/>
        public void SetParameter<P>(string name, P value) => _parent.SetParameter(name, value);

        /// <inheritdoc/>
        public bool TrySetParameter<P>(string name, P value) => _parent.TrySetParameter(name, value);

        /// <inheritdoc/>
        public P GetProperty<P>(string name) => _parent.GetProperty<P>(name);

        /// <inheritdoc/>
        public bool TryGetProperty<P>(string name, out P value) => _parent.TryGetProperty(name, out value);

        /// <inheritdoc/>
        public Action<P> CreateParameterSetter<P>(string name) => _parent.CreateParameterSetter<P>(name);

        /// <inheritdoc/>
        public Func<P> CreatePropertyGetter<P>(string name) => _parent.CreatePropertyGetter<P>(name);
    }
}