using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// A factory for variables that will shield them from outside.
    /// </summary>
    /// <seealso cref="IVariableFactory{V}" />
    /// <seealso cref="IVariable"/>
    public class VariableFactory : IVariableFactory<IVariable>
    {
        private readonly string _name;
        private readonly IVariableFactory<IVariable> _parent;
        private readonly Dictionary<string, string> _nodeMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableFactory"/> class.
        /// </summary>
        /// <param name="name">The subcircuit instance name.</param>
        /// <param name="nodes">The nodes.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="comparer">The equality comparer for nodes.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/>, <paramref name="parent"/> or <paramref name="nodes"/> is <c>null</c>.</exception>
        public VariableFactory(string name, IVariableFactory<IVariable> parent, IEnumerable<Bridge<string>> nodes, IEqualityComparer<string> comparer)
        {
            _name = name.ThrowIfNull(nameof(name));
            _parent = parent.ThrowIfNull(nameof(parent));
            nodes.ThrowIfNull(nameof(nodes));

            _nodeMap = new Dictionary<string, string>(comparer ?? Constants.DefaultComparer)
            {
                { Constants.Ground, Constants.Ground }
            };
            foreach (var bridge in nodes)
                _nodeMap.Add(bridge.Local, bridge.Global);
        }

        /// <inheritdoc/>
        public IVariable GetSharedVariable(string name)
        {
            if (!_nodeMap.TryGetValue(name, out string mapped))
                mapped = _name.Combine(name);
            return _parent.GetSharedVariable(mapped);
        }

        /// <inheritdoc/>
        public IVariable CreatePrivateVariable(string name, IUnit unit) => _parent.CreatePrivateVariable(_name.Combine(name), unit);
    }
}
