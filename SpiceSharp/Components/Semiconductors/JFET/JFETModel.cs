using SpiceSharp.Behaviors;
using SpiceSharp.Components.JFETBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Model for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="Model" />
    public class JFETModel : Model,
        IParameterized<ModelBaseParameters>,
        IParameterized<ModelNoiseParameters>
    {
        private readonly ModelBaseParameters _mbp = new ModelBaseParameters();
        private readonly ModelNoiseParameters _mnp = new ModelNoiseParameters();

        ModelBaseParameters IParameterized<ModelBaseParameters>.Parameters => _mbp;
        ModelNoiseParameters IParameterized<ModelNoiseParameters>.Parameters => _mnp;

        /// <summary>
        /// Initializes a new instance of the <see cref="JFETModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        public JFETModel(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            CalculateDefaults();
            var context = new ModelBindingContext(this, simulation);
            behaviors.AddIfNo<ITemperatureBehavior>(simulation, () => new ModelTemperatureBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
