using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class that represents a (Spice) component/device.
    /// </summary>
    /// <seealso cref="Entity"/>
    /// <seealso cref="IComponent"/>
    /// <seealso cref="IRuleSubject"/>
    public abstract class Component : Entity,
        IComponent,
        IRuleSubject
    {
        private string[] _connections;

        /// <inheritdoc/>
        public IReadOnlyList<string> Nodes => new ReadOnlyCollection<string>(_connections);

        /// <inheritdoc/>
        public string Model { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Component" /> class.
        /// </summary>
        /// <param name="name">The string of the entity.</param>
        /// <param name="nodeCount">The node count.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        protected Component(string name, int nodeCount)
            : base(name)
        {
            _connections = nodeCount > 0 ? new string[nodeCount] : null;
        }

        /// <inheritdoc/>
        public IComponent Connect(params string[] nodes)
        {
            if (_connections == null)
            {
                if (nodes == null || nodes.Length == 0)
                    return this;
                else
                    throw new NodeMismatchException(Name, 0, nodes.Length);
            }

            if (nodes == null || nodes.Length != _connections.Length)
                throw new NodeMismatchException(Name, _connections.Length, nodes?.Length ?? 0);
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].ThrowIfNull("node{0}".FormatString(i + 1));
                _connections[i] = nodes[i];
            }
            return this;
        }

        /// <inheritdoc/>
        void IRuleSubject.Apply(IRules rules)
        {
            var p = rules.GetParameterSet<ComponentRuleParameters>();

            // Map the connections to variables
            if (_connections != null)
            {
                var variables = new IVariable[_connections.Length];
                for (int i = 0; i < _connections.Length; i++)
                    variables[i] = p.Factory.GetSharedVariable(_connections[i]);

                foreach (var rule in rules.GetRules<IConductiveRule>())
                    rule.AddPath(this, variables);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (_connections != null && _connections.Length > 0)
                return "{0} {1} {2}".FormatString(Name, string.Join(", ", _connections), Model ?? "");
            return "{0} {1}".FormatString(Name, Model ?? "");
        }

        /// <inheritdoc/>
        public override IEntity Clone()
        {
            var clone = (Component)MemberwiseClone();
            clone._connections = (string[])_connections.Clone();
            return clone;
        }
    }

    /// <summary>
    /// A class that represents a (Spice) component/device with parameters.
    /// </summary>
    /// <typeparam name="P">The component parameter type.</typeparam>
    public abstract class Component<P> : Component, IParameterized<P>
        where P : IParameterSet, ICloneable<P>, new()
    {
        /// <inheritdoc/>
        public P Parameters { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Component{P}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="nodeCount">The node count.</param>
        protected Component(string name, int nodeCount)
            : base(name, nodeCount)
        {
            Parameters = new();
        }

        /// <inheritdoc/>
        public override IEntity Clone()
        {
            var clone = (Component<P>)base.Clone();
            clone.Parameters = Parameters.Clone();
            return clone;
        }
    }
}
