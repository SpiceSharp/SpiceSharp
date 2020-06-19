namespace SpiceSharpTest.Models
{
    /// <summary>
    /// The only reason we need this class, is because NUnit doesn't allow us to pass mockups through
    /// TestCaseSource.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public class Proxy<T>
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Proxy{T}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public Proxy(T value)
        {
            Value = value;
        }
    }
}
