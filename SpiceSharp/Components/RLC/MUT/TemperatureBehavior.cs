﻿using System;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// Temperature-dependent calculations for a <see cref="MutualInductance"/>.
    /// </summary>
    public class TemperatureBehavior : Behavior, ITemperatureBehavior
    {
        /// <summary>
        /// Gets the coupling factor.
        /// </summary>
        protected double Factor { get; private set; }

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the base parameters of inductor 1.
        /// </summary>
        protected InductorBehaviors.BaseParameters BaseParameters1 { get; private set; }

        /// <summary>
        /// Gets the base parameters of inductor 2.
        /// </summary>
        protected InductorBehaviors.BaseParameters BaseParameters2 { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        public TemperatureBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            // Get parameters
            BaseParameters = context.GetParameterSet<BaseParameters>();
            BaseParameters1 = context.GetParameterSet<InductorBehaviors.BaseParameters>("inductor1");
            BaseParameters2 = context.GetParameterSet<InductorBehaviors.BaseParameters>("inductor2");
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            // Calculate coupling factor
            Factor = BaseParameters.Coupling * Math.Sqrt(BaseParameters1.Inductance * BaseParameters2.Inductance);
        }
    }
}
