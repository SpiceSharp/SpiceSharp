using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
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
        public NoiseBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);
            var c = (ComponentBindingContext)context;
            ResistorNoise.Bind(c, c.Nodes[0], c.Nodes[1]);
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
