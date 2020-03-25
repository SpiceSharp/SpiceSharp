using System;
using System.Collections.Generic;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Noise variables
    /// </summary>
    public class ComponentNoise
    {
        /// <summary>
        /// Gets the total output-referred noise density
        /// </summary>
        public double Noise { get; private set; }

        /// <summary>
        /// Gets the log of the total output-referred noise density
        /// </summary>
        public double LogNoise { get; private set; }

        /// <summary>
        /// Gets the total integrated output-referred noise
        /// </summary>
        public double TotalOutNoise { get; private set; }

        /// <summary>
        /// Gets the total integrated input-referred noise
        /// </summary>
        public double TotalInNoise { get; private set; }

        /// <summary>
        /// Gets all generators
        /// </summary>
        public NoiseGeneratorCollection Generators { get; }

        // States
        private IComplexSimulationState _cstate;
        private INoiseSimulationState _nstate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentNoise"/> class.
        /// </summary>
        /// <param name="generators">Names of the generators</param>
        public ComponentNoise(params NoiseGenerator[] generators)
        {
            generators.ThrowIfNull(nameof(generators));
            Generators = new NoiseGeneratorCollection(generators);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentNoise"/> class.
        /// </summary>
        /// <param name="generators"></param>
        public ComponentNoise(IEnumerable<NoiseGenerator> generators)
        {
            generators.ThrowIfNull(nameof(generators));
            Generators = new NoiseGeneratorCollection(generators);
        }

        /// <summary>
        /// Binds the component noise generators to the simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        /// <param name="nodes">The nodes.</param>
        public void Bind(IComponentBindingContext context, params IVariable[] nodes)
        {
            context.ThrowIfNull(nameof(context));
            _cstate = context.GetState<IComplexSimulationState>();
            _nstate = context.GetState<INoiseSimulationState>();
            
            foreach (var generator in Generators)
                generator.Bind(context, nodes);
        }

        /// <summary>
        /// Evaluate all noise source contributions.
        /// </summary>
        public void Evaluate()
        {
            // Calculate the output noise density
            Noise = 0.0;
            TotalInNoise = 0.0;
            TotalOutNoise = 0.0;
            foreach (var generator in Generators)
            {
                generator.Evaluate();
                Noise += generator.Noise;
                TotalInNoise += generator.TotalInputNoise;
                TotalOutNoise += generator.TotalOutputNoise;
            }

            // Log of the output noise density
            LogNoise = Math.Log(Math.Max(Noise, 1e-38));

            // Output noise density
            _nstate.OutputNoiseDensity += Noise;

            // Integrated input referred noise
            _nstate.InputNoise += TotalInNoise;

            // Integrated output referred noise
            _nstate.OutputNoise += TotalOutNoise;
        }
    }
}
