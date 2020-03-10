using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An <see cref="IBiasingUpdateBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingUpdateBehavior" />
    public class BiasingUpdateBehavior : Behavior, IBiasingUpdateBehavior
    {
        private readonly Workload _updateWorkload;
        private readonly BehaviorList<IBiasingUpdateBehavior> _updateBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingUpdateBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="simulation">The parallel simulation.</param>
        public BiasingUpdateBehavior(string name, ParallelSimulation simulation)
            : base(name)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<BaseParameters>();
            if (parameters.UpdateDistributor != null)
            {
                _updateWorkload = new Workload(parameters.UpdateDistributor, simulation.EntityBehaviors.Count);
                foreach (var behavior in simulation.EntityBehaviors)
                {
                    if (behavior.TryGetValue(out IBiasingUpdateBehavior update))
                        _updateWorkload.Actions.Add(update.Update);
                }
            }

            // Get all behaviors
            _updateBehaviors = simulation.EntityBehaviors.GetBehaviorList<IBiasingUpdateBehavior>();
        }

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
