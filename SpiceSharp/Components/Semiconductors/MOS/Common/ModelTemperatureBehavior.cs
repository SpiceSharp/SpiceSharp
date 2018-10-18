using System;
using System.Collections.Generic;
using System.Text;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    public abstract class ModelTemperatureBehavior : BaseTemperatureBehavior
    {
        public double Factor1 { get; protected set; }
        public double VtNominal { get; protected set; }
        public double EgFet1 { get; protected set; }
        public double PbFactor1 { get; protected set; }
        public double OxideCapFactor { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected ModelTemperatureBehavior(string name) : base(name)
        {
        }
    }
}
