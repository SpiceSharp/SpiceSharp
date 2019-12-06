using SpiceSharp.Simulations;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A variable set that will automatically make variables local by prefixing the names.
    /// </summary>
    /// <seealso cref="IVariableSet" />
    public class SubcircuitVariableSet : IVariableSet
    {
        private string _name;
        private IVariableSet _parent;

        /// <summary>
        /// Gets the <see cref="Variable"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="Variable"/>.
        /// </value>
        /// <param name="id">The name.</param>
        /// <returns></returns>
        public Variable this[string id] => _parent[_name.Combine(id)];

        /// <summary>
        /// Gets the number of variables.
        /// </summary>
        /// <value>
        /// The number of variables.
        /// </value>
        public int Count => _parent.Count;

        /// <summary>
        /// Gets the ground variable.
        /// </summary>
        /// <value>
        /// The ground.
        /// </value>
        public Variable Ground => _parent.Ground;

        /// <summary>
        /// Gets the comparer used for variables.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer => _parent.Comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitVariableSet"/> class.
        /// </summary>
        /// <param name="name">The subcircuit name.</param>
        /// <param name="parent">The parent variable set.</param>
        public SubcircuitVariableSet(string name, IVariableSet parent)
        {
            _name = name.ThrowIfNull(nameof(name));
            _parent = parent.ThrowIfNull(nameof(parent));
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
        public void AliasNode(Variable variable, string alias)
            => _parent.AliasNode(variable, _name.Combine(alias));

        /// <summary>
        /// Clears the set from any variables.
        /// </summary>
        public void Clear() => _parent.Clear();

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="id">The name.</param>
        /// <returns>
        ///   <c>true</c> if the set contains the variable; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(string id) => _parent.Contains(_name.Combine(id));

        /// <summary>
        /// Determines whether the set contains a mapped variable by a specified name.
        /// </summary>
        /// <param name="id">The name.</param>
        /// <returns>
        ///   <c>true</c> if the specified set contains the variable; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsNode(string id) => _parent.ContainsNode(_name.Combine(id));

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        /// <param name="id">The name of the new variable.</param>
        /// <param name="type">The type of the variable.</param>
        /// <returns>
        /// A new variable.
        /// </returns>
        /// <remarks>
        /// Variables created using this method cannot be found back using the method <see cref="MapNode(string,VariableType)" />.
        /// </remarks>
        public Variable Create(string id, VariableType type) => _parent.Create(_name.Combine(id), type);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Variable> GetEnumerator() => _parent.GetEnumerator();

        /// <summary>
        /// This method maps a variable in the circuit. If a variable with the same name already exists, then that variable is returned.
        /// </summary>
        /// <param name="id">The name of the variable.</param>
        /// <param name="type">The type of the variable.</param>
        /// <returns>
        /// A new variable with the specified name and type, or a previously mapped variable if it already existed.
        /// </returns>
        /// <remarks>
        /// If the variable already exists, the variable type is ignored.
        /// </remarks>
        public Variable MapNode(string id, VariableType type)
        {
            if (_parent.TryGetNode(id, out var result) && result == _parent.Ground)
                return result;
            return _parent.MapNode(_name.Combine(id), type);
        }

        /// <summary>
        /// Tries to get a variable.
        /// </summary>
        /// <param name="id">The name.</param>
        /// <param name="node">The found variable.</param>
        /// <returns>
        ///   <c>true</c> if the variable was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetNode(string id, out Variable node)
        {
            if (_parent.TryGetNode(id, out var result) && result == _parent.Ground)
            {
                node = result;
                return true;
            }
            return _parent.TryGetNode(_name.Combine(id), out node);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => _parent.GetEnumerator();
    }
}
