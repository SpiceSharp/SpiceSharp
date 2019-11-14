using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Noise behavior for <see cref="Resistor"/>
    /// </summary>
    public class NoiseBehavior : FrequencyBehavior, INoiseBehavior
    {
        /// <summary>
        /// Gets resistor noise sources
        /// </summary>
        public ComponentNoise ResistorNoise { get; } = new ComponentNoise(new NoiseThermal("thermal", 0, 1));

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="context"></param>
        public NoiseBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            ResistorNoise.Bind(context, context.Nodes[0], context.Nodes[1]);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        void INoiseBehavior.Noise()
        {
            ResistorNoise.Generators[0].SetCoefficients(Conductance);
            ResistorNoise.Evaluate();
        }
    }
}
