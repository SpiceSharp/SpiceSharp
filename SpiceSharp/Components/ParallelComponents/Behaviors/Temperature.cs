using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="ITemperatureBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="ITemperatureBehavior" />
    public class Temperature : Behavior,
        ITemperatureBehavior,
        IParallelBehavior
    {
        private readonly Workload _workload;
        private BehaviorList<ITemperatureBehavior> _behaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="Temperature" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Temperature(ParallelBindingContext context)
            : base(context)
        {
            var parameters = context.GetParameterSet<Parameters>();
            if (parameters.WorkDistributors.TryGetValue(typeof(ITemperatureBehavior), out var dist))
                _workload = new Workload(dist, parameters.Entities.Count);
        }

        /// <inheritdoc/>
        public virtual void FetchBehaviors(ParallelBindingContext context)
        {
            _behaviors = context.GetBehaviors<ITemperatureBehavior>();
            if (_workload != null)
            {
                foreach (var behavior in _behaviors)
                    _workload.Actions.Add(behavior.Temperature);
            }
        }

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature()
        {
            if (_workload != null)
                _workload.Execute();
            else
            {
                foreach (var behavior in _behaviors)
                    behavior.Temperature();
            }
        }
    }
}
