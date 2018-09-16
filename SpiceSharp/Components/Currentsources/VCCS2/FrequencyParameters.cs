﻿using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors2
{
    /// <summary>
    /// AC parameters for a <see cref="CurrentSource"/>
    /// </summary>
    public class FrequencyParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("acmag"), ParameterInfo("A.C. magnitude value")]
        public GivenParameter<double> AcMagnitude { get; } = new GivenParameter<double>();
        [ParameterName("acphase"), ParameterInfo("A.C. phase value")]
        public GivenParameter<double> AcPhase { get; } = new GivenParameter<double>();
        [ParameterName("ac"), ParameterInfo("A.C. magnitude, phase vector")]
        public void SetAc(double[] ac)
        {
            if (ac == null)
                throw new ArgumentNullException(nameof(ac));
            switch (ac.Length)
            {
                case 2: AcPhase.Value = ac[1]; goto case 1;
                case 1: AcMagnitude.Value = ac[0]; break;
                case 0: AcMagnitude.Value = 0.0; break;
                default:
                    throw new BadParameterException("ac");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FrequencyParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="magnitude">Magnitude</param>
        /// <param name="phase">Phase</param>
        public FrequencyParameters(double magnitude, double phase)
        {
            AcMagnitude.Value = magnitude;
            AcPhase.Value = phase;
        }
    }
}
