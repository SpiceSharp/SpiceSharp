using System;
using System.Collections.Generic;
using SpiceSharp.Attributes;
using SpiceSharp.Entities;
using SpiceSharp.General;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class that represents a (Spice) component/device.
    /// </summary>
    public abstract class Component : Entity, IComponent, IRuleSubject
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly string[] _connections;

        /// <summary>
        /// Gets the number of nodes.
        /// </summary>
        /// <value>
        /// The number of nodes.
        /// </value>
        public int PinCount => _connections.Length;

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
        /// Gets the node index of a pin.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <returns>The node index.</returns>
        public string GetNode(int index)
        {
            if (index < 0 || index >= _connections.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _connections[index];
        }

        /// <summary>
        /// Gets the node indexes (in order).
        /// </summary>
        /// <param name="variables">The set of variables.</param>
        /// <returns>An enumerable for all nodes.</returns>
        public IEnumerable<Variable> MapNodes(IVariableSet variables)
        {
            variables.ThrowIfNull(nameof(variables));

            // Map connected nodes
            foreach (var c in _connections)
            {
                var node = variables.MapNode(c, VariableType.Voltage);
                yield return node;
            }
        }

        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        protected override Entity Clone()
        {
            var clone = (Component)base.Clone();
            for (var i = 0; i < _connections.Length; i++)
                clone._connections[i] = _connections[i];
            return clone;

        }

        /// <summary>
        /// Copy from another component.
        /// </summary>
        /// <param name="source">The source component.</param>
        protected override void CopyFrom(ICloneable source)
        {
            base.CopyFrom(source);
            var c = (Component)source;
            for (var i = 0; i < PinCount; i++)
                _connections[i] = c._connections[i];
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <param name="container">The container with all the rules that should be validated.</param>
        public virtual void ApplyTo(IRuleContainer container)
        {
            // Baseline check
            foreach (var rule in container.GetAllValues<IComponentRule>())
                rule.ApplyComponent(this);

            // Checks for conductivity
            foreach (var rule in container.GetAllValues<IConductivePathRule>())
            {
                var doAll = true;
                foreach (var attribute in AttributeCache.GetAttributes<ConnectedAttribute>(GetType()))
                {
                    doAll = false;
                    if (attribute.Pin1 >= 0 && attribute.Pin2 >= 0)
                        rule.ApplyConductivePath(this, GetNode(attribute.Pin1), GetNode(attribute.Pin2));
                    else
                        rule.ApplyConductivePath(this);
                }
                if (doAll)
                {
                    for (var i = 0; i < PinCount; i++)
                    {
                        for (var j = i + 1; j < PinCount; j++)
                            rule.ApplyConductivePath(this, GetNode(i), GetNode(j));
                    }
                }
            }

            // Checks for fixed voltages
            foreach (var rule in container.GetAllValues<IFixedVoltageRule>())
            {
                foreach (var attribute in AttributeCache.GetAttributes<VoltageDriverAttribute>(GetType()))
                    rule.ApplyFixedVoltage(this, GetNode(attribute.Positive), GetNode(attribute.Negative));
            }
        }
    }
}
