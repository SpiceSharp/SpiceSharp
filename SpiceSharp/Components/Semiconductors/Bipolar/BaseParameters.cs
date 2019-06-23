using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance temperature")]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature.Value = value + Constants.CelsiusKelvin;
        }
        public GivenParameter<double> Temperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);
        [ParameterName("area"), ParameterInfo("Area factor")]
        public GivenParameter<double> Area { get; } = new GivenParameter<double>(1);
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }
        [ParameterName("icvbe"), ParameterInfo("Initial B-E voltage")]
        public GivenParameter<double> InitialVoltageBe { get; } = new GivenParameter<double>();
        [ParameterName("icvce"), ParameterInfo("Initial C-E voltage")]
        public GivenParameter<double> InitialVoltageCe { get; } = new GivenParameter<double>();
        [ParameterName("sens_area"), ParameterInfo("flag to request sensitivity WRT area")]
        public bool Sensitivity { get; set; }

        [ParameterName("ic"), ParameterInfo("Initial condition vector")]
        public void SetIc(double[] value)
        {
            value.ThrowIfNull(nameof(value));

            switch (value.Length)
            {
                case 2:
                    InitialVoltageCe.Value = value[1];
                    goto case 1;
                case 1:
                    InitialVoltageBe.Value = value[0];
                    break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
    }
}
