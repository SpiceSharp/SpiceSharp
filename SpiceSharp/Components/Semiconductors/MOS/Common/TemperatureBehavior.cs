using System;
using System.Collections.Generic;
using System.Text;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    public abstract class TemperatureBehavior : BaseTemperatureBehavior
    {
        /// <summary>
        /// Gets or sets the source conductance.
        /// </summary>
        /// <value>
        /// The source conductance.
        /// </value>
        [ParameterName("sourceconductance"), ParameterInfo("Conductance of source")]
        public double SourceConductance { get; protected set; }

        /// <summary>
        /// Gets or sets the drain conductance.
        /// </summary>
        /// <value>
        /// The drain conductance.
        /// </value>
        [ParameterName("drainconductance"), ParameterInfo("Conductance of drain")]
        public double DrainConductance { get; protected set; }

        /// <summary>
        /// Gets the source resistance.
        /// </summary>
        /// <value>
        /// The source resistance.
        /// </value>
        [ParameterName("rs"), ParameterInfo("Source resistance")]
        public double SourceResistance
        {
            get
            {
                if (SourceConductance > 0.0)
                    return 1.0 / SourceConductance;
                return 0.0;
            }
        }

        /// <summary>
        /// Gets the drain resistance.
        /// </summary>
        /// <value>
        /// The drain resistance.
        /// </value>
        [ParameterName("rd"), ParameterInfo("Drain conductance")]
        public double DrainResistance
        {
            get
            {
                if (DrainConductance > 0.0)
                    return 1.0 / DrainConductance;
                return 0.0;
            }
        }

        /// <summary>
        /// Gets or sets the critical source voltage.
        /// </summary>
        /// <value>
        /// The critical source voltage.
        /// </value>
        [ParameterName("sourcevcrit"), ParameterInfo("Critical source voltage")]
        public double SourceVCritical { get; protected set; }

        /// <summary>
        /// Gets or sets the critical drain voltage.
        /// </summary>
        /// <value>
        /// The drain drain voltage.
        /// </value>
        [ParameterName("drainvcrit"), ParameterInfo("Critical drain voltage")]
        public double DrainVCritical { get; protected set; }

        public double TempSaturationCurrent { get; protected set; }
        public double TempSaturationCurrentDensity { get; protected set; }
        public double TempTransconductance { get; protected set; }
        public double TempVt0 { get; protected set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected TemperatureBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        public override void Temperature(BaseSimulation simulation)
        {
            
        }
    }
}
