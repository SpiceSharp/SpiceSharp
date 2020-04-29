using System.Collections.Generic;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Piecewise linear waveform.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IWaveformDescription" />
    public partial class Pwl : ParameterSet,
        IWaveformDescription
    {
        /// <summary>
        /// Gets or sets the time point values.
        /// </summary>
        /// <value>
        /// The time point values.
        /// </value>
        [ParameterName("times"), ParameterInfo("The time points.")]
        public IEnumerable<double> Times { get; set; }

        /// <summary>
        /// Gets or sets the value point values.
        /// </summary>
        /// <value>
        /// The value point values.
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

        /// <inheritdoc/>
        public IWaveform Create(IIntegrationMethod method)
        {
            return new Instance(Times, Values, method);
        }
    }
}
