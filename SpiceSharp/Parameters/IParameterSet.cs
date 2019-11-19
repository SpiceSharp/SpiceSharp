namespace SpiceSharp
{
    /// <summary>
    /// Interface for a set of named parameters that can be read or written.
    /// </summary>
    /// <seealso cref="IImportParameterSet" />
    /// <seealso cref="IExportParameterSet" />
    /// <seealso cref="ICloneable" />
    public interface IParameterSet : IImportParameterSet, IExportParameterSet, ICloneable
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
