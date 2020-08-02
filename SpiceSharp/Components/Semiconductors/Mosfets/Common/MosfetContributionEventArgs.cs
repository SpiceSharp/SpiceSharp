using System;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// Event arguments that are used to add contributions for mosfets.
    /// </summary>
    public class MosfetContributionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the mosfet contributions.
        /// </summary>
        /// <value>
        /// The mosfet contributions.
        /// </value>
        public Contributions<double> Contributions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MosfetContributionEventArgs"/> class.
        /// </summary>
        /// <param name="contributions">The contributions.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="contributions"/> is <c>null</c>.</exception>
        public MosfetContributionEventArgs(Contributions<double> contributions)
        {
            Contributions = contributions.ThrowIfNull(nameof(contributions));
        }
    }
}
