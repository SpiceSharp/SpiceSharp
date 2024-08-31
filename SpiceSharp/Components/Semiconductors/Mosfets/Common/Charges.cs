using System;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// The charges on transistors.
    /// </summary>
    /// <seealso cref="MosfetCharges" />
    public class Charges : MosfetCharges
    {
        /// <summary>
        /// Calculates the charges and capacitances for the specified voltages.
        /// </summary>
        /// <param name="behavior">The biasing behavior.</param>
        /// <param name="mp">The model parameters.</param>
        public void Calculate(IMosfetBiasingBehavior behavior, ModelParameters mp)
        {
            var tp = behavior.Properties;
            double vgs = behavior.Vgs;
            double vds = behavior.Vds;
            double vbs = behavior.Vbs;

            /*
             * Now we do the hard part of the bulk-drain and bulk-source
             * diode - we evaluate the non-linear capacitance and
             * charge.
             *
             * The basic equations are not hard, but the implementation
             * is somewhat long in an attempt to avoid log/exponential
             * evaluations.
             */

            // Bulk-source depletion capacitances
            {
                /* can't bypass the diode capacitance calculations */
                if (tp.Cbs != 0 || tp.CbsSidewall != 0)
                {
                    if (vbs < tp.TempDepCap)
                    {
                        double arg = 1 - vbs / tp.TempBulkPotential;
                        double sarg, sargsw;
                        /*
                         * the following block looks somewhat long and messy,
                         * but since most users use the default grading
                         * coefficients of .5, and sqrt is MUCH faster than an
                         * Math.Exp(Math.Log()) we use this special case code to buy time.
                         * (as much as 10% of total job time!)
                         */
                        if (mp.BulkJunctionBotGradingCoefficient == mp.BulkJunctionSideGradingCoefficient)
                        {
                            if (mp.BulkJunctionBotGradingCoefficient == .5)
                                sarg = sargsw = 1 / Math.Sqrt(arg);
                            else
                                sarg = sargsw = Math.Exp(-mp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                        }
                        else
                        {
                            if (mp.BulkJunctionBotGradingCoefficient == .5)
                                sarg = 1 / Math.Sqrt(arg);
                            else
                                sarg = Math.Exp(-mp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                            if (mp.BulkJunctionSideGradingCoefficient == .5)
                                sargsw = 1 / Math.Sqrt(arg);
                            else
                                sargsw = Math.Exp(-mp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
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
            }

            // Bulk-drain depletion capacitances
            {
                double vbd = vbs - vds;
                if (tp.Cbd != 0 || tp.CbdSidewall != 0)
                {
                    if (vbd < tp.TempDepCap)
                    {
                        double arg = 1 - vbd / tp.TempBulkPotential;
                        double sarg, sargsw;
                        /*
                         * the following block looks somewhat long and messy,
                         * but since most users use the default grading
                         * coefficients of .5, and sqrt is MUCH faster than an
                         * Math.Exp(Math.Log()) we use this special case code to buy time.
                         * (as much as 10% of total job time!)
                         */
                        if (mp.BulkJunctionBotGradingCoefficient == .5 && mp.BulkJunctionSideGradingCoefficient == .5)
                            sarg = sargsw = 1 / Math.Sqrt(arg);
                        else
                        {
                            if (mp.BulkJunctionBotGradingCoefficient == .5)
                                sarg = 1 / Math.Sqrt(arg);
                            else
                                sarg = Math.Exp(-mp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                            if (mp.BulkJunctionSideGradingCoefficient == .5)
                                sargsw = 1 / Math.Sqrt(arg);
                            else
                                sargsw = Math.Exp(-mp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                        }
                        Qbd = tp.TempBulkPotential * (tp.Cbd *
                                (1 - arg * sarg)
                                / (1 - mp.BulkJunctionBotGradingCoefficient)
                                + tp.CbdSidewall *
                                (1 - arg * sargsw)
                                / (1 - mp.BulkJunctionSideGradingCoefficient));
                        Cbd = tp.Cbd * sarg +
                                tp.CbdSidewall * sargsw;
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
            }

            // Meyer capacitance calculation
            {
                /*
                 * new cmeyer - this just evaluates at the current time,
                 * expects you to remember values from previous time
                 * returns 1/2 of non-constant portion of capacitance
                 * you must add in the other half from previous time
                 * and the constant part
                 */
                double cgs, cgd, cgb;
                if (behavior.Mode > 0)
                    Transistor.MeyerCharges(vgs, vgs - vds, mp.MosfetType * behavior.Von, mp.MosfetType * behavior.Vdsat, out cgs, out cgd, out cgb, tp.TempPhi, tp.OxideCap);
                else
                    Transistor.MeyerCharges(vgs - vds, vgs, mp.MosfetType * behavior.Von, mp.MosfetType * behavior.Vdsat, out cgd, out cgs, out cgb, tp.TempPhi, tp.OxideCap);
                Cgs = cgs;
                Cgd = cgd;
                Cgb = cgb;
            }
        }
    }
}
