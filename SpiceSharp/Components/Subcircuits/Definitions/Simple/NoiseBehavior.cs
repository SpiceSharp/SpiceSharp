using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Linq;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// An <see cref="INoiseBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="INoiseBehavior" />
    public class NoiseBehavior : SubcircuitBehavior<INoiseBehavior>, 
        INoiseBehavior
    {
        /// <inheritdoc/>
        public double OutputNoiseDensity => Behaviors.Sum(nb => nb.OutputNoiseDensity);

        /// <inheritdoc/>
        public double TotalOutputNoise => Behaviors.Sum(nb => nb.TotalOutputNoise);

        /// <inheritdoc/>
        public double TotalInputNoise => Behaviors.Sum(nb => nb.TotalInputNoise);

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public NoiseBehavior(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
        }

        /// <inheritdoc/>
        void INoiseSource.Initialize()
        {
            foreach (var behavior in Behaviors)
                behavior.Initialize();
        }

        /// <inheritdoc/>
        void INoiseBehavior.Compute()
        {
            foreach (var behavior in Behaviors)
                behavior.Compute();
        }
    }
}
