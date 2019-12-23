using SpiceSharp.Algebra;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    public abstract partial class LocalSolverState<T>
    {
        /// <summary>
        /// A bridge between a local and parent element.
        /// </summary>
        protected struct Bridge
        {
            private readonly Element<T> _local, _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="Bridge"/> struct.
            /// </summary>
            /// <param name="local">The local element.</param>
            /// <param name="parent">The parent element.</param>
            public Bridge(Element<T> local, Element<T> parent)
            {
                _local = local.ThrowIfNull(nameof(local));
                _parent = parent.ThrowIfNull(nameof(parent));
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
                    if (_local != bridge._local)
                        return false;
                    if (_parent != bridge._parent)
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
                return _local.GetHashCode() ^ (_parent.GetHashCode() * 13);
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
                if (left._local != right._local)
                    return false;
                if (left._parent != right._parent)
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

            /// <summary>
            /// Applies the local elements to the parent elements.
            /// </summary>
            public void Apply()
            {
                _parent.Add(_local.Value);
            }
        }
    }
}
