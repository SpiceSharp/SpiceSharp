using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// An <see cref="IBiasingBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IBiasingBehavior" />
    /// <seealso cref="IConvergenceBehavior"/>
    [BehaviorFor(typeof(Subcircuit))]
    public partial class Biasing : SubcircuitBehavior<IBiasingBehavior>,
        IBiasingBehavior,
        IConvergenceBehavior
    {
        private BehaviorList<IConvergenceBehavior> _convergenceBehaviors;
        private readonly LocalSimulationState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Biasing(SubcircuitBindingContext context)
            : base(context)
        {
            var parameters = context.GetParameterSet<Parameters>();
            var parent = context.GetState<IBiasingSimulationState>();
            if (parameters.LocalSolver)
            {
                _state = new LocalSimulationState(Name, parent, new SparseRealSolver());
                context.AddLocalState<IBiasingSimulationState>(_state);
            }
            else
                context.AddLocalState<IBiasingSimulationState>(new FlatSimulationState(Name, parent, context.Bridges));
        }

        /// <inheritdoc/>
        public override void FetchBehaviors(SubcircuitBindingContext context)
        {
            base.FetchBehaviors(context);
            _convergenceBehaviors = context.GetBehaviors<IConvergenceBehavior>();
            _state?.Initialize(context.Bridges);
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            if (_state != null)
            {
                _state.Update();
                do
                {
                    _state.Solver.Reset();
                    LoadBehaviors();
                }
                while (!_state.Apply());
            }
            else
                LoadBehaviors();
        }

        /// <summary>
        /// Loads the behaviors.
        /// </summary>
        protected virtual void LoadBehaviors()
        {
            foreach (var behavior in Behaviors)
                behavior.Load();
        }

        /// <inheritdoc/>
        bool IConvergenceBehavior.IsConvergent()
        {
            _state?.Update();
            var result = true;
            foreach (var behavior in _convergenceBehaviors)
                result &= behavior.IsConvergent();
            return result;
        }
    }
}
