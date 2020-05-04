using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="IAcceptBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IAcceptBehavior" />
    public class Accept : Behavior, 
        IAcceptBehavior
    {
        private readonly Workload _probeWorkload, _acceptWorkload;
        private readonly BehaviorList<IAcceptBehavior> _acceptBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="Accept"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public Accept(string name, ParallelSimulation simulation)
            : base(name)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<Parameters>();
            if (parameters.ProbeDistributor != null)
                _probeWorkload = new Workload(parameters.ProbeDistributor, simulation.EntityBehaviors.Count);
            if (parameters.AcceptDistributor != null)
                _acceptWorkload = new Workload(parameters.AcceptDistributor, simulation.EntityBehaviors.Count);
            if (_probeWorkload != null || _acceptWorkload != null)
            {   
                foreach (var behavior in simulation.EntityBehaviors)
                {
                    if (behavior.TryGetValue(out IAcceptBehavior accept))
                    {
                        _probeWorkload?.Actions.Add(accept.Probe);
                        _acceptWorkload?.Actions.Add(accept.Probe);
                    }
                }
            }

            // Get behaviors
            _acceptBehaviors = simulation.EntityBehaviors.GetBehaviorList<IAcceptBehavior>();
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
