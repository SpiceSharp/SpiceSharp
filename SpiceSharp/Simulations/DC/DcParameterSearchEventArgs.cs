using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Event arguments for searching a parameter used as a sweep in DC analysis
    /// </summary>
    public class DCParameterSearchEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the name of the parameter
        /// </summary>
        public Identifier Name { get; }

        /// <summary>
        /// Gets the level of the sweep
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Gets or sets the found parameter
        /// </summary>
        public Parameter Result { get; set; }

        /// <summary>
        /// Gets or sets whether or not Temperature behaviors need to be run for every sweep point
        /// of the analysis
        /// </summary>
        public bool TemperatureNeeded { get; set; } = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the swept variable</param>
        /// <param name="level">Level (in nested sweeps)</param>
        public DCParameterSearchEventArgs(Identifier name, int level)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Level = level;
        }
    }
}
