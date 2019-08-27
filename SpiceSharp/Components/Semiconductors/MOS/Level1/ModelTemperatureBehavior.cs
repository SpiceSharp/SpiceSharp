﻿using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Temperature behavior for a <see cref="Model"/>
    /// </summary>
    public class ModelTemperatureBehavior : Behavior, ITemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        protected ModelBaseParameters ModelParameters { get; private set; }

        /// <summary>
        /// Gets implementation-specific factor 1.
        /// </summary>
        public double Factor1 { get; private set; }

        /// <summary>
        /// Gets the nominal thermal voltage.
        /// </summary>
        public double VtNominal { get; private set; }

        /// <summary>
        /// Gets the bandgap voltage.
        /// </summary>
        public double EgFet1 { get; private set; }

        /// <summary>
        /// Gets the implementaiton specific factor PbFactor1.
        /// </summary>
        public double PbFactor1 { get; private set; }


        /// <summary>
        /// Gets the biasing simulation state.
        /// </summary>
        /// <value>
        /// The biasing simulation state.
        /// </value>
        protected BiasingSimulationState BiasingState { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="ModelTemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public ModelTemperatureBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);
            
            // Get parameters
            ModelParameters = context.GetParameterSet<ModelBaseParameters>();

            // Get states
            BiasingState = context.States.Get<BiasingSimulationState>();
        }
        
        /// <summary>
        /// Do temperature-dependent calculations.
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            // Perform model defaulting
            if (!ModelParameters.NominalTemperature.Given)
                ModelParameters.NominalTemperature.RawValue = BiasingState.NominalTemperature;
            Factor1 = ModelParameters.NominalTemperature / Constants.ReferenceTemperature;
            VtNominal = ModelParameters.NominalTemperature * Constants.KOverQ;
            var kt1 = Constants.Boltzmann * ModelParameters.NominalTemperature;
            EgFet1 = 1.16 - 7.02e-4 * ModelParameters.NominalTemperature * ModelParameters.NominalTemperature / (ModelParameters.NominalTemperature + 1108);
            var arg1 = -EgFet1 / (kt1 + kt1) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature + Constants.ReferenceTemperature));
            PbFactor1 = -2 * VtNominal * (1.5 * Math.Log(Factor1) + Constants.Charge * arg1);

            if (ModelParameters.OxideThickness.Given && ModelParameters.OxideThickness > 0.0)
            {
                if (ModelParameters.SubstrateDoping.Given)
                {
                    if (ModelParameters.SubstrateDoping * 1e6 > 1.45e16)
                    {
                        if (!ModelParameters.Phi.Given)
                        {
                            ModelParameters.Phi.RawValue = 2 * VtNominal * Math.Log(ModelParameters.SubstrateDoping * 1e6 / 1.45e16);
                            ModelParameters.Phi.RawValue = Math.Max(.1, ModelParameters.Phi);
                        }

                        var fermis = ModelParameters.MosfetType * .5 * ModelParameters.Phi;
                        var wkfng = 3.2;
                        if (!ModelParameters.GateType.Given)
                            ModelParameters.GateType.RawValue = 1;
                        if (!ModelParameters.GateType.RawValue.Equals(0))
                        {
                            var fermig = ModelParameters.MosfetType * ModelParameters.GateType * .5 * EgFet1;
                            wkfng = 3.25 + .5 * EgFet1 - fermig;
                        }

                        var wkfngs = wkfng - (3.25 + .5 * EgFet1 + fermis);
                        if (!ModelParameters.Gamma.Given)
                        {
                            ModelParameters.Gamma.RawValue =
                                Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Constants.Charge * ModelParameters.SubstrateDoping * 1e6) /
                                ModelParameters.OxideCapFactor;
                        }

                        if (!ModelParameters.Vt0.Given)
                        {
                            if (!ModelParameters.SurfaceStateDensity.Given)
                                ModelParameters.SurfaceStateDensity.RawValue = 0;
                            var vfb = wkfngs - ModelParameters.SurfaceStateDensity * 1e4 * Constants.Charge / ModelParameters.OxideCapFactor;
                            ModelParameters.Vt0.RawValue = vfb + ModelParameters.MosfetType * (ModelParameters.Gamma * Math.Sqrt(ModelParameters.Phi) + ModelParameters.Phi);
                        }
                    }
                    else
                    {
                        ModelParameters.SubstrateDoping.RawValue = 0;
                        throw new CircuitException("{0}: Nsub < Ni".FormatString(Name));
                    }
                }
            }
        }
    }
}
