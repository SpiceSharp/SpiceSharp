namespace SpiceSharp
{
    /// <summary>
    /// Interface describing a set of parameters.
    /// </summary>
    /// <seealso cref="ICloneable" />
    public interface IParameterSet : ICloneable, INamedParameters
    {
        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        void CalculateDefaults();
    }
}
