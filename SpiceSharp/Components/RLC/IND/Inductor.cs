﻿using SpiceSharp.Attributes;
using SpiceSharp.Behaviors.IND;
using SpiceSharp.Components.IND;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An inductor
    /// </summary>
    [Pins("L+", "L-")]
    public class Inductor : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        public int INDposNode { get; internal set; }
        public int INDnegNode { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int INDpinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        public Inductor(Identifier name)
            : base(name, INDpinCount)
        {
            // Add parameters
            Parameters.Add(new BaseParameters());

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(TransientBehavior), () => new TransientBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="ind">The inductance</param>
        public Inductor(Identifier name, Identifier pos, Identifier neg, double ind) 
            : base(name, INDpinCount)
        {
            // Add parameters
            Parameters.Add(new BaseParameters(ind));

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(TransientBehavior), () => new TransientBehavior(Name));

            // Connect
            Connect(pos, neg);
        }

        /// <summary>
        /// Setup the inductor
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public override void Setup(Circuit circuit)
        {
            var nodes = BindNodes(circuit);
            INDposNode = nodes[0].Index;
            INDnegNode = nodes[1].Index;
        }
    }
}
