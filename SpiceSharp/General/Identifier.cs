using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharp
{
    /// <summary>
    /// An identifier that can be used for a variety of applications.
    /// </summary>
    /// <seealso cref="IEquatable{Identifier}" />
    public abstract class Identifier : IEquatable<Identifier>
    {
        /// <summary>
        /// Sets the type of the implicit string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentException">
        /// Invalid type.
        /// or
        /// Type does not have a valid constructor.
        /// </exception>
        public static void SetImplicitStringIdentifierType(Type value)
        {
            // Don't use implicit conversion
            if (value == null)
            {
                _defaultStringConverter = name => new StringIdentifier(name);
                return;
            }

            // Check the type
            var info = value.GetTypeInfo();
            if (!info.IsSubclassOf(typeof(Identifier)))
                throw new ArgumentException(@"Invalid type.");
            var ctor = info.GetConstructor(new[] {typeof(string)});
            if (ctor == null)
                throw new ArgumentException(@"Type does not have a valid constructor.");

            // Use a compiled lambda expression for maximum performance
            var inputArg = Expression.Parameter(typeof(string), "name");
            var expr = Expression.New(ctor, inputArg);
            var lambda = Expression.Lambda(typeof(Func<string, Identifier>), expr, inputArg);
            _defaultStringConverter = (Func<string, Identifier>) lambda.Compile();
        }

        /// <summary>
        /// The default string converter.
        /// </summary>
        private static Func<string, Identifier> _defaultStringConverter = name => new StringIdentifier(name);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool Equals(Identifier other);

        /// <summary>
        /// Clones this identifier.
        /// </summary>
        /// <returns>The cloned identifier.</returns>
        public abstract Identifier Clone();

        /// <summary>
        /// Performs an implicit conversion from <see cref="string"/> to <see cref="Identifier"/>.
        /// </summary>
        /// <param name="id">The string identifier.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Identifier(string id) => _defaultStringConverter?.Invoke(id);
    }
}
