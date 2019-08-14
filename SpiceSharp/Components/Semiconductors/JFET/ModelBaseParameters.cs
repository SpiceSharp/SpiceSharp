using SpiceSharp.Attributes;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="JFETModel" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the measurement temperature in degrees celsius.
        /// </summary>
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature.Value = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the measurement temperature in Kelvin.
        /// </summary>
        [ParameterName("tnom")]
        public GivenParameter<double> NominalTemperature { get; } = new GivenParameter<double>(300.15);

        /// <summary>
        /// Gets the threshold voltage.
        /// </summary>
        [ParameterName("vt0"), ParameterName("vto"), ParameterInfo("Threshold voltage")]
        public GivenParameter<double> Threshold { get; } = new GivenParameter<double>(-2);

        /// <summary>
        /// Gets the transconductance.
        /// </summary>
        [ParameterName("beta"), ParameterInfo("Transconductance parameter")]
        public GivenParameter<double> Beta { get; } = new GivenParameter<double>(1e-4);

        /// <summary>
        /// Gets the channel length modulation parameter.
        /// </summary>
        [ParameterName("lambda"), ParameterInfo("Channel length modulation parameter")]
        public GivenParameter<double> LModulation { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the drain resistance.
        /// </summary>
        [ParameterName("rd"), ParameterInfo("Drain ohmic resistance")]
        public GivenParameter<double> DrainResistance { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the source resistance.
        /// </summary>
        [ParameterName("rs"), ParameterInfo("Source ohmic resistance")]
        public GivenParameter<double> SourceResistance { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the gate-source junction capacitance.
        /// </summary>
        [ParameterName("cgs"), ParameterInfo("G-S junction capacitance")]
        public GivenParameter<double> CapGs { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the gate-drain junction capacitance.
        /// </summary>
        [ParameterName("cgd"), ParameterInfo("G-D junction capacitance")]
        public GivenParameter<double> CapGd { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the gate junction potential.
        /// </summary>
        [ParameterName("pb"), ParameterInfo("Gate junction potential")]
        public GivenParameter<double> GatePotential { get; } = new GivenParameter<double>(1);

        /// <summary>
        /// Gets the gate saturation current.
        /// </summary>
        [ParameterName("is"), ParameterInfo("Gate junction saturation current")]
        public GivenParameter<double> GateSaturationCurrent { get; } = new GivenParameter<double>(1e-14);

        /// <summary>
        /// Gets the forward bias junction fitting parameter.
        /// </summary>
        [ParameterName("fc"), ParameterInfo("Forward bias junction fitting parameter")]
        public GivenParameter<double> DepletionCapCoefficient { get; } = new GivenParameter<double>(0.5);

        /// <summary>
        /// Gets the doping tail parameter.
        /// </summary>
        [ParameterName("b"), ParameterInfo("Doping tail parameter")]
        public GivenParameter<double> B { get; } = new GivenParameter<double>(1);

        /// <summary>
        /// Gets or sets the type of the jfet.
        /// </summary>
        public double JFETType { get; protected set; } = 1.0;

        /// <summary>
        /// Gets the drain conductance.
        /// </summary>
        public double DrainConductance { get; private set; }

        /// <summary>
        /// Gets the source conductance.
        /// </summary>
        public double SourceConductance { get; private set; }

        /// <summary>
        /// Sets the model to be n-type.
        /// </summary>
        /// <param name="flag">if set to <c>true</c> n-type is set.</param>
        [ParameterName("njf"), ParameterInfo("N type JFET model")]
        public void SetNjf(bool flag)
        {
            if (flag)
                JFETType = 1.0;
        }

        /// <summary>
        /// Sets the model to be p-type.
        /// </summary>
        /// <param name="flag">if set to <c>true</c> p-type is set.</param>
        [ParameterName("pjf"), ParameterInfo("P type JFET model")]
        public void SetPjf(bool flag)
        {
            if (flag)
                JFETType = -1.0;
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        [ParameterName("type"), ParameterInfo("N-type or P-type JFET model")]
        public string TypeName
        {
            get
            {
                if (JFETType > 0.0)
                    return "njf";
                return "pjf";
            }
        }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public override void CalculateDefaults()
        {
            if (DrainResistance > 0)
                DrainConductance = 1 / DrainResistance;
            else
                DrainConductance = 0;

            if (SourceResistance > 0)
                SourceConductance = 1 / SourceResistance;
            else
                SourceConductance = 0;
        }
    }
}
