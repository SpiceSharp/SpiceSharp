using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfets.Level1
{
    /// <summary>
    /// Base parameters for a <see cref="Mosfet1Model"/>
    /// </summary>
    /// <seealso cref="Mosfets.ModelParameters"/>
    [GeneratedParameters]
    public partial class ModelParameters : Mosfets.ModelParameters, ICloneable<ModelParameters>
    {
        /// <summary>
        /// Gets the channel length modulation parameter.
        /// </summary>
        /// <value>
        /// The channel length modulation parameter.
        /// </value>
        [ParameterName("lambda"), ParameterInfo("Channel length modulation")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _lambda;

        /// <inheritdoc/>
        ModelParameters ICloneable<ModelParameters>.Clone()
            => (ModelParameters)MemberwiseClone();
    }
}
