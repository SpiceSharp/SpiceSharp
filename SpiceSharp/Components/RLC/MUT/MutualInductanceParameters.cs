﻿using SpiceSharp.Attributes;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Base parameters for a <see cref="MutualInductance"/>
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    public class MutualInductanceParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the coupling coefficient.
        /// </summary>
        /// <value>
        /// The coupling coefficient.
        /// </value>
        /// <remarks>
        /// A value of 1.0 indicates perfect coupling, and 0.0 will result in no coupling between
        /// the inductors. The mutual inductance can be computed using M = k*sqrt(L1*L2).
        /// </remarks>
        [ParameterName("k"), ParameterName("coefficient"), ParameterInfo("Coupling coefficient", IsPrincipal = true)]
        public GivenParameter<double> Coupling { get; set; }
    }
}
