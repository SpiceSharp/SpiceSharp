using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors.CSW;
using SpiceSharp.Components.CSW;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled switch
    /// </summary>
    [PinsAttribute("W+", "W-")]
    public class CurrentSwitch : Component
    {
        /// <summary>
        /// Set the model for the current-controlled switch
        /// </summary>
        public void SetModel(CurrentSwitchModel model) => Model = model;
        
        /// <summary>
        /// Nodes
        /// </summary>
        [PropertyName("pos_node"), PropertyInfo("Positive node of the switch")]
        public int CSWposNode { get; internal set; }
        [PropertyName("neg_node"), PropertyInfo("Negative node of the switch")]
        public int CSWnegNode { get; internal set; }
        [PropertyName("control"), PropertyInfo("Name of the controlling source")]
        public Identifier CSWcontName { get; set; }

        /// <summary>
        /// Get the controlling voltage source
        /// </summary>
        public Voltagesource CSWcontSource { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int CSWpinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled switch</param>
        public CurrentSwitch(Identifier name) : base(name, CSWpinCount)
        {
            // Make sure the current switch is processed after voltage sources
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
        /// <param name="name">The name of the current-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="vsource">The controlling voltage source</param>
        public CurrentSwitch(Identifier name, Identifier pos, Identifier neg, Identifier vsource)
            : base(name, CSWpinCount)
        {
            // Make sure the current switch is processed after voltage sources
            Priority = -1;

            // Add parameters
            Parameters.Add(new BaseParameters());

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            Connect(pos, neg);
            CSWcontName = vsource;
        }

        /// <summary>
        /// Setup the current-controlled switch
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            CSWposNode = nodes[0].Index;
            CSWnegNode = nodes[1].Index;

            // Find the voltage source
            if (ckt.Objects[CSWcontName] is Voltagesource vsrc)
                CSWcontSource = vsrc;
        }

        /// <summary>
        /// Build data provider
        /// </summary>
        /// <param name="pool">Behaviors</param>
        /// <returns></returns>
        protected override Behaviors.SetupDataProvider BuildSetupDataProvider(Behaviors.BehaviorPool pool)
        {
            var provider = base.BuildSetupDataProvider(pool);

            // Add controlling voltage source data
            provider.Add(pool.GetEntityBehaviors(CSWcontName));
            provider.Add(CSWcontSource.Parameters);
            return provider;
        }
    }
}
