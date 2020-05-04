using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="IBiasingUpdateBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingUpdateBehavior" />
    public class BiasingUpdate : Behavior, 
        IBiasingUpdateBehavior
    {
        private readonly Workload _updateWorkload;
        private readonly BehaviorList<IBiasingUpdateBehavior> _updateBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingUpdate"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="simulation">The parallel simulation.</param>
        public BiasingUpdate(string name, ParallelSimulation simulation)
            : base(name)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<Parameters>();
            if (parameters.BiasUpdateDistributor != null)
            {
                _updateWorkload = new Workload(parameters.BiasUpdateDistributor, simulation.EntityBehaviors.Count);
                foreach (var behavior in simulation.EntityBehaviors)
                {
                    if (behavior.TryGetValue(out IBiasingUpdateBehavior update))
                        _updateWorkload.Actions.Add(update.Update);
                }
            }

            // Get all behaviors
            _updateBehaviors = simulation.EntityBehaviors.GetBehaviorList<IBiasingUpdateBehavior>();
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
