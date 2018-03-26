using System;

namespace SpiceSharp
{
    /// <summary>
    /// Regular string identifier
    /// </summary>
    public class StringIdentifier : Identifier
    {
        /// <summary>
        /// The id
        /// </summary>
        private readonly string _id;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        public StringIdentifier(string id)
        {
            _id = id ?? throw new ArgumentNullException(nameof(id));
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString() => _id;

        /// <summary>
        /// Get hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => _id.GetHashCode();

        /// <summary>
        /// Clone the string
        /// </summary>
        /// <returns></returns>
        public override Identifier Clone()
        {
            return new StringIdentifier((string)_id.Clone());
        }

        /// <summary>
        /// Check equality
        /// </summary>
        /// <param name="other">Other identifier</param>
        /// <returns></returns>
        public override bool Equals(Identifier other)
        {
            if (other is StringIdentifier si)
                return _id.Equals(si._id);
            return false;
        }

        /// <summary>
        /// Check equality
        /// </summary>
        /// <param name="obj">Other object</param>
        /// <returns></returns>
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
