using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Components.Switches;
using SpiceSharp.Diagnostics;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled switch
    /// </summary>
    /// <seealso cref="Component" />
    /// <seealso cref="ICurrentControllingComponent" />
    /// <seealso cref="IParameterized{T}" />
    /// <seealso cref="Parameters"/>
    [Pin(0, "W+"), Pin(1, "W-"), Connected(0, 1)]
    public class CurrentSwitch : Component,
        ICurrentControllingComponent,
        IParameterized<Parameters>
    {
        /// <summary>
        /// Controlling source name
        /// </summary>
        [ParameterName("control"), ParameterInfo("Name of the controlling source")]
        public string ControllingSource { get; set; }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
        public const int CurrentSwitchPinCount = 2;

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public Parameters Parameters { get; } = new Parameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSwitch"/> class.
        /// </summary>
        /// <param name="name">The name of the current-controlled switch.</param>
        public CurrentSwitch(string name)
            : base(name, CurrentSwitchPinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSwitch"/> class.
        /// </summary>
        /// <param name="name">The name of the current-controlled switch.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="controllingSource">The controlling voltage source.</param>
        public CurrentSwitch(string name, string pos, string neg, string controllingSource)
            : base(name, CurrentSwitchPinCount)
        {
            Connect(pos, neg);
            ControllingSource = controllingSource;
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new CurrentControlledBindingContext(this, simulation, behaviors, LinkParameters);
            if (context.ModelBehaviors == null)
                throw new NoModelException(Name, typeof(CurrentSwitchModel));
            behaviors.Build(simulation, context)
                .AddIfNo<IAcceptBehavior>(context => new Accept(Name, context, new CurrentControlled(context)))
                .AddIfNo<IFrequencyBehavior>(context => new Frequency(Name, context, new CurrentControlled(context)))
                .AddIfNo<IBiasingBehavior>(context => new Biasing(Name, context, new CurrentControlled(context)));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
