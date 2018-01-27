using System;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled current source
    /// </summary>
    [Pin(0, "F+"), Pin(1, "F-"), Connected(0, 0)]
    public class CurrentControlledCurrentSource : Component
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("control"), PropertyInfo("Name of the controlling source")]
        public Identifier ControllingName { get; set; }

        /// <summary>
        /// Nodes
        /// </summary>
        [PropertyName("pos_node"), PropertyInfo("Positive node of the source")]
        public int PosNode { get; private set; }
        [PropertyName("neg_node"), PropertyInfo("Negative node of the source")]
        public int NegNode { get; private set; }
        [PropertyName("vctrl"), PropertyInfo("Controlling voltage source")]
        public VoltageSource ControllingSource { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int CurrentControlledCurrentSourcePinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        public CurrentControlledCurrentSource(Identifier name) 
            : base(name, CurrentControlledCurrentSourcePinCount)
        {
            // Make sure the current controlled current source happens after voltage sources
            Priority = -1;

            // Add parameters
            Parameters.Add(new BaseParameters());

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="voltageSource">The name of the voltage source</param>
        /// <param name="gain">The current gain</param>
        public CurrentControlledCurrentSource(Identifier name, Identifier pos, Identifier neg, Identifier voltageSource, double gain)
            : base(name, CurrentControlledCurrentSourcePinCount)
        {
            // Register behaviors
            Priority = -1;

            // Add parameters
            Parameters.Add(new BaseParameters(gain));

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            // Connect
            Connect(pos, neg);
            ControllingName = voltageSource;
        }

        /// <summary>
        /// Setup the current controlled current source
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public override void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));

            var nodes = BindNodes(circuit);
            PosNode = nodes[0].Index;
            NegNode = nodes[1].Index;

            // Find the voltage source for which the current is being measured
            if (circuit.Objects[ControllingName] is VoltageSource vsrc)
                ControllingSource = vsrc;
            else
                throw new CircuitException($"{Name}: Could not find voltage source '{ControllingName}'");
        }

        /// <summary>
        /// Build the data provider
        /// </summary>
        /// <param name="pool">Behavior pool</param>
        /// <returns></returns>
        protected override Behaviors.SetupDataProvider BuildSetupDataProvider(Behaviors.BehaviorPool pool)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            var provider = base.BuildSetupDataProvider(pool);

            // Add behaviors and parameters of the controlling voltage source
            provider.Add(pool.GetEntityBehaviors(ControllingName));
            provider.Add(ControllingSource.Parameters);
            return provider;
        }
    }
}
