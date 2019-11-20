using SpiceSharp.Behaviors;
using SpiceSharp.Components.ResistorBehaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for semiconductor <see cref="Resistor"/>
    /// </summary>
    public class ResistorModel : Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResistorModel"/> class.
        /// </summary>
        /// <param name="name"></param>
        public ResistorModel(string name) : base(name)
        {
            Parameters.Add(new ModelBaseParameters());
        }
    }
}
