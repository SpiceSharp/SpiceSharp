using System;

namespace SpiceSharp
{
    /// <summary>
    /// A normal identifier that is just identified by a string.
    /// </summary>
    /// <seealso cref="Identifier" />
    public class StringIdentifier : Identifier
    {
        /// <summary>
        /// The identifier.
        /// </summary>
        private readonly string _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringIdentifier"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="ArgumentNullException">id</exception>
        public StringIdentifier(string id)
        {
            _id = id ?? throw new ArgumentNullException(nameof(id));
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => _id;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => _id.GetHashCode();

        /// <summary>
        /// Clones this identifier.
        /// </summary>
        /// <returns>
        /// The cloned identifier.
        /// </returns>
        public override Identifier Clone()
        {
            return new StringIdentifier(_id);
        }

        /// <summary>
        /// Indicates whether the current identifier is equal to another identifier.
        /// </summary>
        /// <param name="other">An identifier to compare with this identifier.</param>
        /// <returns>
        ///   <c>true</c> if the current identifier is equal to the <paramref name="other" /> parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(Identifier other)
        {
            if (other is StringIdentifier si)
                return _id.Equals(si._id);
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
            if (obj is StringIdentifier si)
                return _id.Equals(si._id);
            if (obj is string str)
                return _id.Equals(str);
            return false;
        }
    }
}
