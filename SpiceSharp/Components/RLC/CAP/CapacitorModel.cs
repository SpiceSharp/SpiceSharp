using SpiceSharp.Behaviors;
using SpiceSharp.Components.CapacitorBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a semiconductor <see cref="Capacitor"/>
    /// </summary>
    public class CapacitorModel : Model,
        IParameterized<ModelBaseParameters>
    {
        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>
        /// The model parameters.
        /// </value>
        public ModelBaseParameters Parameters { get; } = new ModelBaseParameters();

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        ModelBaseParameters IParameterized<ModelBaseParameters>.Parameters => Parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="CapacitorModel"/> class.
        /// </summary>
        /// <param name="name"></param>
        public CapacitorModel(string name) : base(name)
        {
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var container = new BehaviorContainer(Name)
            {
                new ModelBehavior(Name, new ModelBindingContext(this, simulation))
            };
            simulation.EntityBehaviors.Add(container);
        }
    }
}
