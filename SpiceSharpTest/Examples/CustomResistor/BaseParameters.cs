﻿using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.NonlinearResistorBehaviors
{
    /// <summary>
    /// Parameters for a <see cref="NonlinearResistor"/>
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        [ParameterName("a")]
        public double A { get; set; } = 1.0e3;

        [ParameterName("b")]
        public double B { get; set; } = 1.0;
    }
}
