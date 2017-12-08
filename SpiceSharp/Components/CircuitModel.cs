using SpiceSharp.Circuits;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a model.
    /// It also has parameters.
    /// </summary>
    public abstract class CircuitModel : CircuitObject
    {
        /// <summary>
        /// Get the name of the component
        /// </summary>
        public CircuitIdentifier Name { get; }

        /// <summary>
        /// Gets the priority
        /// </summary>
        public int Priority { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the model</param>
        public CircuitModel(CircuitIdentifier name)
        {
            // Make sure the models are evaluated before the actual components
            Name = name;
            Priority = 1;
        }

        /// <summary>
        /// Setup the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public virtual void Setup(Circuit ckt)
        {
            // Do nothing
        }
        
        /// <summary>
        /// Unsetup/destroy the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public virtual void Unsetup(Circuit ckt)
        {
            // Do nothing
        }
    }
}
