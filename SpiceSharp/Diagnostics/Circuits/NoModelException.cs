namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when the model of a component could not be found.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class NoModelException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoModelException"/> class.
        /// </summary>
        /// <param name="componentName">Name of the entity.</param>
        public NoModelException(string componentName)
            : base(Properties.Resources.Components_NoModel.FormatString(componentName))
        {
        }
    }
}
