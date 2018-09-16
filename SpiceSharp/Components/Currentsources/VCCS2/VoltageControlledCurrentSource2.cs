﻿using SpiceSharp.Attributes;
using SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors2;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent current source
    /// </summary>
    [Pin(0, "I+"), Pin(1, "I-"), Connected]
    public class VoltageControlledCurrentSource2 : Component
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CurrentSourcePinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        public VoltageControlledCurrentSource2(Identifier name) 
            : base(name, CurrentSourcePinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters());
            ParameterSets.Add(new FrequencyParameters());

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public VoltageControlledCurrentSource2(Identifier name, Identifier pos, Identifier neg)
            : base(name, CurrentSourcePinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters());
            ParameterSets.Add(new FrequencyParameters());

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));

            // Connect
            Connect(pos, neg);
        }
    }
}
