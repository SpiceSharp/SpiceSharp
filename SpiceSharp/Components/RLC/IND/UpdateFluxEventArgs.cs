using System;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.IND
{
    /// <summary>
    /// Arguments used to modify flux through an inductor
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
        /// <param name="ind">Inductor</param>
        /// <param name="i">Current</param>
        public UpdateFluxEventArgs(double ind, double i, StateDerivative flux, State state)
        {
            Inductance = ind;
            Current = i;
            OriginalFlux = flux.Value;
            Flux = flux;
            State = state;
        }
    }

    /// <summary>
    /// Delegate used for updating flux
    /// </summary>
    /// <param name="sender">Behavior</param>
    /// <param name="args">Arguments</param>
    /// <returns></returns>
    public delegate void UpdateFluxEventHandler(object sender, UpdateFluxEventArgs args);
}
