using SpiceSharp.Behaviors;
using SpiceSharp.Components.JFETs;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Model for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="Model" />
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ModelParameters"/>
    public class JFETModel : Model,
        IParameterized<ModelParameters>
    {
        /// <inheritdoc/>
        public ModelParameters Parameters { get; } = new ModelParameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="JFETModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        public JFETModel(string name)
            : base(name)
        {
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            Parameters.CalculateDefaults();
            var context = new ModelBindingContext(this, simulation, behaviors, LinkParameters);
            behaviors.AddIfNo<ITemperatureBehavior>(simulation, () => new ModelTemperature(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
