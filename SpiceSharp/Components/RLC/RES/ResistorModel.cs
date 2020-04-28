using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for semiconductor <see cref="Resistor"/>
    /// </summary>
    /// <seealso cref="Model"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ResistorModelParameters"/>
    public partial class ResistorModel : Model,
        IParameterized<ResistorModelParameters>
    {
        /// <inheritdoc/>
        public ResistorModelParameters Parameters { get; } = new ResistorModelParameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResistorModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public ResistorModel(string name) 
            : base(name)
        {
        }
    }
}
