using System;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Base parameters for a <see cref="MOS1"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("off"), PropertyInfo("Device initially off")]
        public bool MOS1off { get; set; }
        [PropertyName("icvbs"), PropertyInfo("Initial B-S voltage")]
        public Parameter MOS1icVBS { get; } = new Parameter();
        [PropertyName("icvds"), PropertyInfo("Initial D-S voltage")]
        public Parameter MOS1icVDS { get; } = new Parameter();
        [PropertyName("icvgs"), PropertyInfo("Initial G-S voltage")]
        public Parameter MOS1icVGS { get; } = new Parameter();
        [PropertyName("temp"), PropertyInfo("Instance temperature")]
        public double MOS1_TEMP
        {
            get => MOS1temp - Circuit.CelsiusKelvin;
            set => MOS1temp.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter MOS1temp { get; } = new Parameter();
        [PropertyName("w"), PropertyInfo("Width")]
        public Parameter MOS1w { get; } = new Parameter(1e-4);
        [PropertyName("l"), PropertyInfo("Length")]
        public Parameter MOS1l { get; } = new Parameter(1e-4);
        [PropertyName("as"), PropertyInfo("Source area")]
        public Parameter MOS1sourceArea { get; } = new Parameter();
        [PropertyName("ad"), PropertyInfo("Drain area")]
        public Parameter MOS1drainArea { get; } = new Parameter();
        [PropertyName("ps"), PropertyInfo("Source perimeter")]
        public Parameter MOS1sourcePerimiter { get; } = new Parameter();
        [PropertyName("pd"), PropertyInfo("Drain perimeter")]
        public Parameter MOS1drainPerimiter { get; } = new Parameter();
        [PropertyName("nrs"), PropertyInfo("Source squares")]
        public Parameter MOS1sourceSquares { get; } = new Parameter(1);
        [PropertyName("nrd"), PropertyInfo("Drain squares")]
        public Parameter MOS1drainSquares { get; } = new Parameter(1);

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("ic"), PropertyInfo("Vector of D-S, G-S, B-S voltages")]
        public void SetIC(double[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (value.Length)
            {
                case 3: MOS1icVBS.Set(value[2]); goto case 2;
                case 2: MOS1icVGS.Set(value[1]); goto case 1;
                case 1: MOS1icVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
    }
}
