using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    public partial class Sine
    {
        /// <summary>
        /// An instance of a sine waveform.
        /// </summary>
        /// <seealso cref="IWaveform" />
        protected class Instance : IWaveform
        {
            /// <summary>
            /// Private variables
            /// </summary>
            private double _vo, _va, _freq, _td, _theta, _phase;
            private ITimeSimulationState _state;

            /// <summary>
            /// Gets the value that is currently being probed.
            /// </summary>
            /// <value>
            /// The value at the probed timepoint.
            /// </value>
            public double Value { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Instance"/> class.
            /// </summary>
            /// <param name="state">The simulation state.</param>
            /// <param name="vo">The offset.</param>
            /// <param name="va">The amplitude.</param>
            /// <param name="frequency">The frequency.</param>
            /// <param name="td">The delay.</param>
            /// <param name="theta">The theta.</param>
            /// <param name="phase">The phase.</param>
            public Instance(ITimeSimulationState state, double vo, double va, double frequency, double td, double theta, double phase)
            {
                _state = state;
                _vo = vo;
                _va = va;
                _freq = frequency * 2.0 * Math.PI;
                _td = td;
                _theta = theta;
                _phase = phase * Math.PI / 180;

                Value = _vo;
                if (_freq < 0)
                    throw new BadParameterException(nameof(frequency), frequency,
                        Properties.Resources.Waveforms_Sine_FrequencyTooSmall);
            }

            /// <summary>
            /// Probes a new timepoint.
            /// </summary>
            public void Probe()
            {
                var time = _state?.Method?.Time ?? 0.0;
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

            /// <summary>
            /// Accepts the last probed timepoint.
            /// </summary>
            public void Accept()
            {
            }
        }
    }
}
