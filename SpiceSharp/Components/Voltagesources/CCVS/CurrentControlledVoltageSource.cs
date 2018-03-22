using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CurrentControlledVoltagesourceBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled voltage source
    /// </summary>
    [Pin(0, "H+"), Pin(1, "H-"), VoltageDriver(0, 1)]
    public class CurrentControlledVoltageSource : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        [ParameterName("pos_node"), ParameterInfo("Positive node of the source")]
        public int PosNode { get; internal set; }
        [ParameterName("neg_node"), ParameterInfo("Negative node of the source")]
        public int NegNode { get; internal set; }
        [ParameterName("control"), ParameterInfo("Controlling voltage source")]
        public Identifier ControllingName { get; set; }

        /// <summary>
        /// Gets the controlling voltage source
        /// </summary>
        public VoltageSource ControllingSource { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CurrentControlledVoltageSourcePinCount = 2;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled current source</param>
        public CurrentControlledVoltageSource(Identifier name) 
            : base(name, CurrentControlledVoltageSourcePinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters());

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controllingSource">The controlling voltage source name</param>
        /// <param name="gain">The transresistance (gain)</param>
        public CurrentControlledVoltageSource(Identifier name, Identifier pos, Identifier neg, Identifier controllingSource, double gain) 
            : base(name, CurrentControlledVoltageSourcePinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters(gain));

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            Connect(pos, neg);
            ControllingName = controllingSource;
        }

        /// <summary>
        /// Setup the current-controlled voltage source
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public override void Setup(Simulation simulation)
        {
            var nodes = BindNodes(simulation);
            PosNode = nodes[0].Index;
            NegNode = nodes[1].Index;

            // Find the voltage source
            if (simulation.Circuit.Objects[ControllingName] is VoltageSource vsrc)
                ControllingSource = vsrc;
            else
                throw new CircuitException("{0}: Could not find voltage source '{1}'".FormatString(Name, ControllingName));
        }

        /// <summary>
        /// Setup data provider
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <returns></returns>
        protected override SetupDataProvider BuildSetupDataProvider(BehaviorPool pool)
        {
            var provider = base.BuildSetupDataProvider(pool);

            // Add the controlling source
            provider.Add("control", pool.GetEntityBehaviors(ControllingName));
            provider.Add("control", ControllingSource.ParameterSets);

            return provider;
        }
    }
}
