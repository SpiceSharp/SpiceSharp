using SpiceSharp.Circuits;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a (Spice) model.
    /// </summary>
    public abstract class Model : Entity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        protected Model(string name)
            : base(name)
        {
        }
    }
}
