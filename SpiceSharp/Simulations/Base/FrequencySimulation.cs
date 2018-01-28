using System;
using System.Collections.Generic;
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
        protected FrequencySimulation(Identifier name) : base(name)
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
        protected FrequencySimulation(Identifier name, string steptype, int n, double start, double stop) : base(name)
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
        /// Gets all frequency points
        /// </summary>
        protected IEnumerable<double> Frequencies
        {
            get
            {
                double freqdelta;
                int n;
                var freqconfig = FrequencyConfiguration;

                // Calculate the step
                switch (freqconfig.StepType)
                {
                    case StepTypes.Decade:
                        freqdelta = Math.Exp(Math.Log(10.0) / freqconfig.NumberSteps);
                        n = (int)Math.Floor(Math.Log(freqconfig.StopFreq / freqconfig.StartFreq) / Math.Log(freqdelta) + 0.25) + 1;
                        break;

                    case StepTypes.Octave:
                        freqdelta = Math.Exp(Math.Log(2.0) / freqconfig.NumberSteps);
                        n = (int)Math.Floor(Math.Log(freqconfig.StopFreq / freqconfig.StartFreq) / Math.Log(freqdelta) + 0.25) + 1;
                        break;

                    case StepTypes.Linear:
                        if (freqconfig.NumberSteps > 1)
                        {
                            freqdelta = (freqconfig.StopFreq - freqconfig.StartFreq) / (freqconfig.NumberSteps - 1);
                            n = freqconfig.NumberSteps;
                        }
                        else
                        {
                            freqdelta = double.PositiveInfinity;
                            n = 1;
                        }
                        break;

                    default:
                        throw new CircuitException("Invalid step type");
                }

                // Iterate through the frequency points
                double freq = freqconfig.StartFreq;
                for (int i = 0; i < n; i++)
                {
                    // Use the frequency point
                    yield return freq;

                    // Go to the next frequency
                    switch (freqconfig.StepType)
                    {
                        case StepTypes.Decade:
                        case StepTypes.Octave:
                            freq = freq * freqdelta;
                            break;

                        case StepTypes.Linear:
                            freq = freqconfig.StartFreq + i * freqdelta;
                            break;
                    }
                }
            }
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
            matrix.Solve(state.ComplexRhs, state.ComplexRhs);

            // Reset values
            state.ComplexRhs[0] = 0.0;
            state.ComplexSolution[0] = 0.0;

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
                return (State state) => state.ComplexSolution[node];
            int refnode = Circuit.Nodes[neg].Index;
            return (State state) => state.ComplexSolution[node] - state.ComplexSolution[refnode];
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
