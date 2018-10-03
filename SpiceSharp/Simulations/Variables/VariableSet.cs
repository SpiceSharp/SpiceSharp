using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Contains and manages circuit nodes.
    /// </summary>
    public class VariableSet : IEnumerable<Variable>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly List<Variable> _unknowns = new List<Variable>();
        private readonly Dictionary<string, Variable> _map;
        private bool _locked;

        /// <summary>
        /// The initial conditions. Obsolete, use initial conditions in the <see cref="TimeConfiguration" /> of a <see cref="TimeSimulation" /> instead.
        /// </summary>
        /// <value>
        /// The initial conditions by their variable identifier name.
        /// </value>
        /// <remarks>
        /// Initial conditions specify at which value a <see cref="TimeSimulation" /> should start.
        /// </remarks>
        /// <seealso cref="TimeSimulation" />
        [Obsolete]
        public Dictionary<string, double> InitialConditions { get; } = new Dictionary<string, double>();

        /// <summary>
        /// The nodeset values by their variable identifier name. Obsolete, use nodesets in the <see cref="BaseConfiguration" /> of a <see cref="BaseSimulation" /> instead.
        /// </summary>
        /// <value>
        /// The node sets.
        /// </value>
        /// <remarks>
        /// Nodeset values can help convergence. Specifying values here will allow the simulator
        /// to find a starting point while iterating to a solution. Choosing some values close to the
        /// final solution will usually speed up convergence greatly.
        /// </remarks>
        [Obsolete]
        public Dictionary<string, double> NodeSets { get; } = new Dictionary<string, double>();

        /// <summary>
        /// Gets the ground node.
        /// </summary>
        public Variable Ground { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableSet"/> class.
        /// </summary>
        public VariableSet()
        {
            // Setup the ground node
            Ground = new Variable("0", 0);
            _map = new Dictionary<string, Variable>
            {
                {Ground.Name, Ground}, 
                {"GND", Ground}
            };

            // Unlock
            _locked = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableSet"/> class.
        /// </summary>
        /// <param name="comparer">The comparer for identifiers.</param>
        public VariableSet(IEqualityComparer<string> comparer)
        {
            // Setup the ground node
            Ground = new Variable("0", 0);
            _map = new Dictionary<string, Variable>(comparer)
            {
                {Ground.Name, Ground}, 
                {"GND", Ground}
            };

            // Unlock
            _locked = false;
        }

        /// <summary>
        /// Gets the <see cref="Variable"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="Variable"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The variable at the specified index.</returns>
        public Variable this[int index] => _unknowns[index];

        /// <summary>
        /// Gets the number of variables.
        /// </summary>
        public int Count => _unknowns.Count;

        /// <summary>
        /// This method maps a variable in the circuit. If a variable with the same identifier already exists, then that variable is returned.
        /// </summary>
        /// <remarks>
        /// If the variable already exists, the variable type is ignored.
        /// </remarks>
        /// <param name="id">The identifier of the variable.</param>
        /// <param name="type">The type of the variable.</param>
        /// <returns>A new variable with the specified identifier and type, or a previously mapped variable if it already existed.</returns>
        public Variable MapNode(string id, VariableType type)
        {
            if (_locked)
                throw new CircuitException("Nodes are locked, mapping is not allowed anymore");

            // Check the node
            if (_map.ContainsKey(id))
                return _map[id];

            var node = new Variable(id, type, _unknowns.Count + 1);
            _unknowns.Add(node);
            _map.Add(id, node);
            return node;
        }

        /// <summary>
        /// This method maps a variable in the circuit. If a variable with the same identifier already exists, then that variable is returned.
        /// </summary>
        /// <param name="id">The identifier of the variable.</param>
        /// <returns>A new variable with the specified identifier and type, or a previously mapped variable if it already existed.</returns>
        public Variable MapNode(string id) => MapNode(id, VariableType.Voltage);

        /// <summary>
        /// Make an alias for a variable identifier.
        /// </summary>
        /// <remarks>
        /// This basically gives two names to the same variable. This can be used for example to make multiple identifiers
        /// point to the ground node.
        /// </remarks>
        /// <param name="original">The original identifier.</param>
        /// <param name="alias">The alias for the identifier.</param>
        public void AliasNode(string original, string alias)
        {
            var originalNode = _map[original];
            _map.Add(alias, originalNode);
        }

        /// <summary>
        /// Create a new variable.
        /// </summary>
        /// <remarks>
        /// Variables created using this method cannot be found back using the method <see cref="MapNode(string,VariableType)"/>.
        /// </remarks>
        /// <param name="id">The identifier of the new variable.</param>
        /// <param name="type">The type of the variable.</param>
        /// <returns>A new variable.</returns>
        public Variable Create(string id, VariableType type)
        {
            if (_locked)
                throw new CircuitException("Nodes are locked, mapping is not allowed anymore");

            // Create the node
            var index = _unknowns.Count + 1;
            var node = new Variable(id, type, index);
            _unknowns.Add(node);
            return node;
        }

        /// <summary>
        /// Create a new variable.
        /// </summary>
        /// <remarks>
        /// Variables created using this method cannot be found back using the method <see cref="MapNode(string,VariableType)"/>.
        /// </remarks>
        /// <param name="id">The identifier of the new variable.</param>
        /// <returns>A new variable.</returns>
        public Variable Create(string id) => Create(id, VariableType.Voltage);

        /// <summary>
        /// Determines whether the set contains a mapped variable by a specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        ///   <c>true</c> if the specified set contains the variable; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsNode(string id) => _map.ContainsKey(id);

        /// <summary>
        /// Determines whether the set contains any variable by a specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        ///   <c>true</c> if the set contains the variable; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(string id) => _unknowns.Exists(node => node.Name.Equals(id));

        /// <summary>
        /// Tries to get a variable.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="node">The found variable.</param>
        /// <returns>
        ///   <c>true</c> if the variable was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetNode(string id, out Variable node) => _map.TryGetValue(id, out node);

        /// <summary>
        /// Gets a mapped variable. If the node voltage does not exist, an exception will be thrown.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The node with the specified identifier.
        /// </returns>
        public Variable GetNode(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (_map.TryGetValue(id, out var result))
                return result;
            throw new CircuitException("Could not find node {0}".FormatString(id));
        }

        /// <summary>
        /// Gets a variable.
        /// </summary>
        /// <param name="id">string</param>
        /// <returns>Return the variable with the specified identifier, or <c>null</c> if it doesn't exist.</returns>
        public Variable GetVariable(string id) => _unknowns.FirstOrDefault(node => node.Name.Equals(id));

        /// <summary>
        /// Enumerates all variables.
        /// </summary>
        public IEnumerable<Variable> GetVariables() => _unknowns;

        /// <summary>
        /// Avoids any further additions of variables.
        /// </summary>
        /// <remarks>
        /// It is not possible to dynamically add and remove nodes while performing some operations (like most simulations).
        /// </remarks>
        public void Lock() => _locked = true;

        /// <summary>
        /// Clear all variables.
        /// </summary>
        public void Clear()
        {
            _unknowns.Clear();

            // Setup ground node
            _map.Clear();
            _map.Add(Ground.Name, Ground);
            _map.Add("GND", Ground);

            // Unlock
            _locked = false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Variable> GetEnumerator() => _unknowns.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
