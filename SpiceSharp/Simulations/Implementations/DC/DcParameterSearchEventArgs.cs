using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Event arguments for searching a parameter used as a sweep in DC analysis.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class DCParameterSearchEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the level of the sweep.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Gets or sets the found parameter.
        /// </summary>
        public Parameter<double> Result { get; set; }

        /// <summary>
        /// Gets or sets whether or not Temperature behaviors need to be run for every sweep point
        /// of the analysis
        /// </summary>
        public bool TemperatureNeeded { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="DCParameterSearchEventArgs"/> class.
        /// </summary>
        /// <param name="name">The identifier of the parameter.</param>
        /// <param name="level">The sweep level.</param>
        public DCParameterSearchEventArgs(string name, int level)
        {
            Name = name.ThrowIfNull(nameof(name));
            Level = level;
        }
    }
}
