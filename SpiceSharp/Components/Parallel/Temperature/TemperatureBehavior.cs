using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An <see cref="ITemperatureBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="ITemperatureBehavior" />
    public class TemperatureBehavior : Behavior, ITemperatureBehavior
    {
        private readonly Workload _temperature;
        private readonly BehaviorList<ITemperatureBehavior> _temperatureBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="simulation">The parallel simulation.</param>
        public TemperatureBehavior(string name, ParallelSimulation simulation)
            : base(name)
        {
            if (simulation.LocalConfigurations.TryGetValue(out TemperatureParameters result))
            {
                if (result.TemperatureDistributor != null)
                    _temperature = new Workload(result.TemperatureDistributor, simulation.EntityBehaviors.Count);
                foreach (var container in simulation.EntityBehaviors)
                {
                    if (container.TryGetValue(out ITemperatureBehavior temperature))
                        _temperature.Actions.Add(temperature.Temperature);
                }
            }
            _temperatureBehaviors = simulation.EntityBehaviors.GetBehaviorList<ITemperatureBehavior>();
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        public void Temperature()
        {
            if (_temperature != null)
                _temperature.Execute();
            else
            {
                foreach (var behavior in _temperatureBehaviors)
                    behavior.Temperature();
            }
        }
    }
}
