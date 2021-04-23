using Microsoft.CodeAnalysis;
using System;

namespace SpiceSharpGenerator
{
    /// <summary>
    /// Represents a behavior.
    /// </summary>
    public class BehaviorData : IEquatable<BehaviorData>
    {
        /// <summary>
        /// Gets the behavior type.
        /// </summary>
        /// <value>
        /// The behavior type.
        /// </value>
        public INamedTypeSymbol Behavior { get; }

        /// <summary>
        /// Gets or sets the behavior simulation that needs to checked on.
        /// </summary>
        /// <value>
        /// The checked simulation behavior.
        /// </value>
        public INamedTypeSymbol Check { get; set; }

        /// <summary>
        /// Gets or sets the required behaviors.
        /// </summary>
        /// <value>
        /// The required behavior.
        /// </value>
        public INamedTypeSymbol[] Required { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorData"/> class.
        /// </summary>
        /// <param name="behavior">The behavior type.</param>
        public BehaviorData(INamedTypeSymbol behavior)
        {
            Behavior = behavior ?? throw new ArgumentNullException(nameof(behavior));
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var c = SymbolEqualityComparer.Default;
            return c.GetHashCode(Behavior);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;
            if (obj is BehaviorData other)
                return Equals(other);
            return false;
        }

        /// <inheritdoc/>
        public bool Equals(BehaviorData other)
        {
            if (!SymbolEqualityComparer.Default.Equals(Behavior, other.Behavior))
                return false;
            return true;
        }

        /// <summary>
        /// Converts to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => Behavior.ToString();
    }
}
