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
        [PropertyName("temp"), PropertyInfo("Instance temperature")]
        public double TemperatureCelsius
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter Temperature { get; } = new Parameter(300.15);
        [PropertyName("area"), PropertyInfo("Area factor")]
        public Parameter Area { get; } = new Parameter(1);
        [PropertyName("off"), PropertyInfo("Device initially off")]
        public bool Off { get; set; }
        [PropertyName("icvbe"), PropertyInfo("Initial B-E voltage")]
        public Parameter InitialVoltageBE { get; } = new Parameter();
        [PropertyName("icvce"), PropertyInfo("Initial C-E voltage")]
        public Parameter InitialVoltageCE { get; } = new Parameter();
        [PropertyName("sens_area"), PropertyInfo("flag to request sensitivity WRT area")]
        public bool Sensitivity { get; set; }

        [PropertyName("ic"), PropertyInfo("Initial condition vector")]
        public void SetIC(double[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (value.Length)
            {
                case 2: InitialVoltageCE.Set(value[1]); goto case 1;
                case 1: InitialVoltageBE.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
    }
}
