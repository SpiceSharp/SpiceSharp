using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class that represents a (Spice) component/device.
    /// </summary>
    public abstract class Component : Entity, IComponent,
        IRuleSubject
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly string[] _connections;

        /// <summary>
        /// Gets the nodes.
        /// </summary>
        /// <value>
        /// The nodes.
        /// </value>
        public IReadOnlyList<string> Nodes => new ReadOnlyCollection<string>(_connections);

        /// <summary>
        /// Gets or sets the model of the component.
        /// </summary>
        /// <value>
        /// The model of the component.
        /// </value>
        public string Model { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Component" /> class.
        /// </summary>
        /// <param name="name">The string of the entity.</param>
        /// <param name="nodeCount">The node count.</param>
        protected Component(string name, int nodeCount)
            : base(name)
        {
            _connections = nodeCount > 0 ? new string[nodeCount] : null;
        }

        /// <summary>
        /// Connects the component in the circuit.
        /// </summary>
        /// <param name="nodes">The node indices.</param>
        /// <returns>
        /// The instance calling the method for chaining.
        /// </returns>
        public IComponent Connect(params string[] nodes)
        {
            if (nodes == null || nodes.Length != _connections.Length)
                throw new NodeMismatchException(Name, _connections.Length, nodes?.Length ?? 0);
            for (var i = 0; i < nodes.Length; i++)
            {
                nodes[i].ThrowIfNull("node{0}".FormatString(i + 1));
                _connections[i] = nodes[i];
            }
            return this;
        }

        /// <summary>
        /// Gets the node name by pin index.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <returns>The node index.</returns>
        public string GetNode(int index)
        {
            if (index < 0 || index >= _connections.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _connections[index];
        }

        /// <inheritdoc/>
        protected override Entity Clone()
        {
            var clone = (Component)base.Clone();
            for (var i = 0; i < _connections.Length; i++)
                clone._connections[i] = _connections[i];
            return clone;
        }

        /// <inheritdoc/>
        protected override void CopyFrom(ICloneable source)
        {
            base.CopyFrom(source);
            var c = (Component)source;
            for (var i = 0; i < _connections.Length; i++)
                _connections[i] = c._connections[i];
        }

        void IRuleSubject.Apply(IRules rules)
        {
            var p = rules.GetParameterSet<ComponentRuleParameters>();

            // Map the connections to variables
            if (_connections != null)
            {
                var variables = new IVariable[_connections.Length];
                for (var i = 0; i < _connections.Length; i++)
                    variables[i] = p.Factory.GetSharedVariable(_connections[i]);

                foreach (var rule in rules.GetRules<IConductiveRule>())
                    rule.AddPath(this, variables);
            }
        }
    }
}
