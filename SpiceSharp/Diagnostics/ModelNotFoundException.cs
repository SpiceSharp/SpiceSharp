namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when the model of a component could not be found.
    /// </summary>
    /// <seealso cref="SpiceSharp.CircuitException" />
    public class ModelNotFoundException : CircuitException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelNotFoundException"/> class.
        /// </summary>
        /// <param name="componentName">Name of the entity.</param>
        /// <param name="modelName">Name of the model.</param>
        public ModelNotFoundException(string componentName)
            : base("No model found for component '{1}'.".FormatString(componentName))
        {
        }
    }
}
