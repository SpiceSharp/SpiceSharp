namespace SpiceSharp.Parser
{
    /// <summary>
    /// An assignment token
    /// </summary>
    public class AssignmentToken
    {
        /// <summary>
        /// The name of the assignment NAME = VALUE
        /// </summary>
        public object Name;

        /// <summary>
        /// The value of the assignment NAME = VALUE
        /// </summary>
        public object Value;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="value">The value</param>
        public AssignmentToken(object name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
