using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// Arguments used to modify flux through an <see cref="Inductor"/>
    /// </summary>
    public class UpdateFluxEventArgs : EventArgs
    {
        /// <summary>
        /// Inductance of the inductor
        /// </summary>
        public double Inductance { get; }

        /// <summary>
        /// Original flux through the inductor
        /// </summary>
        public double OriginalFlux { get; }

        /// <summary>
        /// Original current through the inductor
        /// </summary>
        public double Current { get; }

        /// <summary>
        /// New flux through the inductor (initially set to the original flux)
        /// </summary>
        public IDerivative Flux { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateFluxEventArgs"/> class.
        /// </summary>
        /// <param name="inductance">Inductor</param>
        /// <param name="current">Current</param>
        /// <param name="flux">Flux</param>
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
