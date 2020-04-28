using SpiceSharp.Behaviors;

namespace SpiceSharp.Components
{
    public partial class ResistorModel
    {
        /// <summary>
        /// A generic model behavior for a <see cref="ResistorModel"/>.
        /// </summary>
        /// <seealso cref="Behavior" />
        /// <seealso cref="IParameterized{T}" />
        /// <seealso cref="ResistorModelParameters"/>
        protected class ModelBehavior : Behavior, 
            IParameterized<ResistorModelParameters>
        {
            /// <inheritdoc/>
            public ResistorModelParameters Parameters { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ModelBehavior"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="context">The context.</param>
            public ModelBehavior(string name, ModelBindingContext context)
                : base(name)
            {
                Parameters = context.GetParameterSet<ResistorModelParameters>();
            }
        }
    }
}