using System;

namespace SpiceSharp.General
{
    /// <summary>
    /// Event arguments that can be used when a type could not be resolved or found.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public class TypeNotFoundEventArgs<T>
    {
        /// <summary>
        /// Gets the type that could not be found.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type Type { get; }

        /// <summary>
        /// Gets or sets the value that can fill in the blank.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotFoundEventArgs{T}"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
        public TypeNotFoundEventArgs(Type type)
        {
            Type = type.ThrowIfNull(nameof(type));
        }
    }
}
