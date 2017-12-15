using SpiceSharp.Circuits;
using SpiceSharp.Components.ComponentBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for semiconductor <see cref="Resistor"/>
    /// </summary>
    public class ResistorModel : CircuitModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public ResistorModel(CircuitIdentifier name) : base(name)
        {
            RegisterBehavior(new ResistorModelTemperatureBehavior());
        }
    }
}
