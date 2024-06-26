using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="ITimeBehavior"/> for a <see cref="Parallel"/>.
    /// </summary>
    /// <seealso cref="Convergence" />
    /// <seealso cref="ITimeBehavior" />
    public class Time : Behavior,
        IParallelBehavior,
        ITimeBehavior
    {
        private readonly Workload _initWorkload;
        private BehaviorList<ITimeBehavior> _timeBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="Time" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Time(ParallelBindingContext context)
            : base(context)
        {
            var parameters = context.GetParameterSet<Parameters>();
            if (parameters.WorkDistributors.TryGetValue(typeof(ITimeBehavior), out var dist) && dist != null)
                _initWorkload = new Workload(dist, parameters.Entities.Count);
        }

        /// <inheritdoc />
        public void FetchBehaviors(ParallelBindingContext context)
        {
            _timeBehaviors = context.GetBehaviors<ITimeBehavior>();
            if (_initWorkload != null)
            {
                foreach (var behavior in _timeBehaviors)
                    _initWorkload.Actions.Add(behavior.InitializeStates);
            }
        }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            if (_initWorkload != null)
                _initWorkload.Execute();
            else
            {
                foreach (var behavior in _timeBehaviors)
                    behavior.InitializeStates();
            }
        }
    }
}
