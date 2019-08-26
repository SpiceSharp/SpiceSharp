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

        /// <summary>
        /// Gets or sets a flag indicating whether the simulation should clone all parameters.
        /// </summary>
        /// <remarks>
        /// This is mainly useful when using the same circuit for multiple simulations and
        /// running them in multiple threads.
        /// </remarks>
        public bool CloneParameters { get; set; }
    }
}
