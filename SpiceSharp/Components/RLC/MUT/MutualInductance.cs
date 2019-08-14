using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components.MutualInductanceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A mutual inductance
    /// </summary>
    public class MutualInductance : Component
    {
        static MutualInductance()
        {
            RegisterBehaviorFactory(typeof(MutualInductance), new BehaviorFactoryDictionary
            {
                {typeof(TransientBehavior), e => new TransientBehavior(e.Name)},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Gets or sets the name of the primary inductor.
        /// </summary>
        [ParameterName("inductor1"), ParameterName("primary"), ParameterInfo("First coupled inductor")]
        public string InductorName1 { get; set; }

        /// <summary>
        /// Gets or sets the name of the secondary inductor.
        /// </summary>
        [ParameterName("inductor2"), ParameterName("secondary"), ParameterInfo("Second coupled inductor")]
        public string InductorName2 { get; set; }

        /// <summary>
        /// Create a new instance of the <see cref="MutualInductance"/> class.
        /// </summary>
        /// <param name="name">The name of the mutual inductance</param>
        public MutualInductance(string name) : base(name, 0)
        {
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Create a new instance of the <see cref="MutualInductance"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="inductorName1">Inductor 1</param>
        /// <param name="inductorName2">Inductor 2</param>
        /// <param name="coupling">Mutual inductance</param>
        public MutualInductance(string name, string inductorName1, string inductorName2, double coupling)
            : base(name, 0)
        {
            ParameterSets.Add(new BaseParameters(coupling));
            InductorName1 = inductorName1;
            InductorName2 = inductorName2;
        }

        /// <summary>
        /// Add inductances to the data provider for setting up behaviors
        /// </summary>
        /// <returns></returns>
        protected override SetupDataProvider BuildSetupDataProvider(ParameterPool parameters, BehaviorPool behaviors)
        {
            parameters.ThrowIfNull(nameof(parameters));
            behaviors.ThrowIfNull(nameof(behaviors));

            // Base execution (will add entity behaviors and parameters for this mutual inductance)
            var data = base.BuildSetupDataProvider(parameters, behaviors);

            // Register inductor 1
            data.Add("inductor1", parameters[InductorName1]);
            data.Add("inductor1", behaviors[InductorName1]);

            // Register inductor 2
            data.Add("inductor2", parameters[InductorName2]);
            data.Add("inductor2", behaviors[InductorName2]);

            return data;
        }

        /// <summary>
        /// Clone the mutual inductance
        /// </summary>
        /// <param name="data">Instance data.</param>
        /// <returns></returns>
        public override Entity Clone(InstanceData data)
        {
            var clone = (MutualInductance) base.Clone(data);
            if (data is ComponentInstanceData cid)
            {
                if (clone.InductorName1 != null)
                    clone.InductorName1 = cid.GenerateIdentifier(clone.InductorName1);
                if (clone.InductorName2 != null)
                    clone.InductorName2 = cid.GenerateIdentifier(clone.InductorName2);
            }
            return clone;
        }
    }
}
