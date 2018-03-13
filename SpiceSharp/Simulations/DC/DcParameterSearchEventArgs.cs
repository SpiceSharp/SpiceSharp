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
        /// Gets or sets the found parameter
        /// </summary>
        public Parameter Result { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public DCParameterSearchEventArgs(Identifier name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
