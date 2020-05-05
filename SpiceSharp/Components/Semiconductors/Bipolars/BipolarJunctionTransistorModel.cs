using SpiceSharp.Behaviors;
using SpiceSharp.Components.Bipolars;
using SpiceSharp.Simulations;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="BipolarJunctionTransistor"/>.
    /// </summary>
    /// <seealso cref="Model"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ModelParameters"/>
    public class BipolarJunctionTransistorModel : Model,
        IParameterized<ModelParameters>
    {
        /// <inheritdoc/>
        public ModelParameters Parameters { get; } = new ModelParameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="BipolarJunctionTransistorModel"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        public BipolarJunctionTransistorModel(string name) 
            : base(name)
        {
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new ModelBindingContext(this, simulation, LinkParameters);
            behaviors
                .AddIfNo<ITemperatureBehavior>(simulation, () => new ModelTemperature(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
