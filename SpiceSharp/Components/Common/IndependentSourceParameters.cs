﻿using SpiceSharp.ParameterSets;
using System;
using System.Numerics;

namespace SpiceSharp.Components.CommonBehaviors
{
    /// <summary>
    /// Parameters that are common to an independent source.
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    [GeneratedParameters(AddNames = true, AddRules = false)]
    public partial class IndependentSourceParameters : ParameterSet
    {
        /// <summary>
        /// The DC value of the source.
        /// </summary>
        /// <value>
        /// The DC value.
        /// </value>
        [ParameterName("dc"), ParameterInfo("D.C. source value")]
        public GivenParameter<double> DcValue { get; set; }

        /// <summary>
        /// Gets or sets the waveform description.
        /// </summary>
        /// <value>
        /// The waveform description.
        /// </value>
        [ParameterName("waveform"), ParameterInfo("The waveform")]
        public IWaveformDescription Waveform { get; set; }

        /// <summary>
        /// Small-signal magnitude.
        /// </summary>
        /// <value>
        /// The small-signal magnitude.
        /// </value>
        [ParameterName("acmag"), ParameterInfo("AC magnitude value")]
        public double AcMagnitude { get; set; }

        /// <summary>
        /// Small-signal phase.
        /// </summary>
        /// <value>
        /// The small-signal phase.
        /// </value>
        [ParameterName("acphase"), ParameterInfo("AC phase value")]
        public double AcPhase { get; set; }

        /// <summary>
        /// Sets the small-signal parameters of the source.
        /// </summary>
        /// <param name="ac">Parameters.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="ac"/> does not have 0-2 arguments.</exception>
        [ParameterName("ac"), ParameterInfo("A.C. magnitude, phase vector")]
        public void SetAc(double[] ac)
        {
            ac.ThrowIfNotLength(nameof(ac), 0, 2);
            switch (ac.Length)
            {
                case 2:
                    AcPhase = ac[1];
                    goto case 1;
                case 1:
                    AcMagnitude = ac[0];
                    break;
                case 0:
                    AcMagnitude = 0.0;
                    break;
            }
        }

        /// <summary>
        /// Gets the phasor represented by the amplitude and phase.
        /// </summary>
        /// <value>
        /// The complex phasor.
        /// </value>
        public Complex Phasor { get; private set; }

        /// <summary>
        /// Updates the phasor.
        /// </summary>
        public void UpdatePhasor()
        {
            var phase = AcPhase * Math.PI / 180.0;
            Phasor = new Complex(
                AcMagnitude * Math.Cos(phase),
                AcMagnitude * Math.Sin(phase));
        }
    }
}
