namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when a variable cannot be found.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class VariableNotFoundException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableNotFoundException"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public VariableNotFoundException(string name)
            : base(Properties.Resources.VariableNotFound.FormatString(name))
        {
        }
    }
}
