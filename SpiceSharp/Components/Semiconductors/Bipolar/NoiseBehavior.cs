using System;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Noise behavior for <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class NoiseBehavior : Behaviors.NoiseBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        LoadBehavior load;
        ModelNoiseParameters mnp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Noise sources by their index
        /// </summary>
        const int RcNoise = 0;
        const int RbNoise = 1;
        const int ReNoise = 2;
        const int IcNoise = 3;
        const int IbNoise = 4;
        const int FlickerNoise = 5;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise BipolarJunctionTransistorNoise { get; } = new ComponentNoise(
            new NoiseThermal("rc", 0, 4),
            new NoiseThermal("rb", 1, 5),
            new NoiseThermal("re", 2, 6),
            new NoiseShot("ic", 4, 6),
            new NoiseShot("ib", 5, 6),
            new NoiseGain("1overf", 5, 6)
            );

        /// <summary>
        /// Nodes
        /// </summary>
        int colNode, baseNode, emitNode, substNode, colPrimeNode, basePrimeNode, emitPrimeNode;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            mnp = provider.GetParameterSet<ModelNoiseParameters>(1);

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>(0);
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new Diagnostics.CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            colNode = pins[0];
            baseNode = pins[1];
            emitNode = pins[2];
            substNode = pins[3];
        }

        /// <summary>
        /// Connect noise sources
        /// </summary>
        public override void ConnectNoise()
        {
            // Get extra nodes
            colPrimeNode = load.ColPrimeNode;
            basePrimeNode = load.BasePrimeNode;
            emitPrimeNode = load.EmitPrimeNode;

            // Connect noise
            BipolarJunctionTransistorNoise.Setup(colNode, baseNode, emitNode, substNode,
                colPrimeNode, basePrimeNode, emitPrimeNode);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        public override void Noise(Noise simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var noise = simulation.NoiseState;

            // Set noise parameters
            BipolarJunctionTransistorNoise.Generators[RcNoise].SetCoefficients(modeltemp.CollectorConduct * bp.Area);
            BipolarJunctionTransistorNoise.Generators[RbNoise].SetCoefficients(load.Gx);
            BipolarJunctionTransistorNoise.Generators[ReNoise].SetCoefficients(modeltemp.EmitterConduct * bp.Area);
            BipolarJunctionTransistorNoise.Generators[IcNoise].SetCoefficients(load.Cc);
            BipolarJunctionTransistorNoise.Generators[IbNoise].SetCoefficients(load.Cb);
            BipolarJunctionTransistorNoise.Generators[FlickerNoise].SetCoefficients(mnp.FnCoefficient * Math.Exp(mnp.FlickerNoiseExponent * Math.Log(Math.Max(Math.Abs(load.Cb), 1e-38))) / noise.Freq);

            // Evaluate all noise sources
            BipolarJunctionTransistorNoise.Evaluate(simulation);
        }
    }
}
