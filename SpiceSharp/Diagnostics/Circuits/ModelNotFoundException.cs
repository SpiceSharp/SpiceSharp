using System;

namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when the model of a component could not be found.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class ModelNotFoundException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelNotFoundException"/> class.
        /// </summary>
        /// <param name="componentName">Name of the entity.</param>
        public ModelNotFoundException(string componentName)
            : base("No model found for component '{1}'.".FormatString(componentName))
        {
        }
    }
}
