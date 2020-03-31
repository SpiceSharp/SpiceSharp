using System.Collections.Generic;

namespace SpiceSharp.Simulations.Variables
{
    /// <summary>
    /// A simple variable factory where the variables don't have any extra functionality.
    /// </summary>
    /// <seealso cref="IVariableFactory{V}" />
    public class VariableFactory : IVariableFactory<IVariable>
    {
        /// <summary>
        /// Gets all shared variables.
        /// </summary>
        /// <value>
        /// The shared variables.
        /// </value>
        public IVariableSet<IVariable> Variables { get; }

        IVariableSet IVariableFactory.Variables => Variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableFactory"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public VariableFactory(IEqualityComparer<string> comparer = null)
        {
            Variables = new VariableSet<IVariable>(comparer);
        }

        /// <summary>
        /// Maps a shared node in the simulation.
        /// </summary>
        /// <param name="name">The name of the shared node.</param>
        /// <returns>
        /// The shared node variable.
        /// </returns>
        public IVariable GetSharedVariable(string name)
        {
            if (Variables.TryGetValue(name, out var result))
                return result;
            result = new Variable(name, Units.Volt);
            Variables.Add(result);
            return result;
        }

        /// <summary>
        /// Creates a local variable that should not be shared by the state with anyone else.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="unit">The unit of the variable.</param>
        /// <returns>
        /// The local variable.
        /// </returns>
        public IVariable CreatePrivateVariable(string name, IUnit unit) => new Variable(name, unit);
    }
}
