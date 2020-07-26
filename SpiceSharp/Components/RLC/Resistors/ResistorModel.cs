using SpiceSharp.Behaviors;
using SpiceSharp.Components.Capacitors;
using SpiceSharp.Components.Common;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for semiconductor <see cref="Resistor"/>
    /// </summary>
    /// <seealso cref="Model"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ModelParameters"/>
    public class ResistorModel : Model,
        IParameterized<ModelParameters>
    {
        /// <inheritdoc/>
        public ModelParameters Parameters { get; } = new ModelParameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResistorModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public ResistorModel(string name)
            : base(name)
        {
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var container = new BehaviorContainer(Name);
            container.Add(new ParameterBehavior<ModelParameters>(Name, new BindingContext(this, simulation, null, LinkParameters)));
            simulation.EntityBehaviors.Add(container);
        }
    }
}
