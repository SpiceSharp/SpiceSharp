using System;

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
        public double OriginalCurrent { get; }

        /// <summary>
        /// New flux through the inductor (initially set to the original flux)
        /// </summary>
        public double Flux { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="flux">Flux through the capacitor</param>
        public UpdateFluxEventArgs(double ind, double i, double flux)
        {
            Inductance = ind;
            OriginalCurrent = i;
            Flux = flux;
            OriginalFlux = flux;
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
