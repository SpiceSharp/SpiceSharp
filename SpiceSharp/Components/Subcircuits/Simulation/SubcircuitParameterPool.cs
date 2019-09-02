using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A parameter pool for a <see cref="SubcircuitSimulation"/>.
    /// </summary>
    /// <remarks>
    /// The pool will first try to find parameters locally, but if it can't
    /// find them, it will forward parameters from the parent simulation.
    /// </remarks>
    /// <seealso cref="SpiceSharp.ParameterPool" />
    public class SubcircuitParameterPool : ParameterPool
    {
        private ParameterPool _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitParameterPool"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        /// <param name="parentPool">The parent parameter pool.</param>
        public SubcircuitParameterPool(IEqualityComparer<string> comparer, ParameterPool parentPool)
            : base(comparer)
        {
            _parent = parentPool.ThrowIfNull(nameof(parentPool));
        }

        /// <summary>
        /// Gets the <see cref="ParameterSetDictionary"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="ParameterSetDictionary"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public override ParameterSetDictionary this[string name]
        {
            get
            {
                if (base.Contains(name))
                    return base[name];

                // Apparently the name is expected, so let's ask the parent
                // simulation if it knows more
                return _parent[name];
            }
        }
    }
}
