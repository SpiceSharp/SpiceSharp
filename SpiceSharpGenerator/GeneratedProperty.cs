using Microsoft.CodeAnalysis;

namespace SpiceSharpGenerator
{
    /// <summary>
    /// Describes an auto-generated property.
    /// </summary>
    public class GeneratedProperty
    {
        /// <summary>
        /// Gets the field from which the property was generated.
        /// </summary>
        /// <value>
        /// The field symbol that was at the base of the property.
        /// </value>
        public IFieldSymbol Field { get; }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>
        /// The variable name of the generated property.
        /// </value>
        public string Variable { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedProperty"/> class.
        /// </summary>
        /// <param name="field">The field at the base of the generated property.</param>
        public GeneratedProperty(IFieldSymbol field)
        {
            Field = field;

            // Create the name of the property
            string name = field.Name;
            if (name[0] == '_')
                name = char.ToUpper(name[1]) + name.Substring(2);
            else
                name = char.ToUpper(name[0]) + name.Substring(1);
            Variable = name;
        }
    }
}
