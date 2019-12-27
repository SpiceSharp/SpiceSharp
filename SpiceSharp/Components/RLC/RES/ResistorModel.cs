using SpiceSharp.Components.ResistorBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for semiconductor <see cref="Resistor"/>
    /// </summary>
    public class ResistorModel : Model,
        IParameterized<ModelBaseParameters>
    {
        private readonly ModelBaseParameters _mbp = new ModelBaseParameters();

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        ModelBaseParameters IParameterized<ModelBaseParameters>.Parameters => _mbp;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResistorModel"/> class.
        /// </summary>
        /// <param name="name"></param>
        public ResistorModel(string name) : base(name)
        {
        }
    }
}
