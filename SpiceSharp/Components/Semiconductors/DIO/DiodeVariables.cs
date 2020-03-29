using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Variables for a diode.
    /// </summary>
    /// <typeparam name="T">The base value type</typeparam>
    public struct DiodeVariables<T> where T : IFormattable
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
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IVariable<T>[] Variables => new[] { Positive, PosPrime, Negative };

        /// <summary>
        /// Initializes a new instance of the <see cref="DiodeVariables{T}"/> struct.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="context">The context.</param>
        public DiodeVariables(string name, IVariableFactory<IVariable<T>> factory, IComponentBindingContext context)
        {
            context.Nodes.CheckNodes(2);

            var mbp = context.ModelBehaviors.GetParameterSet<ModelBaseParameters>();

            Positive = factory.MapNode(context.Nodes[0]);
            Negative = factory.MapNode(context.Nodes[1]);

            if (mbp.Resistance > 0)
                PosPrime = factory.Create(name.Combine("pos"), Units.Volt);
            else
                PosPrime = Positive;
        }

        /// <summary>
        /// Gets the matrix locations.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>The matrix locations.</returns>
        public MatrixLocation[] GetMatrixLocations(IVariableMap map)
        {
            var pos = map[Positive];
            var posPrime = map[PosPrime];
            var neg = map[Negative];

            return new[] {
                new MatrixLocation(pos, pos),
                new MatrixLocation(neg, neg),
                new MatrixLocation(posPrime, posPrime),
                new MatrixLocation(neg, posPrime),
                new MatrixLocation(posPrime, neg),
                new MatrixLocation(pos, posPrime),
                new MatrixLocation(posPrime, pos)
            };
        }

        /// <summary>
        /// Gets the RHS indicies.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns></returns>
        public int[] GetRhsIndicies(IVariableMap map) => new[] { map[Negative], map[PosPrime] };
    }
}
