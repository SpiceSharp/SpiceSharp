using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Text;

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
    public struct TwoPort<T> where T : IFormattable
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
                factory.MapNode(context.Nodes[0]),
                factory.MapNode(context.Nodes[1]));
            Left = new OnePort<T>(
                factory.MapNode(context.Nodes[2]),
                factory.MapNode(context.Nodes[3]));
        }
    }
}
