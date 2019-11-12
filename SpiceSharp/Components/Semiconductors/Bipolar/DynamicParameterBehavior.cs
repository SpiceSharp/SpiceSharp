﻿using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Common dynamic (time-dependent) parameter logic for a <see cref="BipolarJunctionTransistor" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.BipolarBehaviors.BiasingBehavior" />
    public abstract class DynamicParameterBehavior : BiasingBehavior
    {
        /// <summary>
        /// Gets the internal base-emitter capacitance.
        /// </summary>
        [ParameterName("cpi"), ParameterInfo("Internal base to emitter capactance")]
        public double CapBe { get; private set; }

        /// <summary>
        /// Gets the internal bsae-collector capacitance.
        /// </summary>
        [ParameterName("cmu"), ParameterInfo("Internal base to collector capactiance")]
        public double CapBc { get; private set; }

        /// <summary>
        /// Gets the base-collector capacitance.
        /// </summary>
        [ParameterName("cbx"), ParameterInfo("Base to collector capacitance")]
        public double CapBx { get; private set; }

        /// <summary>
        /// Gets the collector-substrate capacitance.
        /// </summary>
        [ParameterName("ccs"), ParameterInfo("Collector to substrate capacitance")]
        public double CapCs { get; private set; }

        /// <summary>
        /// Gets or sets the base-emitter charge storage.
        /// </summary>
        [ParameterName("qbe"), ParameterInfo("Charge storage B-E junction")]
        public virtual double ChargeBe { get; protected set; }

        /// <summary>
        /// Gets or sets the base-collector charge storage.
        /// </summary>
        [ParameterName("qbc"), ParameterInfo("Charge storage B-C junction")]
        public virtual double ChargeBc { get; protected set; }

        /// <summary>
        /// Gets or sets the base-X charge storage.
        /// </summary>
        [ParameterName("qbx"), ParameterInfo("Charge storage B-X junction")]
        public virtual double ChargeBx { get; protected set; }

        /// <summary>
        /// Gets or sets the collector-substract charge storage.
        /// </summary>
        [ParameterName("qcs"), ParameterInfo("Charge storage C-S junction")]
        public virtual double ChargeCs { get; protected set; }

        /// <summary>
        /// Gets the small-signal equivalent collector-bulk conductance.
        /// </summary>
        protected double Geqcb { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicParameterBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        protected DynamicParameterBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
        }

        /// <summary>
        /// Calculates the capacitances.
        /// </summary>
        protected void CalculateCapacitances(double vbe, double vbc, double vbx, double vcs)
        {
            var cbe = CurrentBe;
            var gbe = CondBe;
            var cbc = CurrentBc;
            var gbc = CondBc;
            var qb = BaseCharge;
            double f1, f2, f3;

            // Charge storage elements
            double tf = ModelParameters.TransitTimeForward;
            double tr = ModelParameters.TransitTimeReverse;
            var czbe = TempBeCap * BaseParameters.Area;
            var pe = TempBePotential;
            double xme = ModelParameters.JunctionExpBe;
            double cdis = ModelParameters.BaseFractionBcCap;
            var ctot = TempBcCap * BaseParameters.Area;
            var czbc = ctot * cdis;
            var czbx = ctot - czbc;
            var pc = TempBcPotential;
            double xmc = ModelParameters.JunctionExpBc;
            var fcpe = TempDepletionCap;
            var czcs = ModelParameters.CapCs * BaseParameters.Area;
            double ps = ModelParameters.PotentialSubstrate;
            double xms = ModelParameters.ExponentialSubstrate;
            double xtf = ModelParameters.TransitTimeBiasCoefficientForward;
            var ovtf = ModelTemperature.TransitTimeVoltageBcFactor;
            var xjtf = ModelParameters.TransitTimeHighCurrentForward * BaseParameters.Area;
            if (!tf.Equals(0) && vbe > 0) // Avoid computations
            {
                double argtf = 0;
                double arg2 = 0;
                double arg3 = 0;
                if (!xtf.Equals(0)) // Avoid computations
                {
                    argtf = xtf;
                    if (!ovtf.Equals(0)) // Avoid expensive Exp()
                    {
                        argtf = argtf * Math.Exp(vbc * ovtf);
                    }
                    arg2 = argtf;
                    if (!xjtf.Equals(0)) // Avoid computations
                    {
                        var tmp = cbe / (cbe + xjtf);
                        argtf = argtf * tmp * tmp;
                        arg2 = argtf * (3 - tmp - tmp);
                    }
                    arg3 = cbe * argtf * ovtf;
                }
                cbe = cbe * (1 + argtf) / qb;
                gbe = (gbe * (1 + arg2) - cbe * Dqbdve) / qb;
                Geqcb = tf * (arg3 - cbe * Dqbdvc) / qb;
            }
            if (vbe < fcpe)
            {
                var arg = 1 - vbe / pe;
                var sarg = Math.Exp(-xme * Math.Log(arg));
                ChargeBe = tf * cbe + pe * czbe * (1 - arg * sarg) / (1 - xme);
                CapBe = tf * gbe + czbe * sarg;
            }
            else
            {
                f1 = TempFactor1;
                f2 = ModelTemperature.F2;
                f3 = ModelTemperature.F3;
                var czbef2 = czbe / f2;
                ChargeBe = tf * cbe + czbe * f1 + czbef2 * (f3 * (vbe - fcpe) + xme / (pe + pe) * (vbe * vbe -
                     fcpe * fcpe));
                CapBe = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
            }
            var fcpc = TempFactor4;
            f1 = TempFactor5;
            f2 = ModelTemperature.F6;
            f3 = ModelTemperature.F7;
            if (vbc < fcpc)
            {
                var arg = 1 - vbc / pc;
                var sarg = Math.Exp(-xmc * Math.Log(arg));
                ChargeBc = tr * cbc + pc * czbc * (1 - arg * sarg) / (1 - xmc);
                CapBc = tr * gbc + czbc * sarg;
            }
            else
            {
                var czbcf2 = czbc / f2;
                ChargeBc = tr * cbc + czbc * f1 + czbcf2 * (f3 * (vbc - fcpc) + xmc / (pc + pc) * (vbc * vbc -
                     fcpc * fcpc));
                CapBc = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
            }
            if (vbx < fcpc)
            {
                var arg = 1 - vbx / pc;
                var sarg = Math.Exp(-xmc * Math.Log(arg));
                ChargeBx = pc * czbx * (1 - arg * sarg) / (1 - xmc);
                CapBx = czbx * sarg;
            }
            else
            {
                var czbxf2 = czbx / f2;
                ChargeBx = czbx * f1 + czbxf2 * (f3 * (vbx - fcpc) + xmc / (pc + pc) * (vbx * vbx - fcpc * fcpc));
                CapBx = czbxf2 * (f3 + xmc * vbx / pc);
            }
            if (vcs < 0)
            {
                var arg = 1 - vcs / ps;
                var sarg = Math.Exp(-xms * Math.Log(arg));
                ChargeCs = ps * czcs * (1 - arg * sarg) / (1 - xms);
                CapCs = czcs * sarg;
            }
            else
            {
                ChargeCs = vcs * czcs * (1 + xms * vcs / (2 * ps));
                CapCs = czcs * (1 + xms * vcs / ps);
            }
        }
    }
}
