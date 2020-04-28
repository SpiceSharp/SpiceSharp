using SpiceSharp.Behaviors;

namespace SpiceSharp.Components
{
    public partial class CapacitorModel
    {
        /// <summary>
        /// A generic behavior for a <see cref="CapacitorModel"/>.
        /// </summary>
        /// <seealso cref="Behavior" />
        /// <seealso cref="IParameterized{T}" />
        /// <seealso cref="CapacitorModelParameters"/>
        protected class ModelBehavior : Behavior, 
            IParameterized<CapacitorModelParameters>
        {
            /// <inheritdoc/>
            public CapacitorModelParameters Parameters { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ModelBehavior"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="context">The context.</param>
            public ModelBehavior(string name, ModelBindingContext context)
                : base(name)
            {
                Parameters = context.GetParameterSet<CapacitorModelParameters>();
            }
        }
    }
}