using System;
using System.Numerics;
using System.Collections.ObjectModel;
using SpiceSharp.Diagnostics;
using SpiceSharp.Sparse;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A base class for frequency-dependent analysis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FrequencySimulation : BaseSimulation
    {
        /// <summary>
        /// Gets the currently active frequency configuration
        /// </summary>
        public FrequencyConfiguration FrequencyConfiguration { get; protected set; }

        /// <summary>
        /// Private variables
        /// </summary>
        protected Collection<FrequencyBehavior> FrequencyBehaviors { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencySimulation(Identifier name) : base(name)
        {
            Parameters.Add(new FrequencyConfiguration());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Identifier</param>
        /// <param name="steptype">Step type</param>
        /// <param name="n">Number of steps</param>
        /// <param name="start">Starting frequency</param>
        /// <param name="stop">Final frequency</param>
        public FrequencySimulation(Identifier name, string steptype, int n, double start, double stop) : base(name)
        {
            Parameters.Add(new FrequencyConfiguration(steptype, n, start, stop));
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        protected override void Setup()
        {
            base.Setup();

            // Get behaviors
            FrequencyBehaviors = SetupBehaviors<FrequencyBehavior>();

            // Setup AC behaviors and configurations
            FrequencyConfiguration = Parameters.Get<FrequencyConfiguration>();
            var matrix = State.Matrix;
            foreach (var behavior in FrequencyBehaviors)
                behavior.GetMatrixPointers(matrix);
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            foreach (var behavior in FrequencyBehaviors)
                behavior.Unsetup();
            FrequencyBehaviors.Clear();
            FrequencyBehaviors = null;

            base.Unsetup();
        }

        /// <summary>
        /// Calculate the AC solution
        /// </summary>
        /// <param name="circuit">Circuit</param>
        protected void AcIterate(Circuit circuit)
        {
            var state = State;
            var matrix = state.Matrix;
            matrix.Complex = true;

            // Initialize the circuit
            if (!state.Initialized)
                state.Initialize(circuit);

            retry:
            state.IsCon = true;

            // Load AC
            state.Clear();
            foreach (var behavior in FrequencyBehaviors)
                behavior.Load(this);

            if (state.Sparse.HasFlag(State.SparseFlags.NIACSHOULDREORDER))
            {
                var error = matrix.Reorder(state.PivotAbsTol, state.PivotRelTol);
                state.Sparse &= ~State.SparseFlags.NIACSHOULDREORDER;
                if (error != SparseError.Okay)
                    throw new CircuitException("Sparse matrix exception: " + SparseUtilities.ErrorMessage(state.Matrix, "AC"));
            }
            else
            {
                var error = matrix.Factor();
                if (error != 0)
                {
                    if (error == SparseError.Singular)
                    {
                        state.Sparse |= State.SparseFlags.NIACSHOULDREORDER;
                        goto retry;
                    }
                    throw new CircuitException("Sparse matrix exception: " + SparseUtilities.ErrorMessage(state.Matrix, "AC"));
                }
            }

            // Solve
            matrix.Solve(state.Rhs, state.iRhs);

            // Reset values
            state.Rhs[0] = 0.0;
            state.iRhs[0] = 0.0;
            state.Solution[0] = 0.0;
            state.iSolution[0] = 0.0;

            // Store them in the solution
            state.StoreComplexSolution();
        }

        /// <summary>
        /// Create an export method for this type of simulation
        /// The simulation will determine which export method is returned if multiple behaviors implement a export method by the same name
        /// </summary>
        /// <param name="name">The identifier of the entity</param>
        /// <param name="property">The parameter name</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(Identifier name, string property)
        {
            var eb = pool.GetEntityBehaviors(name) ?? throw new CircuitException("{0}: Could not find behaviors of {1}".FormatString(Name, name));

            // Most logical place to look for frequency analysis: AC behaviors
            var export = eb.Get<FrequencyBehavior>()?.CreateExport(property);

            // Next most logical place is the LoadBehavior
            if (export == null)
                export = eb.Get<LoadBehavior>()?.CreateExport(property);
            return export;
        }

        /// <summary>
        /// Create an export method
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="property">Property</param>
        /// <returns></returns>
        public Func<State, Complex> CreateAcExport(Identifier name, string property)
        {
            var eb = pool.GetEntityBehaviors(name) ?? throw new CircuitException("{0}: Could not find behaviors of {1}".FormatString(Name, name));

            // Only AC behaviors implement these export methods
            return eb.Get<FrequencyBehavior>()?.CreateAcExport(property);
        }

        /// <summary>
        /// Create an export method for this type of simulation 
        /// </summary>
        /// <param name="pos">Positive node</param>
        /// <param name="neg">Negative node</param>
        /// <returns></returns>
        public virtual Func<State, Complex> CreateAcVoltageExport(Identifier pos, Identifier neg)
        {
            int node = Circuit.Nodes[pos].Index;
            if (neg == null)
                return (State state) => new Complex(state.Solution[node], state.iSolution[node]);
            int refnode = Circuit.Nodes[neg].Index;
            return (State state) => new Complex(state.Solution[node] - state.Solution[refnode], state.iSolution[node] - state.iSolution[refnode]);
        }

        /// <summary>
        /// Create an export method for this type of simulation
        /// </summary>
        /// <param name="pos">Positive node</param>
        /// <returns></returns>
        public virtual Func<State, Complex> CreateAcVoltageExport(Identifier pos)
        {
            return CreateAcVoltageExport(pos, null);
        }
    }
}
