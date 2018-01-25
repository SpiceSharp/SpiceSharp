﻿using System.Collections.Generic;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Configuration for a <see cref="DC"/>
    /// </summary>
    public class DCConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets the list of sweeps that need to be executed
        /// </summary>
        [PropertyNameAttribute("sweeps"), PropertyInfoAttribute("List of sweeps")]
        public List<Sweep> Sweeps { get; } = new List<Sweep>();

        /// <summary>
        /// Number of iterations for DC sweeps
        /// </summary>
        public int SweepMaxIterations = 20;

    }
}
