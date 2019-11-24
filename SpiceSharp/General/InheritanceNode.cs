using System.Collections.Generic;

namespace SpiceSharp.General
{
    /// <summary>
    /// A class that stores information about the inheritance tree stored in a type dictionary.
    /// </summary>
    public class InheritanceNode<T>
    {
        /// <summary>
        /// Gets the associated value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value { get; }

        /// <summary>
        /// Gets a value indicating whether the value type is identical to the key type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is direct; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirect { get; }

        /// <summary>
        /// A base type can have multiple child types. This points to the next node that
        /// has the same base type, but has a different child type.
        /// </summary>
        /// <value>
        /// The next sibling.
        /// </value>
        public InheritanceNode<T> NextSibling { get; set; }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public IEnumerable<T> Values
        {
            get
            {
                var elt = this;
                while (elt != null)
                {
                    yield return elt.Value;
                    elt = elt.NextSibling;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritanceNode{T}" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="isDirect">A flag indicating whether the value is identical to the type.</param>
        public InheritanceNode(T value, bool isDirect)
        {
            Value = value;
            IsDirect = isDirect;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>The cloned instance.</returns>
        public InheritanceNode<T> Clone()
        {
            InheritanceNode<T> clone;
            if (Value is ICloneable cloneable)
                clone = new InheritanceNode<T>((T)cloneable.Clone(), IsDirect);
            else
                clone = new InheritanceNode<T>(Value, IsDirect);
            if (NextSibling != null)
                clone.NextSibling = NextSibling.Clone();
            return clone;
        }
    }
}
