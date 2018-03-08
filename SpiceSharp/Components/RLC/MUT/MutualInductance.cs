using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.MutualInductanceBehaviors;
using SpiceSharp.Diagnostics;

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
        public Identifier InductorName1 { get; set; }
        [PropertyName("inductor2"), PropertyInfo("Second coupled inductor")]
        public Identifier InductorName2 { get; set; }

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
            ParameterSets.Add(new BaseParameters());

            // Add factories
            Behaviors.Add(typeof(TransientBehavior), () => new TransientBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="inductorName1">Inductor 1</param>
        /// <param name="inductorName2">Inductor 2</param>
        /// <param name="coupling">Mutual inductance</param>
        public MutualInductance(Identifier name, Identifier inductorName1, Identifier inductorName2, double coupling)
            : base(name, 0)
        {
            // Make sure mutual inductances are evaluated AFTER inductors
            Priority = -1;

            // Add parameters
            ParameterSets.Add(new BaseParameters(coupling));

            // Add factories
            Behaviors.Add(typeof(TransientBehavior), () => new TransientBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            // Connect
            InductorName1 = inductorName1;
            InductorName2 = inductorName2;
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
            Inductor1 = circuit.Objects[InductorName1] as Inductor ?? throw new CircuitException("{0}: Could not find inductor '{1}'".FormatString(Name, InductorName1));
            Inductor2 = circuit.Objects[InductorName2] as Inductor ?? throw new CircuitException("{0}: Could not find inductor '{1}'".FormatString(Name, InductorName2));
        }

        /// <summary>
        /// Add inductances to the data provider for setting up behaviors
        /// </summary>
        /// <param name="pool">Behaviors</param>
        /// <returns></returns>
        protected override SetupDataProvider BuildSetupDataProvider(BehaviorPool pool)
        {
            // Base execution (will add entity behaviors and parameters for this mutual inductance)
            var data = base.BuildSetupDataProvider(pool);

            // Register inductor 1
            var eb = pool.GetEntityBehaviors(InductorName1) ?? throw new CircuitException("{0}: Could not find behaviors for inductor '{1}'".FormatString(Name, InductorName1));
            data.Add("inductor1", eb);
            var parameters = Inductor1.ParameterSets;
            data.Add("inductor1", parameters);

            // Register inductor 2
            eb = pool.GetEntityBehaviors(InductorName2) ?? throw new CircuitException("{0}: Could not find behaviors for inductor '{1}'".FormatString(Name, InductorName2));
            data.Add("inductor2", eb);
            parameters = Inductor2.ParameterSets;
            data.Add("inductor2", parameters);

            return data;
        }
    }
}
