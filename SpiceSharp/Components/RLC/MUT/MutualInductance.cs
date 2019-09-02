using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components.MutualInductanceBehaviors;
using SpiceSharp.Simulations;

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
        /// Creates behaviors for the specified simulation that describe this <see cref="Entity"/>.
        /// </summary>
        /// <param name="simulation">The simulation requesting the behaviors.</param>
        /// <param name="entities">The entities being processed, used by the entity to find linked entities.</param>
        /// <remarks>
        /// The order typically indicates hierarchy. The entity will create the behaviors in reverse order, allowing
        /// the most specific child class to be used that is necessary. For example, the <see cref="OP" /> simulation needs
        /// <see cref="ITemperatureBehavior" /> and an <see cref="IBiasingBehavior" />. The entity will first look for behaviors
        /// of type <see cref="IBiasingBehavior" />, and then for the behaviors of type <see cref="ITemperatureBehavior" />. However,
        /// if the behavior that was created for <see cref="IBiasingBehavior" /> also implements <see cref="ITemperatureBehavior" />,
        /// then then entity will not create a new instance of the behavior.
        /// </remarks>
        public override void CreateBehaviors(ISimulation simulation, IEntityCollection entities)
        {
            entities[InductorName1.ThrowIfNull("primary inductor")].CreateBehaviors(simulation, entities);
            entities[InductorName2.ThrowIfNull("secondary inductor")].CreateBehaviors(simulation, entities);
            base.CreateBehaviors(simulation, entities);
        }

        /// <summary>
        /// Build the binding context.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <returns></returns>
        protected override ComponentBindingContext BuildBindingContext(ISimulation simulation)
        {
            var context = base.BuildBindingContext(simulation);

            // Register inductor 1
            context.Add("inductor1", simulation.EntityParameters[InductorName1]);
            context.Add("inductor1", simulation.EntityBehaviors[InductorName1]);

            // Register inductor 2
            context.Add("inductor2", simulation.EntityParameters[InductorName2]);
            context.Add("inductor2", simulation.EntityBehaviors[InductorName2]);

            return context;
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
