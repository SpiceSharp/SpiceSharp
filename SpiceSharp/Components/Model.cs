using SpiceSharp.Circuits;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a (Spice) model.
    /// </summary>
    public abstract class Model : Entity
    {
        /// <summary>
        /// The default priority for models.
        /// </summary>
        public const int ModelPriority = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        protected Model(string name)
            : base(name)
        {
            // Make sure the models are evaluated before the actual components
            Priority = ModelPriority;
        }
    }
}
