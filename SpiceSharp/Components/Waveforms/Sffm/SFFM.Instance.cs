using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Text;

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
            private readonly double _vo, _va, _fcar, _mdi, _fsig, _pcar, _psig;
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
                _fcar = carrierFrequency * Math.PI / 180.0;
                _mdi = modulationIndex;
                _fsig = signalFrequency * Math.PI / 180.0;
                _pcar = carrierPhase * Math.PI / 180.0;
                _psig = signalPhase * Math.PI / 180.0;
            }

            /// <summary>
            /// Calculate the pulse value at the designated timepoint.
            /// </summary>
            /// <param name="time">The time value.</param>
            private void At(double time)
            {
                // Compute the waveform value
                Value = _vo + _va *
                    Math.Sin(
                        (2.0 * Math.PI * _fcar * time + _pcar) +
                        _mdi * Math.Sin(2.0 * Math.PI * _fsig * time + _psig));
            }

            /// <inheritdoc/>
            public void Probe()
            {
                var time = _method?.Time ?? 0.0;
                At(time);
            }

            /// <inheritdoc/>
            public void Accept()
            {
            }
        }
    }
}
