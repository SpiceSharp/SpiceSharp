using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
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
        private readonly string[] _connections;

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
            for (var i = 0; i < nodes.Length; i++)
            {
                nodes[i].ThrowIfNull("node{0}".FormatString(i + 1));
                _connections[i] = nodes[i];
            }
            return this;
        }

        /// <inheritdoc/>
        protected override ICloneable Clone()
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override string ToString()
        {
            if (_connections != null && _connections.Length > 0)
                return "{0} {1} {2}".FormatString(Name, string.Join(", ", _connections), Model ?? "");
            return "{0} {1}".FormatString(Name, Model ?? "");
        }
    }

    /// <summary>
    /// A class that represents a (Spice) component/device.
    /// This implementation will by default use dependency injection to find behaviors.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
    /// <seealso cref="IRuleSubject" />
    public abstract class Component<TContext> : Component,
        IEntity<TContext>
        where TContext : IBindingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Component{TContext}"/> class.
        /// </summary>
        /// <param name="name">The string of the entity.</param>
        /// <param name="nodeCount">The node count.</param>
        protected Component(string name, int nodeCount) 
            : base(name, nodeCount)
        {
        }

        /// <inheritdoc />
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            DI.Resolve(simulation, this, behaviors);
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
