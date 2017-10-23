using SpiceSharp.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviours;

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

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt"></param>
        /// <param name="timeStep"></param>
        public virtual void Truncate(Circuit ckt, ref double timeStep)
        {
            // Do nothing
        }
    }
}
