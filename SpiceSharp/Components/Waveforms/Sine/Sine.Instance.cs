using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    public partial class Sine
    {
        /// <summary>
        /// An instance of a <see cref="Sine"/> waveform.
        /// </summary>
        /// <seealso cref="IWaveform" />
        protected class Instance : IWaveform
        {
            private readonly double _vo, _va, _freq, _td, _theta, _phase;
            private readonly IIntegrationMethod _method;

            /// <inheritdoc/>
            public double Value { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Instance"/> class.
            /// </summary>
            /// <param name="method">The integration method.</param>
            /// <param name="vo">The offset.</param>
            /// <param name="va">The amplitude.</param>
            /// <param name="frequency">The frequency.</param>
            /// <param name="td">The delay.</param>
            /// <param name="theta">The theta.</param>
            /// <param name="phase">The phase.</param>
            public Instance(IIntegrationMethod method, double vo, double va, double frequency, double td, double theta, double phase)
            {
                _method = method;
                _vo = vo;
                _va = va;
                _freq = frequency.GreaterThanOrEquals(nameof(frequency), 0) * 2.0 * Math.PI;
                _td = td;
                _theta = theta;
                _phase = phase * Math.PI / 180;

                // Initialize the value
                Value = _vo;
            }

            /// <inheritdoc/>
            public void Probe()
            {
                double time = _method?.Time ?? 0.0;
                time -= _td;

                // Calculate sine wave result (no offset)
                double result;
                if (time <= 0.0)
                    result = 0.0;
                else
                    result = _va * Math.Sin(_freq * time + _phase);

                // Modify with theta
                if (!_theta.Equals(0.0))
                    result *= Math.Exp(-time * _theta);

                // Return result (with offset)
                Value = _vo + result;
            }

            /// <inheritdoc/>
            public void Accept()
            {
            }
        }
    }
}
