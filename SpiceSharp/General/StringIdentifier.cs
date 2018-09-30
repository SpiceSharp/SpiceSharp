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
        /// Specifies whether string identifier is case-sensitive
        /// </summary>
        private readonly bool _caseSensitive;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringIdentifier"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="caseSensitive">Specifies whether string identifier is case-sensitive</param>
        /// <exception cref="ArgumentNullException">id</exception>
        public StringIdentifier(string id, bool caseSensitive = false)
        {
            _id = id ?? throw new ArgumentNullException(nameof(id));
            _caseSensitive = caseSensitive;
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
        public override int GetHashCode()
        {
            if (_caseSensitive)
            {
                return _id.GetHashCode();
            }
            else
            {
                return _id.ToUpper().GetHashCode();
            }
        }

        /// <summary>
        /// Clones this identifier.
        /// </summary>
        /// <returns>
        /// The cloned identifier.
        /// </returns>
        public override Identifier Clone()
        {
            return new StringIdentifier(_id, _caseSensitive);
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
            {
                if (_caseSensitive && si._caseSensitive)
                {
                    return _id.Equals(si._id, StringComparison.CurrentCulture);
                }
                return _id.Equals(si._id, StringComparison.CurrentCultureIgnoreCase);
            }
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
                return Equals(si);
            if (obj is string str)
            {
                if (_caseSensitive)
                {
                    return _id.Equals(str);
                }
                return _id.Equals(str, StringComparison.CurrentCultureIgnoreCase);
            }
            return false;
        }
    }
}
