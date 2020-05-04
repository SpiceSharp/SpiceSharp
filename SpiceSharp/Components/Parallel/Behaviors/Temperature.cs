using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="ITemperatureBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="ITemperatureBehavior" />
    public class Temperature : Behavior, 
        ITemperatureBehavior
    {
        private readonly Workload _workload;
        private readonly BehaviorList<ITemperatureBehavior> _behaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="Temperature"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="simulation">The parallel simulation.</param>
        public Temperature(string name, ParallelSimulation simulation)
            : base(name)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<Parameters>();
            if (parameters.TemperatureDistributor != null)
            {
                _workload = new Workload(parameters.TemperatureDistributor, simulation.EntityBehaviors.Count);
                foreach (var container in simulation.EntityBehaviors)
                {
                    if (container.TryGetValue(out ITemperatureBehavior temperature))
                        _workload.Actions.Add(temperature.Temperature);
                }
            }
            _behaviors = simulation.EntityBehaviors.GetBehaviorList<ITemperatureBehavior>();
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
