using SpiceSharp.Components.Diodes;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Diode"/>.
    /// </summary>
    /// <seealso cref="Model"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ModelParameters"/>
    public class DiodeModel : Entity<BindingContext>,
        IParameterized<ModelParameters>
    {
        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public ModelParameters Parameters { get; } = new ModelParameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="DiodeModel"/> class.
        /// </summary>
        /// <param name="name">The name of the diode model.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public DiodeModel(string name)
            : base(name)
        {
        }
    }
}
