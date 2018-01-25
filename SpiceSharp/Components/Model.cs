using SpiceSharp.Circuits;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a model.
    /// It also has parameters.
    /// </summary>
    public abstract class Model : Entity
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the model</param>
        public Model(Identifier name)
            : base(name)
        {
            // Make sure the models are evaluated before the actual components
            Priority = 1;
        }

        /// <summary>
        /// Setup the model
        /// </summary>
        /// <param name="circuit">Circuit</param>
        public override void Setup(Circuit circuit)
        {
            // Models do not have any nodes to setup
        }
    }
}
