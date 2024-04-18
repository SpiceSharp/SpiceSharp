using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Diodes
{
    /// <summary>
    /// Variables for a diode.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public readonly struct DiodeVariables<T>
    {
        /// <summary>
        /// The positive node.
        /// </summary>
        public readonly IVariable<T> Positive;

        /// <summary>
        /// The internal positive node.
        /// </summary>
        public readonly IVariable<T> PosPrime;

        /// <summary>
        /// The negative node.
        /// </summary>
        public readonly IVariable<T> Negative;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiodeVariables{T}"/> struct.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="context">The context.</param>
        public DiodeVariables(string name, IVariableFactory<IVariable<T>> factory, IComponentBindingContext context)
        {
            context.Nodes.CheckNodes(2);

            var mbp = context.ModelBehaviors.GetParameterSet<ModelParameters>();

            Positive = factory.GetSharedVariable(context.Nodes[0]);
            Negative = factory.GetSharedVariable(context.Nodes[1]);

            if (mbp.Resistance > 0)
                PosPrime = factory.CreatePrivateVariable(name.Combine("pos"), Units.Volt);
            else
                PosPrime = Positive;
        }

        /// <summary>
        /// Gets the matrix locations.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>The matrix locations.</returns>
        public readonly MatrixLocation[] GetMatrixLocations(IVariableMap map)
        {
            int pos = map[Positive];
            int posPrime = map[PosPrime];
            int neg = map[Negative];

            return
            [
                new MatrixLocation(pos, pos),
                new MatrixLocation(neg, neg),
                new MatrixLocation(posPrime, posPrime),
                new MatrixLocation(neg, posPrime),
                new MatrixLocation(posPrime, neg),
                new MatrixLocation(pos, posPrime),
                new MatrixLocation(posPrime, pos)
            ];
        }

        /// <summary>
        /// Gets the right hand side vector indicies.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>The right hand side vector indices.</returns>
        public readonly int[] GetRhsIndicies(IVariableMap map) => [map[Negative], map[PosPrime]];

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object obj)
        {
            if (obj is DiodeVariables<T> dv)
            {
                if (!Positive.Equals(dv.Positive))
                    return false;
                if (!Negative.Equals(dv.Negative))
                    return false;
                if (!PosPrime.Equals(dv.PosPrime))
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
        public override readonly int GetHashCode()
        {
            int hash = Positive.GetHashCode();
            hash = (hash * 13) ^ Negative.GetHashCode();
            hash = (hash * 13) ^ PosPrime.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(DiodeVariables<T> left, DiodeVariables<T> right) => left.Equals(right);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(DiodeVariables<T> left, DiodeVariables<T> right) => !left.Equals(right);
    }
}
