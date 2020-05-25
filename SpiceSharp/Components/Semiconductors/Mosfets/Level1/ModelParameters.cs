using SpiceSharp.ParameterSets;

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
    }
}
