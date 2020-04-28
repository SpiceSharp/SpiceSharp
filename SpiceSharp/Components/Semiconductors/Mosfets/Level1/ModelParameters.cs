using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfets.Level1
{
    /// <summary>
    /// Base parameters for a <see cref="Model"/>
    /// </summary>
    /// <seealso cref="Mosfets.ModelParameters"/>
    [GeneratedParameters]
    public class ModelParameters : Mosfets.ModelParameters
    {
        private GivenParameter<double> _lambda;

        /// <summary>
        /// Gets the channel length modulation parameter.
        /// </summary>
        /// <value>
        /// The channel length modulation parameter.
        /// </value>
        [ParameterName("lambda"), ParameterInfo("Channel length modulation")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> Lambda
        {
            get => _lambda;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Lambda), 0);
                _lambda = value;
            }
        }

        /// <inheritdoc/>
        public override void CalculateDefaults()
        {
            if (!OxideThickness.Given)
                OxideCapFactor = 0.0;
            else
            {
                OxideCapFactor = 3.9 * 8.854214871e-12 / OxideThickness;
                if (!Transconductance.Given)
                    Transconductance = new GivenParameter<double>(SurfaceMobility * OxideCapFactor * 1e-4, false); // m^2/cm^2
            }
        }
    }
}
