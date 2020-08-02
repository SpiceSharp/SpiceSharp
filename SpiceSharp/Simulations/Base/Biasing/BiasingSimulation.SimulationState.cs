using SpiceSharp.Algebra;
using SpiceSharp.Simulations.Variables;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    public abstract partial class BiasingSimulation
    {
        private class SimulationState : VariableDictionary<IVariable<double>>, IBiasingSimulationState
        {
            private readonly VariableMap _map;
            public IVector<double> Solution { get; private set; }
            public IVector<double> OldSolution { get; private set; }
            public IVariableMap Map => _map;
            public ISparsePivotingSolver<double> Solver { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SimulationState"/> class.
            /// </summary>
            /// <param name="solver">The solver.</param>
            /// <param name="comparer">The comparer.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="solver"/> is <c>null</c>.</exception>
            public SimulationState(ISparsePivotingSolver<double> solver, IEqualityComparer<string> comparer)
                : base(comparer)
            {
                Solver = solver.ThrowIfNull(nameof(solver));

                var gnd = new SolverVariable<double>(this, Constants.Ground, 0, Units.Volt);
                _map = new VariableMap(gnd);
                Add(Constants.Ground, gnd);
            }

            public IVariable<double> GetSharedVariable(string name)
            {
                name.ThrowIfNull(nameof(name));
                if (TryGetValue(name, out var result))
                    return result;

                // We create a private variable and then make it shared by adding it to the solved variable set
                result = CreatePrivateVariable(name, Units.Volt);
                Add(name, result);
                return result;
            }

            public IVariable<double> CreatePrivateVariable(string name, IUnit unit)
            {
                var index = _map.Count;
                var result = new SolverVariable<double>(this, name, index, unit);
                _map.Add(result, index);
                return result;
            }

            public void Setup()
            {
                // Initialize all matrices
                Solution = new DenseVector<double>(Solver.Size);
                OldSolution = new DenseVector<double>(Solver.Size);
                Solver.Reset();
            }

            public void StoreSolution()
            {
                var tmp = OldSolution;
                OldSolution = Solution;
                Solution = tmp;
            }
        }
    }
}
