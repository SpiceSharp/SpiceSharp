using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CommonBehaviors
{
    /// <summary>
    /// A structure containing the unknowns for a two-port.
    /// </summary>
    /// <remarks>
    /// A two-port is a device with two ports. Each defined by a voltage over them, and where the current in one
    /// of the nodes is equal to the current out of the other one of the same port.
    /// </remarks>
    /// <typeparam name="T">The base value type.</typeparam>
    public readonly struct TwoPort<T>
    {
        /// <summary>
        /// The left port.
        /// </summary>
        public readonly OnePort<T> Left;

        /// <summary>
        /// The right port.
        /// </summary>
        public readonly OnePort<T> Right;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoPort{T}"/> struct.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="context">The context.</param>
        public TwoPort(IVariableFactory<IVariable<T>> factory, IComponentBindingContext context)
        {
            context.Nodes.CheckNodes(4);
            Right = new OnePort<T>(
                factory.GetSharedVariable(context.Nodes[0]),
                factory.GetSharedVariable(context.Nodes[1]));
            Left = new OnePort<T>(
                factory.GetSharedVariable(context.Nodes[2]),
                factory.GetSharedVariable(context.Nodes[3]));
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object obj)
        {
            if (obj is TwoPort<T> op)
            {
                if (!Left.Equals(op.Left))
                    return false;
                if (!Right.Equals(op.Right))
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
        public override readonly int GetHashCode() => (Left.GetHashCode() * 13 * 13) ^ Right.GetHashCode();

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(TwoPort<T> left, TwoPort<T> right) => left.Equals(right);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(TwoPort<T> left, TwoPort<T> right) => !left.Equals(right);
    }
}
