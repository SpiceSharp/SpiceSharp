﻿using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Components
{
    public partial class AM
    {
        /// <summary>
        /// The instance for a SFFM waveform.
        /// </summary>
        /// <seealso cref="IWaveform" />
        protected class Instance : IWaveform
        {
            private readonly double _va, _vo, _td, _mf, _fc, _phases;
            private readonly IIntegrationMethod _method;

            /// <inheritdoc/>
            public double Value { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Instance"/> class.
            /// </summary>
            /// <param name="method">The integration method.</param>
            /// <param name="amplitude">The amplitude.</param>
            /// <param name="offset">The offset.</param>
            /// <param name="modulationFrequency">The modulation index.</param>
            /// <param name="carrierFrequency">The carrier frequency.</param>
            /// <param name="signalDelay">The signal delay.</param>
            /// <param name="signalPhase">The signal phase.</param>
            public Instance(IIntegrationMethod method,
                double amplitude,
                double offset,
                double modulationFrequency,
                double carrierFrequency,
                double signalDelay,
                double signalPhase)
            {
                _method = method;
                _va = amplitude;
                _vo = offset;
                _mf = modulationFrequency * Math.PI / 180.0;
                _fc = carrierFrequency * Math.PI / 180.0;
                _td = signalDelay;
                _phases = signalPhase * Math.PI / 180.0;
            }

            /// <summary>
            /// Calculate the pulse value at the designated timepoint.
            /// </summary>
            /// <param name="time">The time value.</param>
            private void At(double time)
            {
                time -= _td;
                if (time <= 0)
                    Value = 0;
                else
                {
                    // Compute waveform value
                    Value = _va * (_vo + Math.Sin(2.0 * Math.PI * _mf * time + _phases)) *
                        Math.Sin(2.0 * Math.PI * _fc * time + _phases);
                }
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
