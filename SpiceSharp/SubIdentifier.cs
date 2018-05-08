using System;

namespace SpiceSharp
{
    /// <summary>
    /// Class for modified identifiers
    /// </summary>
    public class SubIdentifier : Identifier
    {
        /// <summary>
        /// Gets the base identifier
        /// </summary>
        public Identifier Id { get; }

        /// <summary>
        /// Gets the modifier
        /// </summary>
        public Identifier Sub { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="sub">Modifier</param>
        public SubIdentifier(Identifier id, Identifier sub)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Sub = sub ?? throw new ArgumentNullException(nameof(sub));
        }

        /// <summary>
        /// Clone the identifier
        /// </summary>
        /// <returns></returns>
        public override Identifier Clone()
        {
            return new SubIdentifier(
                Id.Clone(),
                Sub.Clone());
        }

        /// <summary>
        /// Get the hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var a = Id.GetHashCode();
            var b = Sub.GetHashCode();
            return a * 31 + b;
        }

        /// <summary>
        /// Check equality
        /// </summary>
        /// <param name="other">Other identifier</param>
        /// <returns></returns>
        public override bool Equals(Identifier other)
        {
            if (other == null)
                return false;
            if (other is SubIdentifier mi)
                return Id.Equals(mi.Id) && Sub.Equals(mi.Sub);
            return false;
        }

        /// <summary>
        /// Check equality
        /// </summary>
        /// <param name="obj">Other object</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is SubIdentifier mi)
                return Id.Equals(mi.Id) && Sub.Equals(mi.Sub);
            return false;
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{0}[{1}]".FormatString(Id, Sub);
        }
    }
}
