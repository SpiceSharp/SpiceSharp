using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An <see cref="ITemperatureBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="ParallelBehavior{T}" />
    /// <seealso cref="ITemperatureBehavior" />
    public class TemperatureBehavior : ParallelBehavior<ITemperatureBehavior>, ITemperatureBehavior
    {
        private readonly IWorkDistributor _temperature;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="simulation">The parallel simulation.</param>
        public TemperatureBehavior(string name, ParallelSimulation simulation)
            : base(name, simulation)
        {
            if (simulation.LocalConfigurations.TryGetValue(out TemperatureParameters result))
                _temperature = result.TemperatureDistributor;
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        public void Temperature()
        {
            if (_temperature != null)
            {
                var methods = new Action[Behaviors.Count];
                for (var i = 0; i < methods.Length; i++)
                {
                    var behavior = Behaviors[i];
                    methods[i] = () => behavior.Temperature();
                }
                _temperature.Execute(methods);
            }
            else
            {
                for (var i = 0; i < Behaviors.Count; i++)
                    Behaviors[i].Temperature();
            }
        }
    }
}
