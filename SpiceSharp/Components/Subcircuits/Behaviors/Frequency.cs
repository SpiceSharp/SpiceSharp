using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// An <see cref="IFrequencyBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IFrequencyBehavior" />
    [BehaviorFor(typeof(Subcircuit))]
    public partial class Frequency : SubcircuitBehavior<IFrequencyBehavior>,
        IFrequencyBehavior,
        IFrequencyUpdateBehavior
    {
        private readonly LocalSimulationState _state;

        /// <summary>
        /// Gets the frequency update behaviors in the subcircuit.
        /// </summary>
        protected BehaviorList<IFrequencyUpdateBehavior> UpdateBehaviors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Frequency(SubcircuitBindingContext context)
            : base(context)
        {
            var parameters = context.GetParameterSet<Parameters>();
            UpdateBehaviors = context.GetBehaviors<IFrequencyUpdateBehavior>();
            var parent = context.GetState<IComplexSimulationState>();
            if (parameters.LocalSolver)
            {
                _state = new LocalSimulationState(Name, parent, new SparseComplexSolver());
                context.AddLocalState<IComplexSimulationState>(_state);
            }
            else
                context.AddLocalState<IComplexSimulationState>(new FlatSimulationState(Name, parent, context.Bridges));
        }

        /// <inheritdoc/>
        public override void FetchBehaviors(SubcircuitBindingContext context)
        {
            base.FetchBehaviors(context);
            _state?.Initialize(context.Bridges);
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
            foreach (var behavior in Behaviors)
                behavior.InitializeParameters();
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            if (_state != null)
            {
                _state.Update();
                do
                {
                    _state.IsConvergent = true;
                    _state.Solver.Reset();
                    foreach (var behavior in Behaviors)
                        behavior.Load();
                }
                while (!_state.Apply());
            }
            else
            {
                foreach (var behavior in Behaviors)
                    behavior.Load();
            }
        }

        /// <inheritdoc />
        public void Update()
        {
            _state?.Update();
            foreach (var behavior in UpdateBehaviors)
                behavior.Update();
        }
    }
}
