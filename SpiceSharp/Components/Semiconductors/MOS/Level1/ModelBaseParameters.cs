using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Base parameters for a <see cref="Model"/>
    /// </summary>
    public class ModelBaseParameters : Common.ModelBaseParameters
    {
        /// <summary>
        /// Gets the channel length modulation parameter.
        /// </summary>
        [ParameterName("lambda"), ParameterInfo("Channel length modulation")]
        public GivenParameter<double> Lambda { get; } = new GivenParameter<double>();

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public override void CalculateDefaults()
        {
            if (!OxideThickness.Given || OxideThickness.Value <= 0.0)
                OxideCapFactor = 0.0;
            else
            {
                OxideCapFactor = 3.9 * 8.854214871e-12 / OxideThickness;
                if (!Transconductance.Given)
                    Transconductance.RawValue = SurfaceMobility * OxideCapFactor * 1e-4; // m^2/cm^2
            }
        }
    }
}
