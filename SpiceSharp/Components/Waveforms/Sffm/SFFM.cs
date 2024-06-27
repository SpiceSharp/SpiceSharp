using SpiceSharp.Attributes;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class implements a single-frequency FM waveform.
    /// </summary>
    [GeneratedParameters]
    public partial class SFFM : ParameterSet<IWaveformDescription>,
        IWaveformDescription
    {
        /// <summary>
        /// Gets or sets the offset value.
        /// </summary>
        [ParameterName("vo"), ParameterInfo("DC offset")]
        public double Offset { get; set; }

        /// <summary>
        /// Gets or sets the amplitude value.
        /// </summary>
        [ParameterName("va"), ParameterInfo("Amplitude")]
        public double Amplitude { get; set; }

        /// <summary>
        /// Gets or sets the carrier frequency.
        /// </summary>
        [ParameterName("fc"), ParameterInfo("Carrier frequency", Units = "Hz")]
        [GreaterThanOrEquals(0)]
        private GivenParameter<double> _carrierFrequency;

        /// <summary>
        /// Gets or sets the modulation index.
        /// </summary>
        [ParameterName("mdi"), ParameterInfo("Modulation index")]
        public double ModulationIndex { get; set; }

        /// <summary>
        /// Gets or sets the signal frequency.
        /// </summary>
        [ParameterName("fs"), ParameterInfo("Signal frequency", Units = "Hz")]
        [GreaterThanOrEquals(0)]
        private GivenParameter<double> _signalFrequency;

        /// <summary>
        /// Gets or sets the carrier phase.
        /// </summary>
        [ParameterName("phasec"), ParameterInfo("Carrier phase", Units = "\u00b0")]
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
            sffm.ThrowIfNotLength(nameof(sffm), 1, 5);
            switch (sffm.Length)
            {
                case 7:
                    SignalPhase = sffm[6];
                    goto case 6;
                case 6:
                    CarrierPhase = sffm[5];
                    goto case 5;
                case 5:
                    SignalFrequency = sffm[4];
                    goto case 4;
                case 4:
                    ModulationIndex = sffm[3];
                    goto case 3;
                case 3:
                    CarrierFrequency = sffm[2];
                    goto case 2;
                case 2:
                    Amplitude = sffm[1];
                    goto case 1;
                case 1:
                    Offset = sffm[0];
                    break;
            }
        }

        /// <inheritdoc/>
        public IWaveform Create(IBindingContext context)
        {
            IIntegrationMethod method = null;
            TimeParameters tp = null;
            context?.TryGetState<IIntegrationMethod>(out method);
            context?.TryGetSimulationParameterSet<TimeParameters>(out tp);
            return new Instance(method,
                Offset,
                Amplitude,
                CarrierFrequency.Given ? CarrierFrequency.Value : 1.0 / (tp?.StopTime ?? 1.0),
                ModulationIndex,
                SignalFrequency.Given ? SignalFrequency.Value : 1.0 / (tp?.StopTime ?? 1.0),
                CarrierPhase,
                SignalPhase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SFFM"/> class.
        /// </summary>
        public SFFM()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SFFM"/> class.
        /// </summary>
        /// <param name="offset">The offset value.</param>
        /// <param name="amplitude">The amplitude value.</param>
        /// <param name="carrierFrequency">The carrier frequency.</param>
        /// <param name="modulationIndex">The modulation index.</param>
        /// <param name="signalFrequency">The signal frequency.</param>
        /// <param name="carrierPhase">The carrier phase.</param>
        /// <param name="signalPhase">The signal phase.</param>
        public SFFM(double offset, double amplitude, double carrierFrequency, double modulationIndex, double signalFrequency, double carrierPhase, double signalPhase)
        {
            Offset = offset;
            Amplitude = amplitude;
            CarrierFrequency = carrierFrequency;
            ModulationIndex = modulationIndex;
            SignalFrequency = signalFrequency;
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
            return "sffm({0} {1} {2} {3} {4} {5} {6})".FormatString(
                Offset,
                Amplitude, 
                CarrierFrequency.Value,
                ModulationIndex,
                SignalFrequency.Value,
                CarrierPhase,
                SignalPhase);
        }
    }
}
