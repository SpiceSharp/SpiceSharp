using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// Variables for a mosfet.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public readonly struct MosfetVariables<T>
    {
        /// <summary>
        /// The drain node.
        /// </summary>
        public readonly IVariable<T> Drain;

        /// <summary>
        /// The internal drain node.
        /// </summary>
        public readonly IVariable<T> DrainPrime;

        /// <summary>
        /// The gate node.
        /// </summary>
        public readonly IVariable<T> Gate;

        /// <summary>
        /// The source node.
        /// </summary>
        public readonly IVariable<T> Source;

        /// <summary>
        /// The internal source node.
        /// </summary>
        public readonly IVariable<T> SourcePrime;

        /// <summary>
        /// The bulk node.
        /// </summary>
        public readonly IVariable<T> Bulk;

        /// <summary>
        /// Initializes a new instance of the <see cref="MosfetVariables{T}"/> struct.
        /// </summary>
        /// <param name="context">The binding context.</param>
        /// <param name="factory">The variable factory.</param>
        public MosfetVariables(IComponentBindingContext context, IVariableFactory<IVariable<T>> factory)
        {
            var nodes = context.Nodes.CheckNodes(4);
            Drain = factory.GetSharedVariable(nodes[0]);
            Gate = factory.GetSharedVariable(nodes[1]);
            Source = factory.GetSharedVariable(nodes[2]);
            Bulk = factory.GetSharedVariable(nodes[3]);
            var bp = context.GetParameterSet<Parameters>();
            var mbp = context.ModelBehaviors.GetParameterSet<ModelParameters>();
            
            if (!mbp.DrainResistance.Equals(0.0) || !mbp.SheetResistance.Equals(0.0) && bp.DrainSquares > 0)
                DrainPrime = factory.CreatePrivateVariable(context.Behaviors.Name.Combine("drain"), Units.Volt);
            else
                DrainPrime = Drain;

            if (!mbp.SourceResistance.Equals(0.0) || !mbp.SheetResistance.Equals(0.0) && bp.SourceSquares > 0)
                SourcePrime = factory.CreatePrivateVariable(context.Behaviors.Name.Combine("source"), Units.Volt);
            else
                SourcePrime = Source;
        }

        /// <summary>
        /// Gets the matrix locations.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>The matrix locations.</returns>
        public readonly MatrixLocation[] GetMatrixLocations(IVariableMap map)
        {
            int d = map[Drain];
            int dp = map[DrainPrime];
            int s = map[Source];
            int sp = map[SourcePrime];
            int g = map[Gate];
            int b = map[Bulk];

            return
            [
                new MatrixLocation(d, d),
                new MatrixLocation(g, g),
                new MatrixLocation(s, s),
                new MatrixLocation(b, b),
                new MatrixLocation(dp, dp),
                new MatrixLocation(sp, sp),

                new MatrixLocation(d, dp),

                new MatrixLocation(g, b),
                new MatrixLocation(g, dp),
                new MatrixLocation(g, sp),

                new MatrixLocation(s, sp),

                new MatrixLocation(b, g),
                new MatrixLocation(b, dp),
                new MatrixLocation(b, sp),

                new MatrixLocation(dp, d),
                new MatrixLocation(dp, g),
                new MatrixLocation(dp, b),
                new MatrixLocation(dp, sp),

                new MatrixLocation(sp, g),
                new MatrixLocation(sp, s),
                new MatrixLocation(sp, b),
                new MatrixLocation(sp, dp)
            ];
        }

        /// <summary>
        /// Gets the right hand side vector.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>The right hand side vector indices.</returns>
        public readonly int[] GetRhsIndices(IVariableMap map)
        {
            return
            [
                map[Gate],
                map[Bulk],
                map[DrainPrime],
                map[SourcePrime]
            ];
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object obj)
        {
            if (obj is MosfetVariables<T> mv)
            {
                if (!Drain.Equals(mv.Drain))
                    return false;
                if (!DrainPrime.Equals(mv.DrainPrime))
                    return false;
                if (!Source.Equals(mv.Source))
                    return false;
                if (!SourcePrime.Equals(mv.SourcePrime))
                    return false;
                if (!Gate.Equals(mv.Gate))
                    return false;
                if (!Bulk.Equals(mv.Bulk))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override readonly int GetHashCode()
        {
            int hash = Drain.GetHashCode();
            hash = (hash * 13) ^ DrainPrime.GetHashCode();
            hash = (hash * 13) ^ Source.GetHashCode();
            hash = (hash * 13) ^ SourcePrime.GetHashCode();
            hash = (hash * 13) ^ Gate.GetHashCode();
            hash = (hash * 13) ^ Bulk.GetHashCode();
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
        public static bool operator ==(MosfetVariables<T> left, MosfetVariables<T> right) => left.Equals(right);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(MosfetVariables<T> left, MosfetVariables<T> right) => !left.Equals(right);
    }
}
