using System;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// Common properties for transistor models.
    /// </summary>
    public class ModelProperties
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public double Factor1 { get; set; }
        public double Vtnom { get; set; }
        public double Kt1 { get; set; }
        public double EgFet1 { get; set; }
        public double PbFactor1 { get; set; }
        public double OxideCapFactor { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Updates the model properties.
        /// </summary>
        /// <param name="mp">The model parameters.</param>
        public void Update(ModelParameters mp)
        {
            OxideCapFactor = 3.9 * 8.854214871e-12 / mp.OxideThickness;
            Vtnom = mp.NominalTemperature * Constants.KOverQ;
            Factor1 = mp.NominalTemperature / Constants.ReferenceTemperature;
            Kt1 = Constants.Boltzmann * mp.NominalTemperature;
            EgFet1 = 1.16 - (7.02e-4 * mp.NominalTemperature * mp.NominalTemperature) / (mp.NominalTemperature + 1108);
            double arg1 = -EgFet1 / (Kt1 + Kt1) + 1.1150877 / (Constants.Boltzmann * Constants.ReferenceTemperature * 2);
            PbFactor1 = -2 * Vtnom * (1.5 * Math.Log(Factor1) + Constants.Charge * arg1);
        }
    }
}
