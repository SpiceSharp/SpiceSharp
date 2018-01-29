using System;
using SpiceSharp.IntegrationMethods;
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
        public StateDerivative Flux { get; }

        /// <summary>
        /// Get the state currently being operated on
        /// </summary>
        public State State { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inductance">Inductor</param>
        /// <param name="current">Current</param>
        public UpdateFluxEventArgs(double inductance, double current, StateDerivative flux, State state)
        {
            if (flux == null)
                throw new ArgumentNullException(nameof(flux));

            Inductance = inductance;
            Current = current;
            OriginalFlux = flux.Current;
            Flux = flux;
            State = state ?? throw new ArgumentNullException(nameof(state));
        }
    }
}
