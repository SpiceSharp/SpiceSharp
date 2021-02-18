﻿using SpiceSharp.Attributes;
using SpiceSharp.Components.Switches;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="VoltageSwitch"/>
    /// </summary>
    [AutoGeneratedBehaviors]
    public partial class VoltageSwitchModel : Entity,
        IParameterized<VoltageModelParameters>
    {
        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public VoltageModelParameters Parameters { get; } = new VoltageModelParameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSwitchModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model</param>
        public VoltageSwitchModel(string name)
            : base(name)
        {
        }
    }
}
