using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    public partial class SFFM
    {
        /// <summary>
        /// The instance for a SFFM waveform.
        /// </summary>
        /// <seealso cref="IWaveform" />
        protected class Instance : IWaveform
        {
            private readonly double _vo, _va, _fc, _mdi, _fs, _phasec, _phases;
            private readonly IIntegrationMethod _method;

            /// <inheritdoc/>
            public double Value { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Instance"/> class.
            /// </summary>
            /// <param name="method">The integration method.</param>
            /// <param name="offset">The offset.</param>
            /// <param name="amplitude">The amplitude.</param>
            /// <param name="carrierFrequency">The carrier frequency.</param>
            /// <param name="modulationIndex">The modulation index.</param>
            /// <param name="signalFrequency">The signal frequency.</param>
            /// <param name="carrierPhase">The carrier phase.</param>
            /// <param name="signalPhase">The signal phase.</param>
            public Instance(IIntegrationMethod method,
                double offset, double amplitude,
                double carrierFrequency, double modulationIndex, double signalFrequency,
                double carrierPhase, double signalPhase)
            {
                _method = method;
                _vo = offset;
                _va = amplitude;
                _fc = carrierFrequency * Math.PI * 2.0;
                _mdi = modulationIndex;
                _fs = signalFrequency * Math.PI * 2.0;
                _phasec = carrierPhase * Math.PI / 180.0;
                _phases = signalPhase * Math.PI / 180.0;

                // Initialize the value
                At(0.0);
            }

            /// <summary>
            /// Calculate the pulse value at the designated timepoint.
            /// </summary>
            /// <param name="time">The time value.</param>
            private void At(double time)
            {
                // Compute the waveform value
                Value = _vo + _va *
                    Math.Sin((_fc * time + _phasec) +
                    _mdi * Math.Sin(_fs * time + _phases));
            }

            /// <inheritdoc/>
            public void Probe()
            {
                double time = _method?.Time ?? 0.0;
                At(time);
            }

            /// <inheritdoc/>
            public void Accept()
            {
            }
        }
    }
}
