using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="IFrequencyUpdateBehavior"/> for a <see cref="Parallel"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IFrequencyUpdateBehavior" />
    public class FrequencyUpdate : Behavior,
        IParallelBehavior,
        IFrequencyUpdateBehavior
    {
        private readonly Workload _updateWorkload;
        private BehaviorList<IFrequencyUpdateBehavior> _updateBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyUpdate" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public FrequencyUpdate(ParallelBindingContext context)
            : base(context)
        {
            var parameters = context.GetParameterSet<Parameters>();
            if (parameters.WorkDistributors.TryGetValue(typeof(IFrequencyUpdateBehavior), out var dist) && dist != null)
                _updateWorkload = new Workload(dist, parameters.Entities.Count);
        }

        /// <inheritdoc/>
        public virtual void FetchBehaviors(ParallelBindingContext context)
        {
            _updateBehaviors = context.GetBehaviors<IFrequencyUpdateBehavior>();
            if (_updateWorkload != null)
            {
                foreach (var behavior in _updateBehaviors)
                    _updateWorkload.Actions.Add(behavior.Update);
            }
        }

        /// <inheritdoc/>
        void IFrequencyUpdateBehavior.Update()
        {
            if (_updateWorkload != null)
                _updateWorkload.Execute();
            else
            {
                foreach (var behavior in _updateBehaviors)
                    behavior.Update();
            }
        }
    }
}
