using SpiceSharp.Parameters;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a model.
    /// It also has parameters.
    /// </summary>
    public abstract class CircuitModel<T> : Parameterized<T>, ICircuitObject
    {
        /// <summary>
        /// Get the name of the component
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the priority
        /// </summary>
        public int Priority { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the model</param>
        public CircuitModel(string name)
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
        /// Use initial conditions for the device
        /// </summary>
        /// <param name="ckt"></param>
        public virtual void SetIc(Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt"></param>
        public virtual void Temperature(Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Load the component in the current circuit state
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public virtual void Load(Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Load the component in the current circuit state for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public virtual void AcLoad(Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Accept the current timepoint as the solution
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public virtual void Accept(Circuit ckt)
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
