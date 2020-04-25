using System;

namespace SpiceSharp
{
    /// <summary>
    /// Interface for a set of named parameters that can be read or written.
    /// </summary>
    /// <seealso cref="IImportParameterSet" />
    /// <seealso cref="IExportPropertySet" />
    /// <seealso cref="ICloneable" />
    public interface IParameterSet : IImportParameterSet, IExportPropertySet, ICloneable
    {
        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if the default values canot be calculated using the current parameters.</exception>
        void CalculateDefaults();
    }
}
