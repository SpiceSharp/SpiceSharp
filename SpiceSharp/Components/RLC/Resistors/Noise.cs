using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Resistors
{
    /// <summary>
    /// Noise behavior for a <see cref="Resistor"/>.
    /// </summary>
    /// <seealso cref="Frequency"/>
    /// <seealso cref="INoiseBehavior"/>
    [BehaviorFor(typeof(Resistor)), AddBehaviorIfNo(typeof(INoiseBehavior))]
    [GeneratedParameters]
    public partial class Noise : Frequency, INoiseBehavior
    {
        private readonly NoiseThermal _thermal;

        /// <inheritdoc/>
        public double OutputNoiseDensity => _thermal.OutputNoiseDensity;

        /// <inheritdoc/>
        public double TotalOutputNoise => _thermal.TotalOutputNoise;

        /// <inheritdoc/>
        public double TotalInputNoise => _thermal.TotalInputNoise;

        /// <summary>
        /// Gets the thermal noise source of the resistor.
        /// </summary>
        /// <value>
        /// The thermal noise source.
        /// </value>
        [ParameterName("thermal"), ParameterInfo("The thermal noise source")]
        public INoiseSource Thermal => _thermal;

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Noise(IComponentBindingContext context) : base(context)
        {
            var state = context.GetState<IComplexSimulationState>();
            _thermal = new NoiseThermal("r",
                state.GetSharedVariable(context.Nodes[0]),
                state.GetSharedVariable(context.Nodes[1]));
        }


        /// <inheritdoc/>
        void INoiseSource.Initialize()
        {
            _thermal.Initialize();
        }

        /// <inheritdoc />
        void INoiseBehavior.Load() { }

        /// <inheritdoc/>
        void INoiseBehavior.Compute()
        {
            _thermal.Compute(Conductance, Parameters.Temperature);
        }
    }
}
