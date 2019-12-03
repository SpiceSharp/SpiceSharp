using System;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;
using SpiceSharp.Diagnostics.Validation;
using SpiceSharp.Simulations;

namespace SpiceSharp.Validation.Rules
{
    /// <summary>
    /// An <see cref="IRule"/> for finding floating nodes.
    /// </summary>
    /// <seealso cref="IComponentValidationRule" />
    public class FloatingNodeRule : IConductivePathRule
    {
        private IVariableSet _variables;
        private Dictionary<Variable, HashSet<Variable>> _groups = new Dictionary<Variable, HashSet<Variable>>();

        /// <summary>
        /// Gets the floating node.
        /// </summary>
        /// <value>
        /// The floating node.
        /// </value>
        public Variable FloatingNode { get; private set; }

        /// <summary>
        /// Occurs when the rule has been violated.
        /// </summary>
        public event EventHandler<RuleViolationEventArgs> Violated;

        /// <summary>
        /// Sets up the validation rule.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        /// <exception cref="ValidationException">Thrown when no variable set has been specified.</exception>
        public void Setup(IParameterSetDictionary parameters)
        {
            var config = parameters.GetValue<VariableParameters>();
            _variables = config.Variables ?? throw new ValidationException(Properties.Resources.Validation_NoVariableSet);

            // Reset the node conductive groups
            _groups.Clear();
            _groups.Add(_variables.Ground, new HashSet<Variable> { _variables.Ground });
        }

        /// <summary>
        /// Applies a conductive path between nodes of a component. If no nodes are specified,
        /// then none of the component pins create a conductive path to another node.
        /// </summary>
        /// <param name="component">The component that applies the conductive paths.</param>
        /// <param name="nodes">The nodes that are connected together via a conductive path.</param>
        public void AddConductivePath(IComponent component, params string[] nodes)
        {
            nodes.ThrowIfNull(nameof(nodes));

            // There are some pins conducting to other pins
            for (var i = 0; i < nodes.Length; i++)
            {
                nodes[i].ThrowIfNull(nameof(nodes));
                var ni = _variables.MapNode(nodes[i], VariableType.Voltage);
                var hasni = _groups.TryGetValue(ni, out var groupni);
                for (var j = i + 1; j < nodes.Length; j++)
                {
                    nodes[j].ThrowIfNull(nameof(nodes));
                    var nj = _variables.MapNode(nodes[j], VariableType.Voltage);

                    // Don't have to add here
                    if (ni == nj)
                        continue;
                    var hasnj = _groups.TryGetValue(nj, out var groupnj);
                    if (hasni && hasnj)
                    {
                        // Merge the two groups
                        foreach (var v in groupnj)
                            _groups[v] = groupni;
                        groupni.UnionWith(groupnj);
                    }
                    else if (hasni)
                    {
                        _groups[nj] = groupni;
                        groupni.Add(nj);
                    }
                    else if (hasnj)
                    {
                        _groups[ni] = groupnj;
                        groupnj.Add(ni);
                    }
                    else
                    {
                        var group = new HashSet<Variable> { ni, nj };
                        _groups[ni] = group;
                        _groups[nj] = group;
                    }
                }
            }

            // Make sure that the pins that don't conduct are also taken into account
            for (var i = 0; i < component.PinCount; i++)
            {
                var node = _variables.MapNode(component.GetNode(i), VariableType.Voltage);
                if (!_groups.ContainsKey(node))
                    _groups.Add(node, new HashSet<Variable> { node });
            }
        }

        /// <summary>
        /// Specifies a component that does not have a conductive path between any nodes.
        /// </summary>
        /// <param name="component">The component.</param>
        public void NoConductivePath(IComponent component)
        {

        }

        /// <summary>
        /// Finish the check by validating the results.
        /// </summary>
        /// <exception cref="FloatingNodeException">Thrown when an unhandled floating node has been found.</exception>
        public void Validate()
        {
            foreach (var pair in _groups)
            {
                // Is this node connected indirectly to ground?
                if (pair.Value.Contains(_variables.Ground))
                    continue;

                FloatingNode = pair.Key;
                var args = new RuleViolationEventArgs();
                Violated?.Invoke(this, args);
                if (!args.Ignore)
                    throw new FloatingNodeException(pair.Key);
            }
        }
    }
}
