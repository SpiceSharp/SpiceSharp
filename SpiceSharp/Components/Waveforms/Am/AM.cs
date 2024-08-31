using SpiceSharp.Attributes;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class implements an amplitude-modulated waveform.
    /// </summary>
    [GeneratedParameters]
    public partial class AM : ParameterSet<IWaveformDescription>,
        IWaveformDescription
    {
        /// <summary>
        /// Gets or sets the amplitude value.
        /// </summary>
        [ParameterName("va"), ParameterInfo("Amplitude")]
        public double Amplitude { get; set; }

        /// <summary>
        /// Gets or sets the offset value.
        /// </summary>
        [ParameterName("vo"), ParameterInfo("DC offset")]
        public double Offset { get; set; }

        /// <summary>
        /// Gets or sets the carrier frequency.
        /// </summary>
        [ParameterName("mf"), ParameterInfo("Modulation frequency", Units = "Hz")]
        [GreaterThanOrEquals(0)]
        private GivenParameter<double> _modulationFrequency;

        /// <summary>
        /// Gets or sets the signal frequency.
        /// </summary>
        [ParameterName("fc"), ParameterInfo("Carrier frequency", Units = "Hz")]
        [GreaterThanOrEquals(0)]
        private GivenParameter<double> _carrierFrequency;

        /// <summary>
        /// Gets or sets the signal delay.
        /// </summary>
        [ParameterName("td"), ParameterInfo("Signal delay", Units = "s")]
        public double SignalDelay { get; set; }

        /// <summary>
        /// Gets or sets the carrier phase.
        /// </summary>
        [ParameterName("phasec"), ParameterInfo("Carrier phsae", Units = "\u00b0")]
        public double CarrierPhase { get; set; }

        /// <summary>
        /// Gets or sets the signal phase.
        /// </summary>
        [ParameterName("phases"), ParameterInfo("Signal phase", Units = "\u00b0")]
        public double SignalPhase { get; set; }

        /// <summary>
        /// Sets all SFFM parameters.
        /// </summary>
        /// <param name="sffm">The SFFM parameters.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="sffm"/> does not have 1 - 5 parameters.</exception>
        [ParameterName("sffm"), ParameterInfo("Specify the waveform as a vector")]
        public void SetSffm(double[] sffm)
        {
            sffm.ThrowIfNotLength(nameof(sffm), 1, 6);
            switch (sffm.Length)
            {
                case 7:
                    SignalPhase = sffm[6];
                    goto case 6;
                case 6:
                    CarrierPhase = sffm[5];
                    goto case 5;
                case 5:
                    SignalDelay = sffm[4];
                    goto case 4;
                case 4:
                    CarrierFrequency = sffm[3];
                    goto case 3;
                case 3:
                    ModulationFrequency = sffm[2];
                    goto case 2;
                case 2:
                    Offset = sffm[1];
                    goto case 1;
                case 1:
                    Amplitude = sffm[0];
                    break;
            }
        }

        /// <inheritdoc/>
        public IWaveform Create(IBindingContext context)
        {
            IIntegrationMethod method = null;
            TimeParameters tp = null;
            context?.TryGetState(out method);
            context?.TryGetSimulationParameterSet(out tp);
            return new Instance(method,
                Amplitude,
                Offset,
                ModulationFrequency,
                CarrierFrequency.Given ? CarrierFrequency.Value : 1.0 / (tp?.StopTime ?? 1.0),
                SignalDelay,
                CarrierPhase,
                SignalPhase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SFFM"/> class.
        /// </summary>
        public AM()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SFFM"/> class.
        /// </summary>
        /// <param name="amplitude">The amplitude value.</param>
        /// <param name="offset">The offset value.</param>
        /// <param name="modulationFrequency">The modulation frequency.</param>
        /// <param name="carrierFrequency">The carrier frequency.</param>
        /// <param name="signalDelay">The signal delay.</param>
        /// <param name="carrierPhase">The carrier phase.</param>
        /// <param name="signalPhase">The signal phase.</param>
        public AM(double amplitude, double offset, double modulationFrequency, double carrierFrequency, double signalDelay, double carrierPhase, double signalPhase)
        {
            Amplitude = amplitude;
            Offset = offset;
            ModulationFrequency = modulationFrequency;
            CarrierFrequency = carrierFrequency;
            SignalDelay = signalDelay;
            CarrierPhase = carrierPhase;
            SignalPhase = signalPhase;
        }

        /// <summary>
        /// Returns a string that represents the current single-frequency FM waveform.
        /// </summary>
        /// <returns>
        /// A string that represents the current SFFM.
        /// </returns>
        public override string ToString()
        {
            return "am({0} {1} {2} {3} {4} {5} {6})".FormatString(
                Amplitude,
                Offset,
                ModulationFrequency.Value,
                CarrierFrequency.Value,
                SignalDelay,
                CarrierPhase,
                SignalPhase);
        }
    }
}
