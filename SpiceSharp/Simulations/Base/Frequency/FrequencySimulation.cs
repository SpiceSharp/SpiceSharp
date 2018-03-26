using System;
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
        protected BehaviorList<BaseFrequencyBehavior> FrequencyBehaviors { get; private set; }

        /// <summary>
        /// Gets the complex state
        /// </summary>
        public ComplexState ComplexState { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected FrequencySimulation(Identifier name) : base(name)
        {
            ParameterSets.Add(new FrequencyConfiguration());

            // Create a complex state with shared matrix
            States.Add(new ComplexState());
        }

        /// <summary>
        /// The sweep for frequency points
        /// </summary>
        protected Sweep<double> FrequencySweep { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="frequencySweep">Sweep for the frequency points</param>
        protected FrequencySimulation(Identifier name, Sweep<double> frequencySweep) : base(name)
        {
            ParameterSets.Add(new FrequencyConfiguration(frequencySweep));

            // Create a complex state with shared matrix
            States.Add(new ComplexState());
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        /// <param name="circuit">Circuit</param>
        protected override void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));
            base.Setup(circuit);

            // Get behaviors, configurations and states
            ComplexState = States.Get<ComplexState>() ?? throw new CircuitException("No complex state found");
            FrequencyConfiguration = ParameterSets.Get<FrequencyConfiguration>() ??
                                     throw new CircuitException("No frequency configuration found");
            FrequencySweep = FrequencyConfiguration.FrequencySweep ??
                             throw new CircuitException("No frequency sweep found");

            FrequencyBehaviors = SetupBehaviors<BaseFrequencyBehavior>(circuit.Objects);
            var solver = ComplexState.Solver;
            for (int i = 0; i < FrequencyBehaviors.Count; i++)
                FrequencyBehaviors[i].GetEquationPointers(solver);
        }

        /// <summary>
        /// Execute
        /// </summary>
        protected override void Execute()
        {
            base.Execute();

            // Initialize the state
            ComplexState.Initialize(Nodes);
            ComplexState.Sparse |= ComplexState.SparseStates.AcShouldReorder;
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            for (int i = 0; i < FrequencyBehaviors.Count; i++)
                FrequencyBehaviors[i].Unsetup();
            FrequencyBehaviors = null;

            // Remove the state
            ComplexState.Destroy();
            ComplexState = null;

            // Configuration
            FrequencyConfiguration = null;
            FrequencySweep = null;

            base.Unsetup();
        }
        
        /// <summary>
        /// Calculate the AC solution
        /// </summary>
        protected void AcIterate()
        {
            var cstate = ComplexState;
            var solver = cstate.Solver;

            retry:
            cstate.IsConvergent = true;

            // Load AC
            cstate.Solver.Clear();
            for (int i = 0; i < FrequencyBehaviors.Count; i++)
                FrequencyBehaviors[i].Load(this);

            if (cstate.Sparse.HasFlag(ComplexState.SparseStates.AcShouldReorder))
            {
                solver.OrderAndFactor();
                cstate.Sparse &= ~ComplexState.SparseStates.AcShouldReorder;
            }
            else
            {
                if (!solver.Factor())
                {
                    cstate.Sparse |= ComplexState.SparseStates.AcShouldReorder;
                    goto retry;
                }
            }

            // Solve
            solver.Solve(cstate.Solution);

            // Reset values
            cstate.Solution[0] = 0.0;
        }
    }
}
