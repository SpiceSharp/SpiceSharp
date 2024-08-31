using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="IAcceptBehavior"/> for a <see cref="Parallel"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IAcceptBehavior" />
    public class Accept : Behavior,
        IParallelBehavior,
        IAcceptBehavior
    {
        private readonly Workload _probeWorkload, _acceptWorkload;
        private BehaviorList<IAcceptBehavior> _acceptBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="Accept" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Accept(ParallelBindingContext context)
            : base(context)
        {
            var parameters = context.GetParameterSet<Parameters>();
            if (parameters.WorkDistributors.TryGetValue(typeof(IAcceptBehavior), out var dist))
            {
                _probeWorkload = new Workload(dist, parameters.Entities.Count);
                _acceptWorkload = new Workload(dist, parameters.Entities.Count);
            }
        }

        /// <inheritdoc/>
        public virtual void FetchBehaviors(ParallelBindingContext context)
        {
            _acceptBehaviors = context.GetBehaviors<IAcceptBehavior>();
            if (_acceptWorkload != null)
            {
                foreach (var behavior in _acceptBehaviors)
                    _acceptWorkload.Actions.Add(behavior.Accept);
            }
            if (_probeWorkload != null)
            {
                foreach (var behavior in _acceptBehaviors)
                    _probeWorkload.Actions.Add(behavior.Probe);
            }
        }

        /// <inheritdoc/>
        void IAcceptBehavior.Probe()
        {
            if (_probeWorkload != null)
                _probeWorkload.Execute();
            else
            {
                foreach (var behavior in _acceptBehaviors)
                    behavior.Probe();
            }
        }

        /// <inheritdoc/>
        void IAcceptBehavior.Accept()
        {
            if (_acceptWorkload != null)
                _acceptWorkload.Execute();
            else
            {
                foreach (var behavior in _acceptBehaviors)
                    behavior.Accept();
            }
        }
    }
}
