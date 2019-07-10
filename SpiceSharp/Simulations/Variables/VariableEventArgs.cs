using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Arguments for events that pass a <see cref="Variable"/>.
    /// </summary>
    public class VariableEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the variable.
        /// </summary>
        public Variable Variable { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableEventArgs"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public VariableEventArgs(Variable variable)
        {
            Variable = variable;
        }
    }
}
