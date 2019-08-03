using SpiceSharp.Components.ResistorBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for semiconductor <see cref="Resistor"/>
    /// </summary>
    public class ResistorModel : Model
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ResistorModel"/> class.
        /// </summary>
        /// <param name="name"></param>
        public ResistorModel(string name) : base(name)
        {
            ParameterSets.Add(new ModelBaseParameters());
        }
    }
}
