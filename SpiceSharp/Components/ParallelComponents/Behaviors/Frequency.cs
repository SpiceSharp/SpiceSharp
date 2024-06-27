using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="IFrequencyBehavior" /> for a <see cref="Parallel" />.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IFrequencyBehavior" />
    public partial class Frequency : Behavior,
        IParallelBehavior,
        IFrequencyBehavior
    {
        private readonly ComplexSimulationState _state;
        private readonly Workload _loadWorkload, _initWorkload;
        private BehaviorList<IFrequencyBehavior> _frequencyBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Frequency(ParallelBindingContext context)
            : base(context)
        {
            var parameters = context.GetParameterSet<Parameters>();
            if (parameters.WorkDistributors.TryGetValue(typeof(IFrequencyBehavior), out var dist) && dist != null)
            {
                _initWorkload = new Workload(dist, parameters.Entities.Count);
                _loadWorkload = new Workload(dist, parameters.Entities.Count);
                if (context.TryGetState(out IComplexSimulationState parent))
                    context.AddLocalState<IComplexSimulationState>(_state = new ComplexSimulationState(parent));
            }
        }

        /// <inheritdoc/>
        public virtual void FetchBehaviors(ParallelBindingContext context)
        {
            _frequencyBehaviors = context.GetBehaviors<IFrequencyBehavior>();
            if (_initWorkload != null)
            {
                foreach (var behavior in _frequencyBehaviors)
                    _initWorkload.Actions.Add(behavior.InitializeParameters);
            }
            if (_loadWorkload != null)
            {
                foreach (var behavior in _frequencyBehaviors)
                    _loadWorkload.Actions.Add(behavior.Load);
            }
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
            if (_initWorkload != null)
                _initWorkload.Execute();
            else
            {
                foreach (var behavior in _frequencyBehaviors)
                    behavior.InitializeParameters();
            }
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            if (_loadWorkload != null)
            {
                _state.Reset();
                _loadWorkload.Execute();
                _state.Apply();
            }
            else
            {
                foreach (var behavior in _frequencyBehaviors)
                    behavior.Load();
            }
        }
    }
}
