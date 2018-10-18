using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Base parameters for a <see cref="Mosfet1"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }
        [ParameterName("icvbs"), ParameterInfo("Initial B-S voltage")]
        public GivenParameter<double> InitialVoltageBs { get; } = new GivenParameter<double>();
        [ParameterName("icvds"), ParameterInfo("Initial D-S voltage")]
        public GivenParameter<double> InitialVoltageDs { get; } = new GivenParameter<double>();
        [ParameterName("icvgs"), ParameterInfo("Initial G-S voltage")]
        public GivenParameter<double> InitialVoltageGs { get; } = new GivenParameter<double>();
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance temperature")]
        public double TemperatureCelsius
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Value = value + Circuit.CelsiusKelvin;
        }
        public GivenParameter<double> Temperature { get; } = new GivenParameter<double>(Circuit.ReferenceTemperature);
        [ParameterName("w"), ParameterInfo("Width")]
        public GivenParameter<double> Width { get; } = new GivenParameter<double>(1e-4);
        [ParameterName("l"), ParameterInfo("Length")]
        public GivenParameter<double> Length { get; } = new GivenParameter<double>(1e-4);
        [ParameterName("as"), ParameterInfo("Source area")]
        public GivenParameter<double> SourceArea { get; } = new GivenParameter<double>();
        [ParameterName("ad"), ParameterInfo("Drain area")]
        public GivenParameter<double> DrainArea { get; } = new GivenParameter<double>();
        [ParameterName("ps"), ParameterInfo("Source perimeter")]
        public GivenParameter<double> SourcePerimeter { get; } = new GivenParameter<double>();
        [ParameterName("pd"), ParameterInfo("Drain perimeter")]
        public GivenParameter<double> DrainPerimeter { get; } = new GivenParameter<double>();
        [ParameterName("nrs"), ParameterInfo("Source squares")]
        public GivenParameter<double> SourceSquares { get; } = new GivenParameter<double>(1);
        [ParameterName("nrd"), ParameterInfo("Drain squares")]
        public GivenParameter<double> DrainSquares { get; } = new GivenParameter<double>(1);

        /// <summary>
        /// Methods
        /// </summary>
        [ParameterName("ic"), ParameterInfo("Vector of D-S, G-S, B-S voltages")]
        public void SetIc(double[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (value.Length)
            {
                case 3: 
                    InitialVoltageBs.Value = value[2];
                    goto case 2;
                case 2: 
                    InitialVoltageGs.Value = value[1]; 
                    goto case 1;
                case 1: 
                    InitialVoltageDs.Value = value[0]; 
                    break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
    }
}
