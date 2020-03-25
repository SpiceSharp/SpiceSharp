using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Contains and manages circuit nodes.
    /// </summary>
    public class VariableSet : IVariableSet
    {
        /// <summary>
        /// Gets all the unknowns stored in the variable set.
        /// </summary>
        /// <value>
        /// The unknowns.
        /// </value>
        protected List<IVariable> Unknowns { get; } = new List<IVariable>();

        /// <summary>
        /// Gets the map of variables that are searchable by their name.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        protected Dictionary<string, IVariable> Map { get; }

        /// <summary>
        /// Event that is called when a variable is added to the set.
        /// </summary>
        public event EventHandler<VariableEventArgs> VariableAdded;

        /// <summary>
        /// Gets the ground node.
        /// </summary>
        public IVariable Ground { get; }

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{T}"/> that is used to determine equality of keys.
        /// </summary>
        public IEqualityComparer<string> Comparer => Map.Comparer;

        /// <summary>
        /// Gets the number of variables.
        /// </summary>
        public int Count => Unknowns.Count;

        /// <summary>
        /// Enumerate all variable names in the set.
        /// </summary>
        public IEnumerable<string> Keys => Map.Keys;

        /// <summary>
        /// Gets the <see cref="IVariable"/> with the specified identifier.
        /// </summary>
        /// <value>
        /// The <see cref="IVariable"/>.
        /// </value>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="VariableNotFoundException">Thrown if the variable wasn't found.</exception>
        public IVariable this[string id]
        {
            get
            {
                if (Map.TryGetValue(id, out var node))
                    return node;
                throw new VariableNotFoundException(id);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableSet"/> class.
        /// </summary>
        public VariableSet()
            : this(EqualityComparer<string>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableSet"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing variable/node names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        public VariableSet(IEqualityComparer<string> comparer)
        {
            // Setup the ground node
            Ground = new Variable("0", Units.Volt);
            Map = new Dictionary<string, IVariable>(comparer)
            {
                { Ground.Name, Ground }, 
                { "GND", Ground }
            };
        }

        /// <summary>
        /// This method maps a variable in the circuit. If a variable with the same name already exists, then that variable is returned.
        /// </summary>
        /// <param name="id">The name of the variable.</param>
        /// <param name="units">The unit of the variable.</param>
        /// <returns>
        /// A new variable with the specified name and type, or a previously mapped variable if it already existed.
        /// </returns>
        /// <remarks>
        /// If the variable already exists, the variable type is ignored.
        /// </remarks>
        public IVariable MapNode(string id, Units units)
        {
            id.ThrowIfNull(nameof(id));

            // Check the node
            if (Map.ContainsKey(id))
                return Map[id];

            var node = new Variable(id, units);
            Unknowns.Add(node);
            Map.Add(id, node);

            // Call the event
            var args = new VariableEventArgs(node);
            OnVariableAdded(args);
            return node;
        }

        /// <summary>
        /// Make an alias for a variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="alias">The alias for the name.</param>
        /// <remarks>
        /// This basically gives two names to the same variable. This can be used for example to make multiple names
        /// point to the ground node.
        /// </remarks>
        public void AliasNode(IVariable variable, string alias)
        {
            variable.ThrowIfNull(nameof(variable));
            alias.ThrowIfNull(nameof(alias));
            if (!Map.ContainsValue(variable))
                throw new VariableNotFoundException(nameof(variable));
            Map.Add(alias, variable);
        }

        /// <summary>
        /// Creates the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IVariable Create(string id, Units type)
        {
            id.ThrowIfNull(nameof(id));

            // Create the node
            var node = new Variable(id, type);
            Unknowns.Add(node);

            // Call the event
            var args = new VariableEventArgs(node);
            OnVariableAdded(args);
            return node;
        }

        /// <summary>
        /// Determines whether the set contains a mapped variable by a specified name.
        /// </summary>
        /// <param name="id">The name.</param>
        /// <returns>
        ///   <c>true</c> if the specified set contains the variable; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsNode(string id) => Map.ContainsKey(id);

        /// <summary>
        /// Determines whether the set contains any variable by a specified name.
        /// </summary>
        /// <param name="id">The name.</param>
        /// <returns>
        ///   <c>true</c> if the set contains the variable; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(string id) => Unknowns.Exists(node => Map.Comparer.Equals(id, node.Name));

        /// <summary>
        /// Tries to get a variable.
        /// </summary>
        /// <param name="id">The name.</param>
        /// <param name="node">The found variable.</param>
        /// <returns>
        ///   <c>true</c> if the variable was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetNode(string id, out IVariable node) => Map.TryGetValue(id, out node);

        /// <summary>
        /// Gets a mapped variable. If the node voltage does not exist, an exception will be thrown.
        /// </summary>
        /// <param name="id">The name.</param>
        /// <returns>
        /// The node with the specified name.
        /// </returns>
        public IVariable GetNode(string id)
        {
            id.ThrowIfNull(nameof(id));
            if (Map.TryGetValue(id, out var result))
                return result;
            throw new VariableNotFoundException(nameof(id));
        }

        /// <summary>
        /// Clear all variables.
        /// </summary>
        public void Clear()
        {
            Unknowns.Clear();

            // Setup ground node
            Map.Clear();
            Map.Add(Ground.Name, Ground);
            Map.Add("GND", Ground);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IVariable> GetEnumerator() => Unknowns.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Method that calls the <see cref="VariableAdded"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected void OnVariableAdded(VariableEventArgs args) => VariableAdded?.Invoke(this, args);
    }
}
