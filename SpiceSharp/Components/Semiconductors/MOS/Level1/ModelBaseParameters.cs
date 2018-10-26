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
        /// <value>
        /// The channel length modulation parameter.
        /// </value>
        [ParameterName("lambda"), ParameterInfo("Channel length modulation")]
        public GivenParameter<double> Lambda { get; } = new GivenParameter<double>();

        public override void CalculateDefaults()
        {
            base.CalculateDefaults();

            if (!OxideThickness.Given)
                OxideCapFactor = 0.0;
        }
    }
}
