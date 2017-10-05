using System;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Noise variables
    /// </summary>
    public class NoiseGenerators
    {
        /// <summary>
        /// Container for noise variables
        /// </summary>
        public class NoiseVariables
        {
            public double LnLastDens;
            public double OutNoiz;
            public double InNoiz;
        }

        /// <summary>
        /// Get all generators
        /// </summary>
        public NoiseGenerator[] Generators { get; }

        /// <summary>
        /// Get all noise variables
        /// </summary>
        public NoiseVariables[] Variables { get; }

        /// <summary>
        /// Constructor
        /// One generator is always added at the end for the total noise density
        /// </summary>
        /// <param name="generators">Names of the generators</param>
        public NoiseGenerators(params NoiseGenerator[] generators)
        {
            Generators = new NoiseGenerator[generators.Length];
            if (Generators.Length > 0)
            {
                // The last variable will be the total input/output noise
                Variables = new NoiseVariables[generators.Length + 1];
                for (int i = 0; i < generators.Length; i++)
                {
                    Generators[i] = generators[i];
                    Variables[i] = new NoiseVariables();
                }
                Variables[generators.Length] = new NoiseVariables();
            }
            else
            {
                // We only have 1, so no need to store more
                Generators[0] = generators[0];
                Variables = new NoiseVariables[1];
                Variables[0] = new NoiseVariables();
            }
        }
    }
}
