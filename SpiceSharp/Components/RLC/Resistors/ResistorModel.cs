using SpiceSharp.Components.Capacitors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for semiconductor <see cref="Resistor" />
    /// </summary>
    /// <seealso cref="Entity{T}" />
    /// <seealso cref="IParameterized{T}" />
    /// <seealso cref="IParameterized{P}" />
    /// <seealso cref="ModelParameters" />
    public class ResistorModel : Entity<BindingContext>,
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
    }
}
