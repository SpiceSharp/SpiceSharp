using System;
using System.Collections.Generic;
using SpiceSharp.Attributes;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Piecewise linear waveform.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IWaveformDescription" />
    public partial class Pwl : ParameterSet, IWaveformDescription
    {
        /// <summary>
        /// Gets or sets the times.
        /// </summary>
        /// <value>
        /// The times.
        /// </value>
        [ParameterName("times"), ParameterInfo("The time points.")]
        public IEnumerable<double> Times { get; set; }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        [ParameterName("values"), ParameterInfo("The values.")]
        public IEnumerable<double> Values { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pwl"/> class.
        /// </summary>
        /// <param name="times">Enumeration of time points.</param>
        /// <param name="values">Enumeration of values.</param>
        public Pwl(IEnumerable<double> times, IEnumerable<double> values)
        {
            Times = times;
            Values = values;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pwl"/> class.
        /// </summary>
        public Pwl()
        {
        }

        /// <summary>
        /// Creates a waveform instance for the specified simulation and entity.
        /// </summary>
        /// <param name="method">The integration method.</param>
        /// <returns>
        /// A waveform instance.
        /// </returns>
        public IWaveform Create(IIntegrationMethod method)
        {
            return new Instance(Times, Values, method);
        }
    }
}
