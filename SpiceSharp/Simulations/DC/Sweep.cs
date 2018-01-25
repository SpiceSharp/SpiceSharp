﻿using System;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that describes a job
    /// </summary>
    public class Sweep
    {
        /// <summary>
        /// Starting value
        /// </summary>
        [PropertyName("start"), PropertyInfo("The starting value")]
        public double Start { get; set; }

        /// <summary>
        /// Ending value
        /// </summary>
        [PropertyName("stop"), PropertyInfo("The stopping value")]
        public double Stop { get; set; }

        /// <summary>
        /// Value step
        /// </summary>
        [PropertyName("step"), PropertyInfo("The step")]
        public double Step { get; set; }

        /// <summary>
        /// The name of the source being varied
        /// </summary>
        [PropertyName("source"), PropertyInfo("The name of the swept source")]
        public Identifier ComponentName { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the source to sweep</param>
        /// <param name="start">The starting value</param>
        /// <param name="stop">The stopping value</param>
        /// <param name="step">The step value</param>
        public Sweep(Identifier name, double start, double stop, double step) : base()
        {
            ComponentName = name;
            Start = start;
            Stop = stop;
            Step = step;
        }
    }
}