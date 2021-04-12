using SpiceSharp.Behaviors;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="IBiasingUpdateBehavior"/> for a <see cref="Parallel"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingUpdateBehavior" />
    [BehaviorFor(typeof(Parallel))]
    public class BiasingUpdate : Behavior,
        IParallelBehavior,
        IBiasingUpdateBehavior
    {
        private readonly Workload _updateWorkload;
        private BehaviorList<IBiasingUpdateBehavior> _updateBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingUpdate"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public BiasingUpdate(ParallelBindingContext context)
            : base(context)
        {
            var parameters = context.GetParameterSet<Parameters>();
            if (parameters.WorkDistributors.TryGetValue(typeof(IBiasingUpdateBehavior), out var dist) && dist != null)
            {
                _updateWorkload = new Workload(dist, parameters.Entities.Count);
            }
        }

        /// <inheritdoc/>
        public virtual void FetchBehaviors(ParallelBindingContext context)
        {
            _updateBehaviors = context.GetBehaviors<IBiasingUpdateBehavior>();
            if (_updateWorkload != null)
            {
                foreach (var behavior in _updateBehaviors)
                    _updateWorkload.Actions.Add(behavior.Update);
            }
        }

        /// <inheritdoc/>
        void IBiasingUpdateBehavior.Update()
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
