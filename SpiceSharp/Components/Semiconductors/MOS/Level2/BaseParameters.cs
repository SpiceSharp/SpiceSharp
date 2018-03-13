using System;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// Base parameters for a <see cref="Mosfet2"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("temp"), PropertyInfo("Instance operating temperature")]
        public double TemperatureCelsius
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter Temperature { get; } = new Parameter();
        [ParameterName("w"), PropertyInfo("Width")]
        public Parameter Width { get; } = new Parameter(1e-4);
        [ParameterName("l"), PropertyInfo("Length")]
        public Parameter Length { get; } = new Parameter(1e-4);
        [ParameterName("as"), PropertyInfo("Source area")]
        public Parameter SourceArea { get; } = new Parameter();
        [ParameterName("ad"), PropertyInfo("Drain area")]
        public Parameter DrainArea { get; } = new Parameter();
        [ParameterName("ps"), PropertyInfo("Source perimeter")]
        public Parameter SourcePerimeter { get; } = new Parameter();
        [ParameterName("pd"), PropertyInfo("Drain perimeter")]
        public Parameter DrainPerimeter { get; } = new Parameter();
        [ParameterName("nrs"), PropertyInfo("Source squares")]
        public Parameter SourceSquares { get; } = new Parameter(1);
        [ParameterName("nrd"), PropertyInfo("Drain squares")]
        public Parameter DrainSquares { get; } = new Parameter(1);
        [ParameterName("off"), PropertyInfo("Device initially off")]
        public bool Off { get; set; }
        [ParameterName("icvbs"), PropertyInfo("Initial B-S voltage")]
        public Parameter InitialVoltageBs { get; } = new Parameter();
        [ParameterName("icvds"), PropertyInfo("Initial D-S voltage")]
        public Parameter InitialVoltageDs { get; } = new Parameter();
        [ParameterName("icvgs"), PropertyInfo("Initial G-S voltage")]
        public Parameter InitialVoltageGs { get; } = new Parameter();
        [ParameterName("ic"), PropertyInfo("Vector of D-S, G-S, B-S voltages")]
        public void SetIc(double[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (value.Length)
            {
                case 3: InitialVoltageBs.Set(value[2]); goto case 2;
                case 2: InitialVoltageGs.Set(value[1]); goto case 1;
                case 1: InitialVoltageDs.Set(value[0]); break;
                default:
                    throw new BadParameterException("ic");
            }
        }
    }
}
