using System;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Mosfets.Level2
{
    /// <summary>
    /// This class allows calculation of dynamic (time-dependent) parameters for a <see cref="Mosfet2" />.
    /// </summary>
    /// <seealso cref="Biasing" />
    public abstract class Dynamic : Biasing
    {
        /// <summary>
        /// Gets the zero-bias bulk-drain junction capacitance.
        /// </summary>
        /// <value>
        /// The zero-bias bulk-drain capacitance.
        /// </value>
        [ParameterName("cbd0"), ParameterInfo("Zero-Bias B-D junction capacitance")]
        public double CapBd { get; private set; }

        /// <summary>
        /// Gets the zero-bias bulk-drain junction sidewall capacitance.
        /// </summary>
        /// <value>
        /// The zero-bias bulk-drain junction sidewall capacitance.
        /// </value>
        [ParameterName("cbdsw0"), ParameterInfo("Zero-Bias B-D sidewall capacitance")]
        public double CapBdSidewall { get; private set; }

        /// <summary>
        /// Gets the zero-bias bulk-source junction capacitance.
        /// </summary>
        /// <value>
        /// The zero-bias bulk-source junction capacitance.
        /// </value>
        [ParameterName("cbs0"), ParameterInfo("Zero-Bias B-S junction capacitance")]
        public double CapBs { get; private set; }

        /// <summary>
        /// Gets the zero-bias bulk-source junction sidewall capacitance.
        /// </summary>
        /// <value>
        /// The zero-bias bulk-source junction sidewall capacitance.
        /// </value>
        [ParameterName("cbssw0"), ParameterInfo("Zero-Bias B-S sidewall capacitance")]
        public double CapBsSidewall { get; private set; }

        /// <summary>
        /// Gets the stored bulk-source charge.
        /// </summary>
        /// <value>
        /// The bulk-source charge storage.
        /// </value>
        [ParameterName("qbs"), ParameterInfo("Bulk-Source charge storage")]
        public virtual double ChargeBs { get; protected set; }

        /// <summary>
        /// Gets or sets the stored bulk-drain charge.
        /// </summary>
        /// <value>
        /// The bulk-drain charge storage.
        /// </value>
        [ParameterName("qbd"), ParameterInfo("Bulk-Drain charge storage")]
        public virtual double ChargeBd { get; protected set; }

        /// <summary>
        /// Gets or sets the capacitance due to gate-source charge storage.
        /// </summary>
        /// <value>
        /// The gate-source capacitance.
        /// </value>
        [ParameterName("cgs"), ParameterInfo("Gate-Source capacitance")]
        public virtual double CapGs { get; protected set; }

        /// <summary>
        /// Gets or sets the capacitance due to gate-drain charge storage.
        /// </summary>
        [ParameterName("cgd"), ParameterInfo("Gate-Drain capacitance")]
        public virtual double CapGd { get; protected set; }

        /// <summary>
        /// Gets or sets the capacitance due to gate-bulk charge storage.
        /// </summary>
        /// <value>
        /// The gate-bulk capacitance.
        /// </value>
        [ParameterName("cgb"), ParameterInfo("Gate-Bulk capacitance")]
        public virtual double CapGb { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified bulk-drain capacitance.
        /// </summary>
        /// <value>
        /// The temperature-modified bulk-drain capacitance.
        /// </value>
        protected double TempCapBd { get; private set; }

        /// <summary>
        /// Gets the temperature-modified bulk-source capacitance.
        /// </summary>
        /// <value>
        /// The temperature-modified bulk-source capacitance.
        /// </value>
        protected double TempCapBs { get; private set; }

        /// <summary>
        /// Gets the temperature-modified junction capacitance.
        /// </summary>
        /// <value>
        /// The temperature-modified junction capacitance.
        /// </value>
        protected double TempJunctionCap { get; private set; }

        /// <summary>
        /// Gets the temperature-modified sidewall junction capacitance.
        /// </summary>
        /// <value>
        /// The temperature-modified sidewall junction capacitance.
        /// </value>
        protected double TempJunctionCapSidewall { get; private set; }

        /// <summary>
        /// Gets the temperature-modified depletion capacitance.
        /// </summary>
        /// <value>
        /// The temperature-modified depletion capacitance.
        /// </value>
        protected double TempDepletionCap { get; private set; }

        /// <summary>
        /// Gets the implementation-specific factor 2 - drain.
        /// </summary>
        /// <value>
        /// The implementation-specific factor 2 - drain.
        /// </value>
        protected double F2D { get; private set; }

        /// <summary>
        /// Gets the implementation-specific factor 3 - drain.
        /// </summary>
        /// <value>
        /// The implementation-specific factor 3 - drain.
        /// </value>
        protected double F3D { get; private set; }

        /// <summary>
        /// Gets the implementation-specific factor 4 - drain.
        /// </summary>
        /// <value>
        /// The implementation-specific factor 4 - drain.
        /// </value>
        protected double F4D { get; private set; }

        /// <summary>
        /// Gets the implementation-specific factor 2 - source.
        /// </summary>
        /// <value>
        /// the implementation-specific factor 2 - source.
        /// </value>
        protected double F2S { get; private set; }

        /// <summary>
        /// Gets the implementation-specific factor 3 - source.
        /// </summary>
        /// <value>
        /// The implementation-specific factor 3 - source.
        /// </value>
        protected double F3S { get; private set; }

        /// <summary>
        /// Gets the implementation-specific factor 4 - source.
        /// </summary>
        /// <value>
        /// The implementation-specific factor 4 - source.
        /// </value>
        protected double F4S { get; private set; }

        /// <summary>
        /// Gets the effective length.
        /// </summary>
        /// <value>
        /// The effective length.
        /// </value>
        protected double EffectiveLength { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dynamic"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        protected Dynamic(string name, ComponentBindingContext context) : base(name, context)
        {
        }

        /// <summary>
        /// Calculates the base capacitance parameters. Only needs to be calculated
        /// once when temperature or parameters have changed.
        /// </summary>
        protected void CalculateBaseCapacitances()
        {
            EffectiveLength = Parameters.Length - 2 * ModelParameters.LateralDiffusion;

            var pbo = (ModelParameters.BulkJunctionPotential - ModelTemperature.PbFactor1) / ModelTemperature.Factor1;
            var gmaold = (ModelParameters.BulkJunctionPotential - pbo) / pbo;
            var capfact = 1 / (1 + ModelParameters.BulkJunctionBotGradingCoefficient *
                               (4e-4 * (ModelParameters.NominalTemperature - Constants.ReferenceTemperature) -
                                gmaold));
            TempCapBd = ModelParameters.CapBd * capfact;
            TempCapBs = ModelParameters.CapBs * capfact;
            TempJunctionCap = ModelParameters.BulkCapFactor * capfact;
            capfact = 1 / (1 + ModelParameters.BulkJunctionSideGradingCoefficient *
                           (4e-4 * (ModelParameters.NominalTemperature - Constants.ReferenceTemperature) - gmaold));
            TempJunctionCapSidewall = ModelParameters.SidewallCapFactor * capfact;
            var gmanew = (TempBulkPotential - pbo) / pbo;
            capfact = 1 + ModelParameters.BulkJunctionBotGradingCoefficient *
                      (4e-4 * (Parameters.Temperature - Constants.ReferenceTemperature) - gmanew);
            TempCapBd *= capfact;
            TempCapBs *= capfact;
            TempJunctionCap *= capfact;
            capfact = 1 + ModelParameters.BulkJunctionSideGradingCoefficient *
                      (4e-4 * (Parameters.Temperature - Constants.ReferenceTemperature) - gmanew);
            TempJunctionCapSidewall *= capfact;
            TempDepletionCap = ModelParameters.ForwardCapDepletionCoefficient * TempBulkPotential;

            double cz, czsw;
            if (ModelParameters.CapBd.Given)
                cz = TempCapBd;
            else
            {
                if (ModelParameters.BulkCapFactor.Given)
                    cz = TempJunctionCap * Parameters.DrainArea;
                else
                    cz = 0;
            }

            if (ModelParameters.SidewallCapFactor.Given)
                czsw = TempJunctionCapSidewall * Parameters.DrainPerimeter;
            else
                czsw = 0;
            CapBd = cz;
            CapBdSidewall = czsw;

            // Calculate capacitance factors
            var arg = 1 - ModelParameters.ForwardCapDepletionCoefficient;
            var sarg = Math.Exp(-ModelParameters.BulkJunctionBotGradingCoefficient * Math.Log(arg));
            var sargsw = Math.Exp(-ModelParameters.BulkJunctionSideGradingCoefficient * Math.Log(arg));
            F2D = CapBd * (1 - ModelParameters.ForwardCapDepletionCoefficient *
                           (1 + ModelParameters.BulkJunctionBotGradingCoefficient)) * sarg / arg + CapBdSidewall *
                  (1 - ModelParameters.ForwardCapDepletionCoefficient *
                   (1 + ModelParameters.BulkJunctionSideGradingCoefficient)) * sargsw / arg;
            F3D = CapBd * ModelParameters.BulkJunctionBotGradingCoefficient * sarg / arg / TempBulkPotential +
                  CapBdSidewall * ModelParameters.BulkJunctionSideGradingCoefficient *
                  sargsw / arg / TempBulkPotential;
            F4D = CapBd * TempBulkPotential * (1 - arg * sarg) /
                  (1 - ModelParameters.BulkJunctionBotGradingCoefficient) + CapBdSidewall * TempBulkPotential *
                  (1 - arg *
                   sargsw) / (1 - ModelParameters.BulkJunctionSideGradingCoefficient) -
                  F3D / 2 * (TempDepletionCap * TempDepletionCap) - TempDepletionCap * F2D;

            F2S = CapBs * (1 - ModelParameters.ForwardCapDepletionCoefficient *
                           (1 + ModelParameters.BulkJunctionBotGradingCoefficient)) * sarg / arg + CapBsSidewall *
                  (1 -
                   ModelParameters.ForwardCapDepletionCoefficient *
                   (1 + ModelParameters.BulkJunctionSideGradingCoefficient)) * sargsw / arg;
            F3S = CapBs * ModelParameters.BulkJunctionBotGradingCoefficient * sarg / arg / TempBulkPotential +
                  CapBsSidewall * ModelParameters.BulkJunctionSideGradingCoefficient *
                  sargsw / arg / TempBulkPotential;
            F4S = CapBs * TempBulkPotential * (1 - arg * sarg) /
                  (1 - ModelParameters.BulkJunctionBotGradingCoefficient) + CapBsSidewall * TempBulkPotential *
                  (1 - arg *
                   sargsw) / (1 - ModelParameters.BulkJunctionSideGradingCoefficient) -
                  F3S / 2 * (TempDepletionCap * TempDepletionCap) - TempDepletionCap * F2S;

            if (ModelParameters.CapBs.Given)
                cz = TempCapBs;
            else
            {
                if (ModelParameters.BulkCapFactor.Given)
                    cz = TempJunctionCap * Parameters.SourceArea;
                else
                    cz = 0;
            }

            if (ModelParameters.SidewallCapFactor.Given)
                czsw = TempJunctionCapSidewall * Parameters.SourcePerimeter;
            else
                czsw = 0;
            CapBs = cz;
            CapBsSidewall = czsw;
        }

        /// <summary>
        /// Calculates the capacitances based on the current biasing point.
        /// </summary>
        /// <param name="vds">The drain-source voltage.</param>
        /// <param name="vbs">The bulk-source voltage.</param>
        protected void CalculateCapacitances(double vds, double vbs)
        {
            var vbd = vbs - vds;

            /*
            * now we do the hard part of the bulk - drain and bulk - source
            * diode - we evaluate the non - linear capacitance and
            * charge
            *
            * the basic equations are not hard, but the implementation
            * is somewhat long in an attempt to avoid log / exponential
            * evaluations
            */
            /*
            * charge storage elements
            *
            * .. bulk - drain and bulk - source depletion capacitances
            */
            if (vbs < TempDepletionCap)
            {
                double arg = 1 - vbs / TempBulkPotential, sarg, sargsw;

                /*
                * the following block looks somewhat long and messy,
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (ModelParameters.BulkJunctionBotGradingCoefficient.Equals(ModelParameters.BulkJunctionSideGradingCoefficient))
                {
                    if (ModelParameters.BulkJunctionBotGradingCoefficient.Equals(0.5))
                    {
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        sarg = sargsw = Math.Exp(-ModelParameters.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                }
                else
                {
                    if (ModelParameters.BulkJunctionBotGradingCoefficient.Equals(0.5))
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-ModelParameters.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                    if (ModelParameters.BulkJunctionSideGradingCoefficient.Equals(0.5))
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-ModelParameters.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                    }
                }
                ChargeBs = TempBulkPotential * (CapBs * (1 - arg * sarg) / (1 - ModelParameters.BulkJunctionBotGradingCoefficient) +
                    CapBsSidewall * (1 - arg * sargsw) / (1 - ModelParameters.BulkJunctionSideGradingCoefficient));
                CapBs = CapBs * sarg + CapBsSidewall * sargsw;
            }
            else
            {
                ChargeBs = F4S + vbs * (F2S + vbs * (F3S / 2));
                CapBs = F2S + F3S * vbs;
            }

            if (vbd < TempDepletionCap)
            {
                double arg = 1 - vbd / TempBulkPotential, sarg, sargsw;
                /*
                * the following block looks somewhat long and messy,
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (ModelParameters.BulkJunctionBotGradingCoefficient.Equals(0.5) && ModelParameters.BulkJunctionSideGradingCoefficient.Equals(0.5))
                {
                    sarg = sargsw = 1 / Math.Sqrt(arg);
                }
                else
                {
                    if (ModelParameters.BulkJunctionBotGradingCoefficient.Equals(0.5))
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-ModelParameters.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                    if (ModelParameters.BulkJunctionSideGradingCoefficient.Equals(0.5))
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-ModelParameters.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                    }
                }

                ChargeBd = TempBulkPotential * (CapBd * (1 - arg * sarg) / (1 - ModelParameters.BulkJunctionBotGradingCoefficient) +
                    CapBdSidewall * (1 - arg * sargsw) / (1 - ModelParameters.BulkJunctionSideGradingCoefficient));
                CapBd = CapBd * sarg + CapBdSidewall * sargsw;
            }
            else
            {
                ChargeBd = F4D + vbd * (F2D + vbd * F3D / 2);
                CapBd = F2D + vbd * F3D;
            }
        }

        /// <summary>
        /// Calculates the Meyer capacitors.
        /// </summary>
        /// <param name="vgs">The gate-source voltage.</param>
        /// <param name="vgd">The gate-drain voltage.</param>
        protected void CalculateMeyerCharges(double vgs, double vgd)
        {
            var von = ModelParameters.MosfetType * Von;
            var vdsat = ModelParameters.MosfetType * SaturationVoltageDs;
            var oxideCap = ModelParameters.OxideCapFactor * EffectiveLength * Parameters.Width;

            /* 
             * calculate meyer's capacitors
             */
            double icapgs, icapgd, icapgb;
            if (Mode > 0)
            {
                Transistor.MeyerCharges(vgs, vgd, von, vdsat,
                    out icapgs, out icapgd, out icapgb, TempPhi, oxideCap);
            }
            else
            {
                Transistor.MeyerCharges(vgd, vgs, von, vdsat,
                    out icapgd, out icapgs, out icapgb, TempPhi, oxideCap);
            }
            CapGs = icapgs;
            CapGd = icapgd;
            CapGb = icapgb;
        }
    }
}
