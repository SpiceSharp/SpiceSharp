using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentSwitchModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the resistance parameter when closed.
        /// </summary>
        [ParameterName("ron"), ParameterInfo("Closed resistance")]
        public double OnResistance { get; set; } = 1;

        /// <summary>
        /// Gets the resistance parameter when open.
        /// </summary>
        [ParameterName("roff"), ParameterInfo("Open resistance")]
        public double OffResistance { get; set; } = 1e12;

        /// <summary>
        /// Gets the threshold parameter.
        /// </summary>
        public virtual double Threshold { get; set; }

        /// <summary>
        /// Gets the hysteresis parameter.
        /// </summary>
        public virtual double Hysteresis { get; set; }

        /// <summary>
        /// Gets the on conductance.
        /// </summary>
        public double OnConductance { get; private set; }

        /// <summary>
        /// Gets the off conductance.
        /// </summary>
        public double OffConductance { get; private set; }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        public override void CalculateDefaults()
        {
            // Only positive hysteresis values!
            if (Hysteresis < 0)
                Hysteresis = -Hysteresis;
            OnConductance = 1.0 / OnResistance;
            OffConductance = 1.0 / OffResistance;
        }
    }
}
