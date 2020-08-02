using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Inductors
{
    /// <summary>
    /// Event arguments used to modify flux through an <see cref="Inductor"/>.
    /// </summary>
    /// <seealso cref="EventArgs"/>
    public class UpdateFluxEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the inductance of the inductor
        /// </summary>
        /// <value>
        /// The inductance of the inductor.
        /// </value>
        public double Inductance { get; }

        /// <summary>
        /// Gets the original flux through the inductor
        /// </summary>
        /// <value>
        /// The original flux through the inductor.
        /// </value>
        public double OriginalFlux { get; }

        /// <summary>
        /// Gets the original current through the inductor
        /// </summary>
        /// <value>
        /// The original current through the inductor.
        /// </value>
        public double Current { get; }

        /// <summary>
        /// Gets the <see cref="IDerivative"/> that represents the flux through the inductor (initially set to the original flux).
        /// </summary>
        /// <value>
        /// The flux of the inductor.
        /// </value>
        /// <remarks>
        /// Change this value if you want to change the flux through the inductor.
        /// </remarks>
        public IDerivative Flux { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateFluxEventArgs"/> class.
        /// </summary>
        /// <param name="inductance">The current inductance.</param>
        /// <param name="current">The current.</param>
        /// <param name="flux">Flux</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flux"/> is <c>null</c>.</exception>
        public UpdateFluxEventArgs(double inductance, double current, IDerivative flux)
        {
            flux.ThrowIfNull(nameof(flux));

            Inductance = inductance;
            Current = current;
            OriginalFlux = flux.Value;
            Flux = flux;
        }
    }
}
