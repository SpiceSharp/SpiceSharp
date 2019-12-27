using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// A generic behavior for a switch model
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IParameterized{T}" />
    public class ModelBehavior : Behavior, IParameterized<ModelBaseParameters>
    {
        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>
        /// The model parameters.
        /// </value>
        public ModelBaseParameters ModelParameters { get; }

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        ModelBaseParameters IParameterized<ModelBaseParameters>.Parameters => ModelParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public ModelBehavior(string name, ModelBindingContext context)
            : base(name)
        {
            ModelParameters = context.GetParameterSet<ModelBaseParameters>();
        }
    }
}
