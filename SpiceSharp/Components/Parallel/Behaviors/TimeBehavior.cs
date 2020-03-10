using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An <see cref="ITimeBehavior"/> for a <see cref="ParallelSimulation"/>.
    /// </summary>
    /// <seealso cref="ConvergenceBehavior" />
    /// <seealso cref="ITimeBehavior" />
    public class TimeBehavior : ConvergenceBehavior, ITimeBehavior
    {
        private readonly Workload _initWorkload;
        private readonly BehaviorList<ITimeBehavior> _timeBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public TimeBehavior(string name, ParallelSimulation simulation)
            : base(name, simulation)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<BaseParameters>();
            if (parameters.TimeInitDistributor != null)
            {
                _initWorkload = new Workload(parameters.TimeInitDistributor, simulation.EntityBehaviors.Count);
                foreach (var container in simulation.EntityBehaviors)
                {
                    if (container.TryGetValue(out ITimeBehavior time))
                        _initWorkload.Actions.Add(time.InitializeStates);
                }
            }

            _timeBehaviors = simulation.EntityBehaviors.GetBehaviorList<ITimeBehavior>();
        }

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
