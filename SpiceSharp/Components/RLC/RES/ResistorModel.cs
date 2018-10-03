using SpiceSharp.Components.ResistorBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for semiconductor <see cref="Resistor"/>
    /// </summary>
    public class ResistorModel : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public ResistorModel(string name) : base(name)
        {
            ParameterSets.Add(new ModelBaseParameters());
        }
    }
}
