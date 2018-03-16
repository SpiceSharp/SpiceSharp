using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Base parameters for a <see cref="Mosfet3"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("w"), ParameterInfo("Width")]
        public GivenParameter Width { get; } = new GivenParameter(1e-4);
        [ParameterName("l"), ParameterInfo("Length")]
        public GivenParameter Length { get; } = new GivenParameter(1e-4);
        [ParameterName("as"), ParameterInfo("Source area")]
        public GivenParameter SourceArea { get; } = new GivenParameter();
        [ParameterName("ad"), ParameterInfo("Drain area")]
        public GivenParameter DrainArea { get; } = new GivenParameter();
        [ParameterName("ps"), ParameterInfo("Source perimeter")]
        public GivenParameter SourcePerimeter { get; } = new GivenParameter();
        [ParameterName("pd"), ParameterInfo("Drain perimeter")]
        public GivenParameter DrainPerimeter { get; } = new GivenParameter();
        [ParameterName("nrs"), ParameterInfo("Source squares")]
        public GivenParameter SourceSquares { get; } = new GivenParameter(1);
        [ParameterName("nrd"), ParameterInfo("Drain squares")]
        public GivenParameter DrainSquares { get; } = new GivenParameter(1);
        [ParameterName("temp"), ParameterInfo("Instance operating temperature")]
        public double TemperatureCelsius
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Value = value + Circuit.CelsiusKelvin;
        }
        public GivenParameter Temperature { get; } = new GivenParameter();

        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }
        [ParameterName("icvbs"), ParameterInfo("Initial B-S voltage")]
        public GivenParameter InitialVoltageBs { get; } = new GivenParameter();
        [ParameterName("icvds"), ParameterInfo("Initial D-S voltage")]
        public GivenParameter InitialVoltageDs { get; } = new GivenParameter();
        [ParameterName("icvgs"), ParameterInfo("Initial G-S voltage")]
        public GivenParameter InitialVoltageGs { get; } = new GivenParameter();

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
                case 3: InitialVoltageBs.Value = value[2]; goto case 2;
                case 2: InitialVoltageGs.Value = value[1]; goto case 1;
                case 1: InitialVoltageDs.Value = value[0]; break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
    }
}
