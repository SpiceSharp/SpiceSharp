using SpiceSharp.Algebra;
using SpiceSharp.Simulations.Variables;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    public abstract partial class FrequencySimulation
    {
        /// <summary>
        /// A simulation state using complex numbers.
        /// </summary>
        /// <seealso cref="IComplexSimulationState" />
        protected class ComplexSimulationState : VariableDictionary<IVariable<Complex>>, IComplexSimulationState
        {
            private readonly VariableMap _map;

            /// <inheritdoc/>
            public IVector<Complex> Solution { get; private set; }

            /// <inheritdoc/>
            public Complex Laplace { get; set; } = new Complex();

            /// <inheritdoc/>
            public IVariableMap Map => _map;

            /// <inheritdoc/>
            public ISparsePivotingSolver<Complex> Solver { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComplexSimulationState"/> class.
            /// </summary>
            /// <param name="solver">The solver.</param>
            /// <param name="comparer">The comparer.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="solver"/> is <c>null</c>.</exception>
            public ComplexSimulationState(ISparsePivotingSolver<Complex> solver, IEqualityComparer<string> comparer)
                : base(comparer)
            {
                Solver = solver.ThrowIfNull(nameof(solver));

                var gnd = new SolverVariable<Complex>(this, Constants.Ground, 0, Units.Volt);
                _map = new VariableMap(gnd);
                Add(Constants.Ground, gnd);
            }

            /// <inheritdoc/>
            public IVariable<Complex> GetSharedVariable(string name)
            {
                if (TryGetValue(name, out var result))
                    return result;

                // We create a private variable and then make it shared
                result = CreatePrivateVariable(name, Units.Volt);
                Add(name, result);
                return result;
            }

            /// <inheritdoc/>
            public IVariable<Complex> CreatePrivateVariable(string name, IUnit unit)
            {
                int index = _map.Count;
                var result = new SolverVariable<Complex>(this, name, index, unit);
                _map.Add(result, index);
                return result;
            }

            /// <summary>
            /// Set up the simulation state for the simulation.
            /// </summary>
            public void Setup()
            {
                Solution = new DenseVector<Complex>(Solver.Size);
            }
        }
    }
}
