using System.Collections.ObjectModel;
using SpiceSharp.Diagnostics;
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
        protected override void Setup()
        {
            base.Setup();

            // Get behaviors, configurations and states
            ComplexState = States.Get<ComplexState>() ?? throw new CircuitException("No complex state found");
            FrequencyConfiguration = ParameterSets.Get<FrequencyConfiguration>() ?? throw new CircuitException("No frequency configuration found");
            FrequencySweep = FrequencyConfiguration.FrequencySweep ?? throw new CircuitException("No frequency sweep found");

            FrequencyBehaviors = SetupBehaviors<FrequencyBehavior>();
            var solver = ComplexState.Solver;
            foreach (var behavior in FrequencyBehaviors)
                behavior.GetEquationPointers(solver);
        }

        /// <summary>
        /// Execute
        /// </summary>
        protected override void Execute()
        {
            base.Execute();

            // Initialize the state
            ComplexState.Initialize(Circuit.Nodes);
            ComplexState.Sparse |= ComplexState.SparseStates.ACShouldReorder;
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
        protected void ACIterate()
        {
            var cstate = ComplexState;
            var solver = cstate.Solver;

            retry:
            cstate.IsConvergent = true;

            // Load AC
            cstate.Solver.Clear();
            foreach (var behavior in FrequencyBehaviors)
                behavior.Load(this);

            if (cstate.Sparse.HasFlag(ComplexState.SparseStates.ACShouldReorder))
            {
                solver.OrderAndFactor();
                cstate.Sparse &= ~ComplexState.SparseStates.ACShouldReorder;
            }
            else
            {
                if (!solver.Factor())
                {
                    cstate.Sparse |= ComplexState.SparseStates.ACShouldReorder;
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
