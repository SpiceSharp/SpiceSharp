using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An <see cref="IFrequencyUpdateBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IFrequencyUpdateBehavior" />
    public class FrequencyUpdateBehavior : Behavior, IFrequencyUpdateBehavior
    {
        private readonly Workload _updateWorkload;
        private readonly BehaviorList<IFrequencyUpdateBehavior> _updateBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyUpdateBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="simulation">The parallel simulation.</param>
        public FrequencyUpdateBehavior(string name, ParallelSimulation simulation)
            : base(name)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<BaseParameters>();
            if (parameters.UpdateDistributor != null)
            {
                _updateWorkload = new Workload(parameters.UpdateDistributor, simulation.EntityBehaviors.Count);
                foreach (var behavior in simulation.EntityBehaviors)
                {
                    if (behavior.TryGetValue(out IFrequencyUpdateBehavior update))
                        _updateWorkload.Actions.Add(update.Update);
                }
            }

            // Get all behaviors
            _updateBehaviors = simulation.EntityBehaviors.GetBehaviorList<IFrequencyUpdateBehavior>();
        }

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
