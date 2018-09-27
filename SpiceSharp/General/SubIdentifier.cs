using System;

namespace SpiceSharp
{
    /// <summary>
    /// Class for an identifier with multiple parts.
    /// </summary>
    /// <seealso cref="Identifier" />
    /// <remarks>
    /// This is meant for identifying for example entities inside subcircuits. The modifier indicates the
    /// subcircuit, while Id will identify the actual name of the entity.
    /// </remarks>
    public class SubIdentifier : Identifier
    {
        /// <summary>
        /// Gets the base identifier.
        /// </summary>
        public Identifier Id { get; }

        /// <summary>
        /// Gets the modifier.
        /// </summary>
        public Identifier Sub { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubIdentifier"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="sub">The modifier.</param>
        /// <exception cref="ArgumentNullException">
        /// id
        /// or
        /// sub
        /// </exception>
        public SubIdentifier(Identifier id, Identifier sub)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Sub = sub ?? throw new ArgumentNullException(nameof(sub));
        }

        /// <summary>
        /// Clones this identifier.
        /// </summary>
        /// <returns>
        /// The cloned identifier.
        /// </returns>
        public override Identifier Clone()
        {
            return new SubIdentifier(
                Id.Clone(),
                Sub.Clone());
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var a = Id.GetHashCode();
            var b = Sub.GetHashCode();
            return a * 31 + b;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(Identifier other)
        {
            if (other == null)
                return false;
            if (other is SubIdentifier mi)
                return Id.Equals(mi.Id) && Sub.Equals(mi.Sub);
            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is SubIdentifier mi)
                return Id.Equals(mi.Id) && Sub.Equals(mi.Sub);
            return false;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "{0}[{1}]".FormatString(Id, Sub);
        }
    }
}
