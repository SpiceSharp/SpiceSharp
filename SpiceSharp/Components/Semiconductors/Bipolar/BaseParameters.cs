using System;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

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
        [ParameterName("temp"), ParameterInfo("Instance temperature")]
        public double TemperatureCelsius
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter Temperature { get; } = new Parameter(300.15);
        [ParameterName("area"), ParameterInfo("Area factor")]
        public Parameter Area { get; } = new Parameter(1);
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }
        [ParameterName("icvbe"), ParameterInfo("Initial B-E voltage")]
        public Parameter InitialVoltageBe { get; } = new Parameter();
        [ParameterName("icvce"), ParameterInfo("Initial C-E voltage")]
        public Parameter InitialVoltageCe { get; } = new Parameter();
        [ParameterName("sens_area"), ParameterInfo("flag to request sensitivity WRT area")]
        public bool Sensitivity { get; set; }

        [ParameterName("ic"), ParameterInfo("Initial condition vector")]
        public void SetIc(double[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (value.Length)
            {
                case 2: InitialVoltageCe.Set(value[1]); goto case 1;
                case 1: InitialVoltageBe.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
    }
}
