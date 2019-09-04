namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can configure how collections are created in simulations.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class CollectionConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets or sets the variables. If null, the default is used (<see cref="VariableSet"/>).
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IVariableSet Variables { get; set; }
    }
}
