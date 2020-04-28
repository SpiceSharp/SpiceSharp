using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Resistors
{
    /// <summary>
    /// Noise behavior for a <see cref="Resistor"/>.
    /// </summary>
    /// <seealso cref="Frequency"/>
    /// <seealso cref="INoiseBehavior"/>
    public class Noise : Frequency, INoiseBehavior
    {
        /// <summary>
        /// Gets resistor noise sources
        /// </summary>
        /// <value>
        /// The resistor noise generators.
        /// </value>
        public ComponentNoise ResistorNoise { get; } = new ComponentNoise(new NoiseThermal("thermal", 0, 1));

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="context">The binding context.</param>
        public Noise(string name, ComponentBindingContext context) : base(name, context) 
        {
            var state = context.GetState<IComplexSimulationState>();
            ResistorNoise.Bind(context, state.GetSharedVariable(context.Nodes[0]), state.GetSharedVariable(context.Nodes[1]));
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
