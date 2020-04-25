using System.Collections.Generic;

namespace SpiceSharp.Simulations.Variables
{
    /// <summary>
    /// A simple variable factory where the variables don't have any extra functionality.
    /// </summary>
    /// <seealso cref="VariableDictionary{V}"/>
    /// <seealso cref="IVariableFactory{V}" />
    /// <seealso cref="IVariable"/>
    public class VariableFactory : VariableDictionary<IVariable>, IVariableFactory<IVariable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableFactory"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public VariableFactory(IEqualityComparer<string> comparer = null)
            : base(comparer)
        {
        }

        /// <inheritdoc/>
        public IVariable GetSharedVariable(string name)
        {
            if (TryGetValue(name, out var result))
                return result;
            result = new Variable(name, Units.Volt);
            Add(name, result);
            return result;
        }

        /// <inheritdoc/>
        public IVariable CreatePrivateVariable(string name, IUnit unit) => new Variable(name, unit);
    }
}
