using SpiceSharp.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// A generic behavior for a <see cref="CapacitorModel"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IParameterized{T}" />
    public class ModelBehavior : Behavior, IParameterized<ModelBaseParameters>
    {
        private readonly ModelBaseParameters _mbp;

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        ModelBaseParameters IParameterized<ModelBaseParameters>.Parameters => _mbp;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public ModelBehavior(string name, ModelBindingContext context)
            : base(name)
        {
            _mbp = context.GetParameterSet<ModelBaseParameters>();
        }
    }
}
