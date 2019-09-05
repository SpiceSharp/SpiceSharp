﻿using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components.MosfetBehaviors.Common;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Noise behavior for a <see cref="Mosfet3"/>
    /// </summary>
    public class NoiseBehavior : FrequencyBehavior, INoiseBehavior
    {
        /// <summary>
        /// Gets the noise parameters.
        /// </summary>
        protected ModelNoiseParameters NoiseParameters { get; private set; }

        /// <summary>
        /// Index of the thermal noise generated by the drain resistance.
        /// </summary>
        protected const int RdNoise = 0;

        /// <summary>
        /// Index of the thermal noise generated by the source resistance.
        /// </summary>
        protected const int RsNoise = 1;

        /// <summary>
        /// Index of the shot-noise generated by the drain current.
        /// </summary>
        protected const int IdNoise = 2;

        /// <summary>
        /// Index of the flicker-noise.
        /// </summary>
        protected const int FlickerNoise = 3;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise MosfetNoise { get; } = new ComponentNoise(
            new NoiseThermal("rd", 0, 4),
            new NoiseThermal("rs", 2, 5),
            new NoiseThermal("id", 4, 5),
            new NoiseGain("1overf", 4, 5)
        );

        
        private NoiseSimulationState _state;

        /// <summary>
        /// Creates a new instance of the <see cref="NoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="context"></param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);
            var c = (ComponentBindingContext)context;
            NoiseParameters = c.ModelBehaviors.Parameters.Get<ModelNoiseParameters>();
            _state = context.States.Get<NoiseSimulationState>();
            MosfetNoise.Bind(context, DrainNode,
                GateNode,
                SourceNode,
                BulkNode,
                DrainNodePrime,
                SourceNodePrime);
        }

        /// <summary>
        /// Calculate the noise contributions.
        /// </summary>
        void INoiseBehavior.Noise()
        {
            var noise = _state.ThrowIfNotBound(this);

            double coxSquared;
            if (ModelParameters.OxideCapFactor > 0.0)
                coxSquared = ModelParameters.OxideCapFactor;
            else
                coxSquared = 3.9 * 8.854214871e-12 / 1e-7;
            coxSquared *= coxSquared;

            // Set noise parameters
            MosfetNoise.Generators[RdNoise].SetCoefficients(DrainConductance);
            MosfetNoise.Generators[RsNoise].SetCoefficients(SourceConductance);
            MosfetNoise.Generators[IdNoise].SetCoefficients(2.0 / 3.0 * Math.Abs(Transconductance));
            MosfetNoise.Generators[FlickerNoise].SetCoefficients(
                NoiseParameters.FlickerNoiseCoefficient *
                Math.Exp(NoiseParameters.FlickerNoiseExponent * Math.Log(Math.Max(Math.Abs(DrainCurrent), 1e-38))) /
                (BaseParameters.Width * (BaseParameters.Length - 2 * ModelParameters.LateralDiffusion) *
                 coxSquared) / noise.Frequency);

            // Evaluate noise sources
            MosfetNoise.Evaluate();
        }
    }
}
