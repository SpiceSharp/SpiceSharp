using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A description for component noise.
    /// </summary>
    public class ComponentNoise
    {
        private INoiseSimulationState _nstate;

        /// <summary>
        /// Gets the total output-referred noise density
        /// </summary>
        /// <value>
        /// The noise density.
        /// </value>
        public double Noise { get; private set; }

        /// <summary>
        /// Gets the natural logarithm of the total output-referred noise density.
        /// </summary>
        /// <value>
        /// The natural logarithm of the total output-referred noise density.
        /// </value>
        public double LogNoise { get; private set; }

        /// <summary>
        /// Gets the total integrated output-referred noise.
        /// </summary>
        /// <value>
        /// The total integrated output-referred noise.
        /// </value>
        public double TotalOutNoise { get; private set; }

        /// <summary>
        /// Gets the total integrated input-referred noise.
        /// </summary>
        /// <value>
        /// The total integrated input-referred noise.
        /// </value>
        public double TotalInNoise { get; private set; }

        /// <summary>
        /// Gets a collection of all the noise generators.
        /// </summary>
        /// <value>
        /// A collection of all the noise generators.
        /// </value>
        public NoiseGeneratorCollection Generators { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentNoise"/> class.
        /// </summary>
        /// <param name="generators">Names of the generators.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="generators"/> or any of the noise generators is <c>null</c>.</exception>
        public ComponentNoise(params NoiseGenerator[] generators)
        {
            generators.ThrowIfNull(nameof(generators));
            Generators = new NoiseGeneratorCollection(generators);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentNoise"/> class.
        /// </summary>
        /// <param name="generators">The noise generators.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="generators"/> or any of the noise generators is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public void Bind(IComponentBindingContext context, params IVariable<Complex>[] nodes)
        {
            context.ThrowIfNull(nameof(context));
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
