using SpiceSharp.Algebra;
using SpiceSharp.Simulations.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
namespace SpiceSharp.Simulations;

public partial class DC
{
    /// <summary>
    /// A <see cref="IComplexSimulationState"/> that throws away most functionality and
    /// memory allocation to be used for DC simulations.
    /// </summary>
    protected class ComplexSimulationState : IComplexSimulationState
    {
        private readonly SparseComplexSolver _solver;
        private readonly DenseVector<Complex> _solution;
        private readonly IVariableMap _map;
        private readonly IVariable<Complex> _gndVariable;
        private readonly HashSet<string> _variables;

        private class DummyVariableMap : IVariableMap
        {
            private readonly ComplexSimulationState _state;

            /// <inheritdoc />
            public int Count => 1;

            /// <inheritdoc />
            public int this[IVariable variable] => 0;

            /// <inheritdoc />
            public IVariable this[int index] => _state._gndVariable;

            /// <summary>
            /// Creates a new <see cref="DummyVariableMap"/>.
            /// </summary>
            /// <param name="state">The state.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="state"/> is <c>null</c>.</exception>
            public DummyVariableMap(ComplexSimulationState state)
            {
                _state = state ?? throw new ArgumentNullException(nameof(state));
            }

            /// <inheritdoc />
            public bool Contains(IVariable variable) => true;

            /// <inheritdoc />
            public IEnumerator<KeyValuePair<IVariable, int>> GetEnumerator()
            {
                yield return new KeyValuePair<IVariable, int>(_state._gndVariable, 0);
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <inheritdoc />
        public Complex Laplace => new();

        /// <inheritdoc />
        public ISparsePivotingSolver<Complex> Solver => _solver;

        /// <inheritdoc />
        public IVector<Complex> Solution => _solution;

        /// <inheritdoc />
        public IVariableMap Map => _map;

        /// <inheritdoc />
        public IVariable<Complex> this[string key]
        {
            get
            {
                if (_variables.Contains(key))
                    return _gndVariable;
                throw new KeyNotFoundException();
            }
        }

        /// <inheritdoc />
        public IEqualityComparer<string> Comparer => throw new NotImplementedException();

        /// <inheritdoc />
        public IEnumerable<string> Keys => _variables;

        /// <inheritdoc />
        public IEnumerable<IVariable<Complex>> Values
        {
            get
            {
                yield return _gndVariable;
            }
        }

        /// <inheritdoc />
        public int Count => 1;

        /// <summary>
        /// Creates a new <see cref="ComplexSimulationState"/>.
        /// </summary>
        public ComplexSimulationState()
        {
            _solver = new SparseComplexSolver();
            _gndVariable = new SolverVariable<Complex>(this, "gnd", 0, Units.Volt);
            _map = new DummyVariableMap(this);
        }

        /// <inheritdoc />
        public IVariable<Complex> GetSharedVariable(string name)
        {
            // Just map everything to ground
            return _gndVariable;
        }

        /// <inheritdoc />
        public IVariable<Complex> CreatePrivateVariable(string name, IUnit unit)
        {
            // Normally this would create a new variable, but we will simply return ground regardless
            return _gndVariable;
        }

        /// <inheritdoc />
        public void Add(string id, IVariable<Complex> variable)
            => _variables.Add(id);

        /// <inheritdoc />
        public bool ContainsKey(string key) => _variables.Contains(key);

        /// <inheritdoc />
        public bool TryGetValue(string key, out IVariable<Complex> value)
        {
            if (_variables.Contains(key))
            {
                value = _gndVariable;
                return true;
            }
            value = default;
            return false;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, IVariable<Complex>>> GetEnumerator()
        {
            foreach (string name in _variables)
                yield return new KeyValuePair<string, IVariable<Complex>>(name, _gndVariable);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
