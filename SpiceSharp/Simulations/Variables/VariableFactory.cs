using System.Collections.Generic;

namespace SpiceSharp.Simulations.Variables
{
    /// <summary>
    /// A simple variable factory where the variables don't have any extra functionality.
    /// </summary>
    /// <seealso cref="IVariableFactory{V}" />
    public class VariableFactory : IVariableFactory<IVariable>
    {
        private readonly VariableSet<IVariable> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableFactory"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public VariableFactory(IEqualityComparer<string> comparer = null)
        {
            _variables = new VariableSet<IVariable>(comparer);
        }

        /// <summary>
        /// Maps a shared node in the simulation.
        /// </summary>
        /// <param name="name">The name of the shared node.</param>
        /// <returns>
        /// The shared node variable.
        /// </returns>
        public IVariable MapNode(string name)
        {
            if (_variables.TryGetValue(name, out var result))
                return result;
            result = new Variable(name, Units.Volt);
            _variables.Add(result);
            return result;
        }

        /// <summary>
        /// Maps a number of nodes.
        /// </summary>
        /// <param name="names">The nodes.</param>
        /// <returns>
        /// The shared node variables.
        /// </returns>
        public IEnumerable<IVariable> MapNodes(IEnumerable<string> names)
        {
            foreach (var name in names)
                yield return MapNode(name);
        }

        /// <summary>
        /// Creates a local variable that should not be shared by the state with anyone else.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="unit">The unit of the variable.</param>
        /// <returns>
        /// The local variable.
        /// </returns>
        public IVariable Create(string name, IUnit unit) => new Variable(name, unit);

        /// <summary>
        /// Determines whether the specified variable is a node without mapping it.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <returns>
        /// <c>true</c> if the specified variable has node; otherwise, <c>false</c>.
        /// </returns>
        public bool HasNode(string name) => _variables.ContainsKey(name);
    }
}
