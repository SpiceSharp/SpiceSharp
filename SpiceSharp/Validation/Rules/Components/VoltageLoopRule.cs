using SpiceSharp.Components;
using SpiceSharp.Diagnostics;
using SpiceSharp.Diagnostics.Validation;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Validation.Rules
{
    /// <summary>
    /// An <see cref="IRule"/> that checks for voltage loops.
    /// </summary>
    /// <seealso cref="IFixedVoltageRule" />
    public class VoltageLoopRule : IFixedVoltageRule
    {
        private VariableParameters _vp;
        private Dictionary<Variable, HashSet<Variable>> _fixedGroups = new Dictionary<Variable, HashSet<Variable>>();

        /// <summary>
        /// Gets the nodes that are fixed in relationship to each other.
        /// </summary>
        /// <value>
        /// The fixed.
        /// </value>
        public HashSet<Variable> Fixed { get; private set; }

        /// <summary>
        /// Gets the component that closes the voltage loop.
        /// </summary>
        /// <value>
        /// The closes loop.
        /// </value>
        public IComponent ClosesLoop { get; private set; }

        /// <summary>
        /// Occurs when the rule has been violated.
        /// </summary>
        public event EventHandler<RuleViolationEventArgs> Violated;

        /// <summary>
        /// Resets the rule.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        /// <exception cref="ValidationException">Thrown when no variable set has been specified.</exception>
        public void Reset(IParameterSetDictionary parameters)
        {
            _vp = parameters.GetValue<VariableParameters>();
            if (_vp == null || _vp.Variables == null)
                throw new ValidationException(Properties.Resources.Validation_NoVariableSet);
            var variables = _vp.Variables;

            // Reset the graph
            _fixedGroups.Clear();
            _fixedGroups.Add(variables.Ground, new HashSet<Variable> { variables.Ground });
        }

        /// <summary>
        /// Checks whether or not two nodes have a fixed voltage relationship.
        /// </summary>
        /// <param name="node1">The first node.</param>
        /// <param name="node2">The second node.</param>
        /// <returns>
        /// <c>true</c> if the nodes have a fixed voltage relationship; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ValidationException">Thrown if no variable set was specified.</exception>
        public bool AreFixed(string node1, string node2)
        {
            if (_vp == null || _vp.Variables == null)
                throw new ValidationException(Properties.Resources.Validation_NoVariableSet);
            var variables = _vp.Variables;

            if (variables.TryGetNode(node1, out var n1) && variables.TryGetNode(node2, out var n2))
            {
                if (_fixedGroups.TryGetValue(n1, out var group1) && _fixedGroups.TryGetValue(n2, out var group2))
                    return group1 == group2;
            }
            return false;
        }

        /// <summary>
        /// Applies a fixed voltage between nodes.
        /// </summary>
        /// <param name="component">The component that applies a fixed voltage.</param>
        /// <param name="nodes">The nodes over which a fixed voltage is applied.</param>
        /// <exception cref="VoltageLoopException">If a voltage loop has been detected.</exception>
        public void ApplyFixedVoltage(IComponent component, params string[] nodes)
        {
            nodes.ThrowIfNull(nameof(nodes));
            if (_vp == null || _vp.Variables == null)
                throw new ValidationException(Properties.Resources.Validation_NoVariableSet);
            var variables = _vp.Variables;
            if (nodes.Length <= 1)
                throw new ValidationException(Properties.Resources.Validation_NoFixedVoltageNodes);

            for (var i = 0; i < nodes.Length; i++)
            {
                var ni = variables.MapNode(nodes[i], VariableType.Voltage);
                var hasni = _fixedGroups.TryGetValue(ni, out var fixedGroupi);
                for (var j = i + 1; j < nodes.Length; j++)
                {
                    var nj = variables.MapNode(nodes[j], VariableType.Voltage);
                    var hasnj = _fixedGroups.TryGetValue(nj, out var fixedGroupj);

                    // Short-circuit and fixed voltage violation
                    if (ni == nj)
                    {
                        Fixed = fixedGroupi;
                        ClosesLoop = component;
                        var args = new RuleViolationEventArgs();
                        Violated?.Invoke(this, args);
                        if (!args.Ignore)
                            throw new VoltageLoopException(component);
                    }

                    // These two have already fixed connections to other nodes
                    if (hasni && hasnj)
                    {
                        // Closes a voltage loop (both variable are already part of a fixed-relation graph group
                        if (fixedGroupi == fixedGroupj)
                        {
                            Fixed = fixedGroupi;
                            ClosesLoop = component;
                            var args = new RuleViolationEventArgs();
                            Violated?.Invoke(this, args);
                            if (!args.Ignore)
                                throw new VoltageLoopException(component);
                        }

                        // Merge the two groups
                        foreach (var v in fixedGroupj)
                            _fixedGroups[v] = fixedGroupj;
                        fixedGroupi.UnionWith(fixedGroupj);
                    }
                    else if (hasni)
                    {
                        _fixedGroups[nj] = fixedGroupi;
                        fixedGroupi.Add(nj);
                    }
                    else if (hasnj)
                    {
                        _fixedGroups[ni] = fixedGroupj;
                        fixedGroupj.Add(ni);
                    }
                    else
                    {
                        var group = new HashSet<Variable> { ni, nj };
                        _fixedGroups[ni] = group;
                        _fixedGroups[nj] = group;
                    }
                }
            }
        }

        /// <summary>
        /// Finish the check by validating the results.
        /// </summary>
        public void Validate()
        {
            // Validation is already done by this point.
        }
    }
}
