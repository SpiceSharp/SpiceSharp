using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// A factory for variables shielding
    /// </summary>
    /// <seealso cref="IVariableFactory{V}" />
    public class VariableFactory : IVariableFactory
    {
        private readonly string _name;
        private readonly IVariableFactory _parent;
        private readonly Dictionary<string, string> _nodeMap;

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IVariableSet Variables => _parent.Variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableFactory"/> class.
        /// </summary>
        /// <param name="name">The subcircuit instance name.</param>
        /// <param name="nodes">The nodes.</param>
        /// <param name="parent">The parent.</param>
        public VariableFactory(string name, IReadOnlyList<Bridge<string>> nodes, IVariableFactory parent)
        {
            _name = name.ThrowIfNull(nameof(name));
            _parent = parent.ThrowIfNull(nameof(parent));

            _nodeMap = new Dictionary<string, string>(parent.Variables.Comparer);
            _nodeMap.Add(Constants.Ground, Constants.Ground);
            foreach (var bridge in nodes)
                _nodeMap.Add(bridge.Local, bridge.Global);
        }

        /// <summary>
        /// Gets a variable that can be shared with other behaviors by the factory. If another variable
        /// already exists with the same name, that is returned instead.
        /// </summary>
        /// <param name="name">The name of the shared variable.</param>
        /// <returns>
        /// The shared variable.
        /// </returns>
        public IVariable GetSharedVariable(string name)
        {
            if (_nodeMap.TryGetValue(name, out var mapped))
                return _parent.GetSharedVariable(mapped);
            return _parent.GetSharedVariable(_name.Combine(name));
        }

        /// <summary>
        /// Creates a variable that is private to whoever requested it. The factory will not shared this
        /// variable with anyone else, and the name is only used for display purposes.
        /// </summary>
        /// <param name="name">The name of the private variable.</param>
        /// <param name="unit">The unit of the variable.</param>
        /// <returns>
        /// The private variable.
        /// </returns>
        public IVariable CreatePrivateVariable(string name, IUnit unit) => _parent.CreatePrivateVariable(_name.Combine(name), unit);
    }
}
