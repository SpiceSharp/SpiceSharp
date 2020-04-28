using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a semiconductor <see cref="Capacitor"/>
    /// </summary>
    /// <seealso cref="Model"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="CapacitorModelParameters"/>
    public partial class CapacitorModel : Model,
        IParameterized<CapacitorModelParameters>
    {
        /// <inheritdoc/>
        public CapacitorModelParameters Parameters { get; } = new CapacitorModelParameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="CapacitorModel"/> class.
        /// </summary>
        /// <param name="name">The name of the capacitor model.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public CapacitorModel(string name) 
            : base(name)
        {
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var container = new BehaviorContainer(Name)
            {
                new ModelBehavior(Name, new ModelBindingContext(this, simulation, LinkParameters))
            };
            simulation.EntityBehaviors.Add(container);
        }
    }
}
