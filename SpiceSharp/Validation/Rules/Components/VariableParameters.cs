using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Validation.Rules
{
    /// <summary>
    /// Parameters for <see cref="IRule"/> for variables.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class VariableParameters : ParameterSet
    {
        /// <summary>
        /// Gets the variable set used.
        /// </summary>
        /// <value>
        /// The variable set.
        /// </value>
        [ParameterName("variables"), ParameterInfo("The variable set for which validation is executed.")]
        public IVariableSet Variables { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableParameters"/> class.
        /// </summary>
        public VariableParameters()
        {
            Variables = new VariableSet();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableParameters"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public VariableParameters(ISimulation simulation)
        {
            Variables = simulation.Variables;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableParameters"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public VariableParameters(IVariableSet variables)
        {
            Variables = variables;
        }
    }
}
