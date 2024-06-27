using System;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// A class that computes the bulk-source and bulk-drain charges and capacitances.
    /// </summary>
    public class MosfetCharges
    {
        /// <include file='docs.xml' path='docs/members/BulkSourceCharge/*'/>
        public double Qbs { get; set; }

        /// <include file='docs.xml' path='docs/members/BulkDrainCharge/*'/>
        public double Qbd { get; set; }

        /// <include file='docs.xml' path='docs/members/BulkSourceCapacitance/*'/>
        public double Cbs { get; set; }

        /// <include file='docs.xml' path='docs/members/BulkDrainCapacitance/*'/>
        public double Cbd { get; set; }

        /// <include file='docs.xml' path='docs/members/GateSourceCapacitance/*'/>
        public double Cgs { get; set; }

        /// <include file='docs.xml' path='docs/members/GateDrainCapacitance/*'/>
        public double Cgd { get; set; }

        /// <include file='docs.xml' path='docs/members/GateBulkCapacitance/*'/>
        public double Cgb { get; set; }

        /// <summary>
        /// Updates the charges and capacitances..
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="vgs">The gate-source voltage.</param>
        /// <param name="vds">The drain-source voltage.</param>
        /// <param name="vbs">The bulk-source voltage.</param>
        /// <param name="von">The threshold voltage.</param>
        /// <param name="vdsat">The saturation voltage.</param>
        /// <param name="mp">The model parameters.</param>
        /// <param name="tp">The temperature-dependent properties.</param>
        public void Update(double mode, double vgs, double vds, double vbs, double von, double vdsat,
            ModelParameters mp,
            TemperatureProperties tp)
        {
            double vbd = vbs - vds;
            double vgd = vgs - vds;
            double arg, sarg, sargsw;

            /*
             * now we do the hard part of the bulk-drain and bulk-source
             * diode - we evaluate the non-linear capacitance and
             * charge
             *
             * the basic equations are not hard, but the implementation
             * is somewhat long in an attempt to avoid log/exponential
             * evaluations
             */
            /*
             *  charge storage elements
             *
             *.. bulk-drain and bulk-source depletion capacitances
             */

            // Can't bypass the diode capacitance calculations
            if (!tp.Cbs.Equals(0.0) || !tp.CbsSidewall.Equals(0.0))
            {
                if (vbs < tp.TempDepCap)
                {
                    arg = 1 - vbs / tp.TempBulkPotential;

                    /*
                     * the following block looks somewhat long and messy,
                     * but since most users use the default grading
                     * coefficients of .5, and sqrt is MUCH faster than an
                     * Math.Exp(Math.Log()) we use this special case code to buy time.
                     * (as much as 10% of total job time!)
                     */
                    if (mp.BulkJunctionBotGradingCoefficient.Equals(mp.BulkJunctionSideGradingCoefficient))
                    {
                        if (mp.BulkJunctionBotGradingCoefficient.Equals(0.5))
                            sarg = sargsw = 1 / Math.Sqrt(arg);
                        else
                        {
                            sarg = sargsw =
                                    Math.Exp(-mp.BulkJunctionBotGradingCoefficient *
                                    Math.Log(arg));
                        }
                    }
                    else
                    {
                        if (mp.BulkJunctionBotGradingCoefficient.Equals(0.5))
                        {
                            sarg = 1 / Math.Sqrt(arg);
                        }
                        else
                        {
                            sarg = Math.Exp(-mp.BulkJunctionBotGradingCoefficient *
                                   Math.Log(arg));
                        }
                        if (mp.BulkJunctionSideGradingCoefficient.Equals(0.5))
                        {
                            sargsw = 1 / Math.Sqrt(arg);
                        }
                        else
                        {
                            sargsw = Math.Exp(-mp.BulkJunctionSideGradingCoefficient *
                                    Math.Log(arg));
                        }
                    }
                    Qbs = tp.TempBulkPotential * (tp.Cbs *
                            (1 - arg * sarg) / (1 - mp.BulkJunctionBotGradingCoefficient)
                            + tp.CbsSidewall *
                            (1 - arg * sargsw) /
                            (1 - mp.BulkJunctionSideGradingCoefficient));
                    Cbs = tp.Cbs * sarg + tp.CbsSidewall * sargsw;
                }
                else
                {
                    Qbs = tp.F4s + vbs * (tp.F2s + vbs * (tp.F3s / 2));
                    Cbs = tp.F2s + tp.F3s * vbs;
                }
            }
            else
            {
                Qbs = 0;
                Cbs = 0;
            }

            // can't bypass the diode capacitance calculations
            if (!tp.Cbd.Equals(0.0) || !tp.CbdSidewall.Equals(0.0))
            {
                if (vbd < tp.TempDepCap)
                {
                    arg = 1 - vbd / tp.TempBulkPotential;

                    /*
                     * the following block looks somewhat long and messy,
                     * but since most users use the default grading
                     * coefficients of .5, and sqrt is MUCH faster than an
                     * Math.Exp(Math.Log()) we use this special case code to buy time.
                     * (as much as 10% of total job time!)
                     */
                    if (mp.BulkJunctionBotGradingCoefficient.Equals(0.5) &&
                            mp.BulkJunctionSideGradingCoefficient.Equals(0.5))
                    {
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        if (mp.BulkJunctionBotGradingCoefficient == .5)
                            sarg = 1 / Math.Sqrt(arg);
                        else
                        {
                            sarg = Math.Exp(-mp.BulkJunctionBotGradingCoefficient *
                                   Math.Log(arg));
                        }
                        if (mp.BulkJunctionSideGradingCoefficient == .5)
                            sargsw = 1 / Math.Sqrt(arg);
                        else
                        {
                            sargsw = Math.Exp(-mp.BulkJunctionSideGradingCoefficient *
                                    Math.Log(arg));
                        }
                    }
                    Qbd = tp.TempBulkPotential * (tp.Cbd *
                            (1 - arg * sarg)
                            / (1 - mp.BulkJunctionBotGradingCoefficient)
                            + tp.CbdSidewall *
                            (1 - arg * sargsw)
                            / (1 - mp.BulkJunctionSideGradingCoefficient));
                    Cbd = tp.Cbd * sarg + tp.CbdSidewall * sargsw;
                }
                else
                {
                    Qbd = tp.F4d + vbd * (tp.F2d + vbd * tp.F3d / 2);
                    Cbd = tp.F2d + vbd * tp.F3d;
                }
            }
            else
            {
                Qbd = 0;
                Cbd = 0;
            }

            // Calculate Meyer capacitances
            double cgs, cgd, cgb;
            if (mode > 0)
                Transistor.MeyerCharges(vgs, vgd, von, vdsat, out cgs, out cgd, out cgb, tp.TempPhi, tp.OxideCap);
            else
                Transistor.MeyerCharges(vgd, vgs, von, vdsat, out cgd, out cgs, out cgb, tp.TempPhi, tp.OxideCap);
            Cgs = cgs;
            Cgd = cgd;
            Cgb = cgb;
        }
    }
}
