using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Base parameters for a <see cref="Model"/>
    /// </summary>
    [GeneratedParameters]
    public class ModelBaseParameters : Common.ModelBaseParameters
    {
        private GivenParameter<double> _lambda;

        /// <summary>
        /// Gets the channel length modulation parameter.
        /// </summary>
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

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
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
