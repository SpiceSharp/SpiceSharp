﻿using SpiceSharp.Attributes;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class implements a pulse waveform.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IWaveformDescription" />
    public partial class Pulse : ParameterSet,
        IWaveformDescription
    {
        /// <summary>
        /// Gets or sets the initial/low value.
        /// </summary>
        /// <value>
        /// The initial/low value.
        /// </value>
        [ParameterName("v1"), ParameterInfo("The initial value")]
        public double InitialValue { get; set; }

        /// <summary>
        /// Gets or sets the pulsed/high value.
        /// </summary>
        /// <value>
        /// The pulsed/high value.
        /// </value>
        [ParameterName("v2"), ParameterInfo("The peak value")]
        public double PulsedValue { get; set; }

        /// <summary>
        /// Gets the delay of the waveform in seconds.
        /// </summary>
        /// <value>
        /// The delay of the waveform.
        /// </value>
        [ParameterName("td"), ParameterInfo("The initial delay time in seconds", Units = "s")]
        public double Delay { get; set; }

        /// <summary>
        /// Gets or sets the rise time in seconds.
        /// </summary>
        /// <value>
        /// The rise time.
        /// </value>
        [ParameterName("tr"), ParameterInfo("The rise time", Units = "s")]
        public double RiseTime { get; set; }

        /// <summary>
        /// Gets or sets the fall time in seconds.
        /// </summary>
        /// <value>
        /// The fall time.
        /// </value>
        [ParameterName("tf"), ParameterInfo("The fall time", Units = "s")]
        public double FallTime { get; set; }

        /// <summary>
        /// Gets or sets the width of the pulse in seconds.
        /// </summary>
        /// <value>
        /// The pulse width.
        /// </value>
        [ParameterName("pw"), ParameterInfo("The pulse width", Units = "s")]
        public double PulseWidth { get; set; } = double.PositiveInfinity;

        /// <summary>
        /// Gets or sets the period in seconds.
        /// </summary>
        /// <value>
        /// The period.
        /// </value>
        [ParameterName("per"), ParameterInfo("The period", Units = "s")]
        public double Period { get; set; } = double.PositiveInfinity;

        /// <summary>
        /// Sets all the pulse parameters.
        /// </summary>
        /// <param name="pulse">The pulse parameters.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="pulse"/> does not have 1-7 parameters.</exception>
        [ParameterName("pulse"), ParameterInfo("A vector of all pulse waveform parameters")]
        public void SetPulse(double[] pulse)
        {
            pulse.ThrowIfNotLength(nameof(pulse), 1, 7);
            switch (pulse.Length)
            {
                case 7:
                    Period = pulse[6];
                    goto case 6;
                case 6:
                    PulseWidth = pulse[5];
                    goto case 5;
                case 5:
                    FallTime = pulse[4];
                    goto case 4;
                case 4:
                    RiseTime = pulse[3];
                    goto case 3;
                case 3:
                    Delay = pulse[2];
                    goto case 2;
                case 2:
                    PulsedValue = pulse[1];
                    goto case 1;
                case 1:
                    InitialValue = pulse[0];
                    break;
            }
        }

        /// <inheritdoc/>
        public IWaveform Create(IIntegrationMethod method)
        {
            return new Instance(method,
                InitialValue, PulsedValue, Delay, RiseTime, FallTime, PulseWidth, Period);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pulse"/> class.
        /// </summary>
        public Pulse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pulse"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="pulsedValue">The peak value.</param>
        /// <param name="delay">The initial delay time in seconds.</param>
        /// <param name="riseTime">The rise time in seconds.</param>
        /// <param name="fallTime">The fall time in seconds.</param>
        /// <param name="pulseWidth">The pulse width in seconds.</param>
        /// <param name="period">The period in seconds.</param>
        public Pulse(double initialValue, double pulsedValue, double delay, double riseTime, double fallTime, double pulseWidth, double period)
        {
            InitialValue = initialValue;
            PulsedValue = pulsedValue;
            Delay = delay;
            RiseTime = riseTime;
            FallTime = fallTime;
            PulseWidth = pulseWidth;
            Period = period;
        }
    }
}
