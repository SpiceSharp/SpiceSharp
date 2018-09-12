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
        private BehaviorList<BaseFrequencyBehavior> _frequencyBehaviors;
        private LoadStateEventArgs _loadStateEventArgs;

        /// <summary>
        /// Event called before loading the frequency behaviors
        /// </summary>
        public event EventHandler<LoadStateEventArgs> BeforeFrequencyLoad;

        /// <summary>
        /// Event called after loading the frequency behaviors
        /// </summary>
        public event EventHandler<LoadStateEventArgs> AfterFrequencyLoad; 

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

            // Create the state for complex numbers
            ComplexState = new ComplexState();
            _loadStateEventArgs = new LoadStateEventArgs(ComplexState);
            States.Add(ComplexState);

            // Get behaviors, configurations and states
            FrequencyConfiguration = ParameterSets.Get<FrequencyConfiguration>() ??
                                     throw new CircuitException("No frequency configuration found");
            FrequencySweep = FrequencyConfiguration.FrequencySweep ??
                             throw new CircuitException("No frequency sweep found");

            _frequencyBehaviors = SetupBehaviors<BaseFrequencyBehavior>(circuit.Objects);
            var solver = ComplexState.Solver;
            for (var i = 0; i < _frequencyBehaviors.Count; i++)
                _frequencyBehaviors[i].GetEquationPointers(solver);

            ComplexState.Setup(Nodes);
        }

        /// <summary>
        /// Execute
        /// </summary>
        protected override void Execute()
        {
            base.Execute();

            // Initialize the state
            ComplexState.Sparse |= ComplexState.SparseStates.AcShouldReorder;
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            for (var i = 0; i < _frequencyBehaviors.Count; i++)
                _frequencyBehaviors[i].Unsetup(this);
            _frequencyBehaviors = null;

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
            FrequencyLoad();

            if ((cstate.Sparse & ComplexState.SparseStates.AcShouldReorder) != 0) //cstate.Sparse.HasFlag(ComplexState.SparseStates.AcShouldReorder))
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

        /// <summary>
        /// Call event before loading frequency behaviors
        /// </summary>
        protected virtual void OnBeforeFrequencyLoad(LoadStateEventArgs args) => BeforeFrequencyLoad?.Invoke(this, args);

        /// <summary>
        /// Call event after loading frequency behaviors
        /// </summary>
        protected virtual void OnAfterFrequencyLoad(LoadStateEventArgs args) => AfterFrequencyLoad?.Invoke(this, args);

        /// <summary>
        /// Initialize all AC parameters
        /// </summary>
        protected void InitializeAcParameters()
        {
            RealState.Domain = RealState.DomainType.Frequency;
            Load();
            for (var i = 0; i < _frequencyBehaviors.Count; i++)
            {
                _frequencyBehaviors[i].Load(this);
                _frequencyBehaviors[i].InitializeParameters(this);
            }
        }

        /// <summary>
        /// Load AC behaviors
        /// </summary>
        protected void FrequencyLoad()
        {
            var cstate = ComplexState;

            OnBeforeFrequencyLoad(_loadStateEventArgs);

            cstate.Solver.Clear();
            LoadFrequencyBehaviors();

            OnAfterFrequencyLoad(_loadStateEventArgs);
        }

        /// <summary>
        /// Load frequency behaviors
        /// </summary>
        protected virtual void LoadFrequencyBehaviors()
        {
            for (var i = 0; i < _frequencyBehaviors.Count; i++)
                _frequencyBehaviors[i].Load(this);
        }
    }
}
