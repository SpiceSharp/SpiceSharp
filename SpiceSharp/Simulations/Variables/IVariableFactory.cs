using SpiceSharp.Simulations.Variables;
using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for a variable factory.
    /// </summary>
    /// <typeparam name="V">The base variable type.</typeparam>
    /// <seealso cref="IVariable"/>
    public interface IVariableFactory<out V> where V : IVariable
    {
        /// <summary>
        /// Gets a variable that can be shared with other behaviors by the factory. If another variable
        /// already exists with the same name, that is returned instead.
        /// </summary>
        /// <param name="name">The name of the shared variable.</param>
        /// <returns>
        /// The shared variable.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        V GetSharedVariable(string name);

        /// <summary>
        /// Creates a variable that is private to whoever requested it. The factory will not shared this
        /// variable with anyone else, and the name is only used for display purposes.
        /// </summary>
        /// <param name="name">The name of the private variable.</param>
        /// <param name="unit">The unit of the variable.</param>
        /// <returns>
        /// The private variable.
        /// </returns>
        V CreatePrivateVariable(string name, IUnit unit);
    }
}
