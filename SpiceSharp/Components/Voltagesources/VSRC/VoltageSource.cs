﻿using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Components.VoltageSources;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System;
using System.Linq;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent voltage source.
    /// </summary>
    /// <seealso cref="Component"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="IRuleSubject"/>
    /// <seealso cref="IndependentSourceParameters"/>
    [Pin(0, "V+"), Pin(1, "V-"), VoltageDriver(0, 1), IndependentSource]
    public class VoltageSource : Component,
        IParameterized<IndependentSourceParameters>,
        IRuleSubject
    {
        /// <inheritdoc/>
        public IndependentSourceParameters Parameters { get; } = new IndependentSourceParameters();

        /// <summary>
        /// The pin count for a voltage source.
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
        public const int PinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public VoltageSource(string name)
            : base(name, PinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public VoltageSource(string name, string pos, string neg, double dc)
            : this(name)
        {
            Parameters.DcValue = dc;
            Connect(pos, neg);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="waveform">The waveform</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public VoltageSource(string name, string pos, string neg, IWaveformDescription waveform)
            : this(name)
        {
            Parameters.Waveform = waveform;
            Connect(pos, neg);
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new ComponentBindingContext(this, simulation, behaviors, LinkParameters);
            behaviors
                .AddIfNo<IAcceptBehavior>(simulation, () => new Accept(Name, context))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }

        void IRuleSubject.Apply(IRules rules)
        {
            var p = rules.GetParameterSet<ComponentRuleParameters>();
            var nodes = Nodes.Select(name => p.Factory.GetSharedVariable(name)).ToArray();
            foreach (var rule in rules.GetRules<IConductiveRule>())
                rule.AddPath(this, nodes[0], nodes[1]);
            foreach (var rule in rules.GetRules<IAppliedVoltageRule>())
                rule.Fix(this, nodes[0], nodes[1]);
        }
    }
}
