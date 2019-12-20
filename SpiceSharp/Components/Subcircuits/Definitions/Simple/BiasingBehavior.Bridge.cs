using SpiceSharp.Algebra;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    public partial class BiasingBehavior
    {
        /// <summary>
        /// A bridge between a local and parent element.
        /// </summary>
        protected struct Bridge
        {
            /// <summary>
            /// Gets the local.
            /// </summary>
            /// <value>
            /// The local.
            /// </value>
            public Element<double> Local { get; }

            /// <summary>
            /// Gets the parent.
            /// </summary>
            /// <value>
            /// The parent.
            /// </value>
            public Element<double> Parent { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Bridge"/> struct.
            /// </summary>
            /// <param name="local">The local element.</param>
            /// <param name="parent">The parent element.</param>
            public Bridge(Element<double> local, Element<double> parent)
            {
                Local = local.ThrowIfNull(nameof(local));
                Parent = parent.ThrowIfNull(nameof(parent));
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
                if (obj is Bridge bridge)
                {
                    if (Local != bridge.Local)
                        return false;
                    if (Parent != bridge.Parent)
                        return false;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                return Local.GetHashCode() ^ (Parent.GetHashCode() * 13);
            }

            /// <summary>
            /// Implements the operator ==.
            /// </summary>
            /// <param name="left">The left operand.</param>
            /// <param name="right">The right operand.</param>
            /// <returns>
            /// The result of the operator.
            /// </returns>
            public static bool operator ==(Bridge left, Bridge right)
            {
                if (left.Local != right.Local)
                    return false;
                if (left.Parent != right.Parent)
                    return false;
                return true;
            }

            /// <summary>
            /// Implements the operator !=.
            /// </summary>
            /// <param name="left">The left operand.</param>
            /// <param name="right">The right operand.</param>
            /// <returns>
            /// The result of the operator.
            /// </returns>
            public static bool operator !=(Bridge left, Bridge right)
            {
                return !(left == right);
            }
        }
    }
}
