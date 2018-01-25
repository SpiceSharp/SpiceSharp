using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors.MUT;
using SpiceSharp.Components.MUT;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A mutual inductance
    /// </summary>
    public class MutualInductance : Component
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("inductor1"), PropertyInfo("First coupled inductor")]
        public Identifier MUTind1 { get; set; }
        [PropertyName("inductor2"), PropertyInfo("Second coupled inductor")]
        public Identifier MUTind2 { get; set; }

        /// <summary>
        /// Private variables
        /// </summary>
        public Inductor Inductor1 { get; private set; }
        public Inductor Inductor2 { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the mutual inductance</param>
        public MutualInductance(Identifier name) : base(name, 0)
        {
            // Make sure mutual inductances are evaluated AFTER inductors
            Priority = -1;

            // Add parameters
            Parameters.Add(new BaseParameters());

            // Add factories
            AddFactory(typeof(TransientBehavior), () => new TransientBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="ind1">Inductor 1</param>
        /// <param name="ind2">Inductor 2</param>
        /// <param name="coupling">Mutual inductance</param>
        public MutualInductance(Identifier name, Identifier ind1, Identifier ind2, double coupling)
            : base(name, 0)
        {
            // Make sure mutual inductances are evaluated AFTER inductors
            Priority = -1;

            // Add parameters
            Parameters.Add(new BaseParameters(coupling));

            // Add factories
            AddFactory(typeof(TransientBehavior), () => new TransientBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            // Connect
            MUTind1 = ind1;
            MUTind2 = ind2;
        }

        /// <summary>
        /// Setup the mutual inductance
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public override void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));

            // Get the inductors for the mutual inductance
            Inductor1 = circuit.Objects[MUTind1] as Inductor ?? throw new CircuitException($"{Name}: Could not find inductor '{MUTind1}'");
            Inductor2 = circuit.Objects[MUTind2] as Inductor ?? throw new CircuitException($"{Name}: Could not find inductor '{MUTind2}'");
        }

        /// <summary>
        /// Add inductances to the data provider for setting up behaviors
        /// </summary>
        /// <param name="pool">Behaviors</param>
        /// <returns></returns>
        protected override Behaviors.SetupDataProvider BuildSetupDataProvider(Behaviors.BehaviorPool pool)
        {
            // Base execution (will add entity behaviors and parameters for this mutual inductance)
            var data = base.BuildSetupDataProvider(pool);

            // Register inductor 1
            var eb = pool.GetEntityBehaviors(MUTind1) ?? throw new CircuitException($"{Name}: Could not find behaviors for inductor '{MUTind1}'");
            data.Add(eb);
            var parameters = Inductor1.Parameters;
            data.Add(parameters);

            // Register inductor 2
            eb = pool.GetEntityBehaviors(MUTind2) ?? throw new CircuitException($"{Name}: Could not find behaviors for inductor '{MUTind2}'");
            data.Add(eb);
            parameters = Inductor2.Parameters;
            data.Add(parameters);

            return data;
        }
    }
}
