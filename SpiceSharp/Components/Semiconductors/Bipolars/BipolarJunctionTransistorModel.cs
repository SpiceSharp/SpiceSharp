using SpiceSharp.Components.Bipolars;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="BipolarJunctionTransistor" />.
    /// </summary>
    /// <seealso cref="Entity{T}" />
    /// <seealso cref="IParameterized{P}" />
    /// <seealso cref="ModelParameters" />
    public class BipolarJunctionTransistorModel : Entity<BindingContext>,
        IParameterized<ModelParameters>
    {
        /// <inheritdoc/>
        public ModelParameters Parameters { get; } = new ModelParameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="BipolarJunctionTransistorModel"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public BipolarJunctionTransistorModel(string name)
            : base(name)
        {
        }
    }
}
