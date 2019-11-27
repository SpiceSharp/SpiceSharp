namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when a variable cannot be found.
    /// </summary>
    /// <seealso cref="CircuitException" />
    public class VariableNotFoundException : CircuitException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableNotFoundException"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public VariableNotFoundException(string name)
            : base("A variable by the name '{0}' could not be found.".FormatString(name))
        {
        }
    }
}
