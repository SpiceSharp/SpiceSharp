using System;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// Event arguments used when a rule has been violated.
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class RuleViolationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether this rule violation can be ignored.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the rule needs to be ignored; otherwise, <c>false</c>.
        /// </value>
        public bool Ignore { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleViolationEventArgs"/> class.
        /// </summary>
        public RuleViolationEventArgs()
        {
            Ignore = false;
        }
    }
}
