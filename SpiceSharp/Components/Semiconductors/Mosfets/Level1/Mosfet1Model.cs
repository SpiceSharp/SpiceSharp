using SpiceSharp.Behaviors;
using SpiceSharp.Components.Mosfets.Level1;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Mosfet1"/>.
    /// </summary>
    /// <seealso cref="Model"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ModelParameters"/>
    public class Mosfet1Model : Model,
        IParameterized<ModelParameters>
    {
        /// <inheritdoc/>
        public ModelParameters Parameters { get; } = new ModelParameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="Mosfet1Model"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Mosfet1Model(string name) 
            : base(name)
        {
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            CalculateDefaults();
            var context = new ModelBindingContext(this, simulation, LinkParameters);
            behaviors.AddIfNo<ITemperatureBehavior>(simulation, () => new ModelTemperature(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
