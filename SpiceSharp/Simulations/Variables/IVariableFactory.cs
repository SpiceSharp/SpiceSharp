using SpiceSharp.Simulations.Variables;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for a variable factory.
    /// </summary>
    /// <typeparam name="V">The base variable type.</typeparam>
    public interface IVariableFactory<V> : IVariableFactory where V : IVariable
    {
        /// <summary>
        /// Gets all shared variables.
        /// </summary>
        /// <value>
        /// The shared variables.
        /// </value>
        new IVariableSet<V> Variables { get; }

        /// <summary>
        /// Gets a variable that can be shared with other behaviors by the factory. If another variable
        /// already exists with the same name, that is returned instead.
        /// </summary>
        /// <param name="name">The name of the shared variable.</param>
        /// <returns>
        /// The shared variable.
        /// </returns>
        new V GetSharedVariable(string name);

        /// <summary>
        /// Creates a variable that is private to whoever requested it. The factory will not shared this
        /// variable with anyone else, and the name is only used for display purposes.
        /// </summary>
        /// <param name="name">The name of the private variable.</param>
        /// <param name="unit">The unit of the variable.</param>
        /// <returns>
        /// The private variable.
        /// </returns>
        new V CreatePrivateVariable(string name, IUnit unit);
    }

    /// <summary>
    /// A variable factory that is not strongly typed.
    /// </summary>
    /// <seealso cref="IVariableFactory" />
    public interface IVariableFactory
    {
        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        IVariableSet Variables { get; }

        /// <summary>
        /// Gets a variable that can be shared with other behaviors by the factory. If another variable
        /// already exists with the same name, that is returned instead.
        /// </summary>
        /// <param name="name">The name of the shared variable.</param>
        /// <returns>
        /// The shared variable.
        /// </returns>
        IVariable GetSharedVariable(string name);

        /// <summary>
        /// Creates a variable that is private to whoever requested it. The factory will not shared this
        /// variable with anyone else, and the name is only used for display purposes.
        /// </summary>
        /// <param name="name">The name of the private variable.</param>
        /// <param name="unit">The unit of the variable.</param>
        /// <returns>
        /// The private variable.
        /// </returns>
        IVariable CreatePrivateVariable(string name, IUnit unit);
    }
}
