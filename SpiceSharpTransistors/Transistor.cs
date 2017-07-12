using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Components.Transistors
{
    public static class Transistor
    {
        /// <summary>
        /// Limiting function FET
        /// </summary>
        /// <param name="vnew">New voltage</param>
        /// <param name="vold">Olt voltage</param>
        /// <param name="vto">Threshold</param>
        /// <returns></returns>
        public static double DEVfetlim(double vnew, double vold, double vto)
        {
            double vtsthi;
            double vtstlo;
            double vtox;
            double delv;
            double vtemp;

            vtsthi = Math.Abs(2 * (vold - vto)) + 2;
            vtstlo = vtsthi / 2 + 2;
            vtox = vto + 3.5;
            delv = vnew - vold;

            if (vold >= vto)
            {
                if (vold >= vtox)
                {
                    if (delv <= 0)
                    {
                        /* going off */
                        if (vnew >= vtox)
                        {
                            if (-delv > vtstlo)
                            {
                                vnew = vold - vtstlo;
                            }
                        }
                        else
                        {
                            vnew = Math.Max(vnew, vto + 2);
                        }
                    }
                    else
                    {
                        /* staying on */
                        if (delv >= vtsthi)
                        {
                            vnew = vold + vtsthi;
                        }
                    }
                }
                else
                {
                    /* middle region */
                    if (delv <= 0)
                    {
                        /* decreasing */
                        vnew = Math.Max(vnew, vto - .5);
                    }
                    else
                    {
                        /* increasing */
                        vnew = Math.Min(vnew, vto + 4);
                    }
                }
            }
            else
            {
                /* off */
                if (delv <= 0)
                {
                    if (-delv > vtsthi)
                    {
                        vnew = vold - vtsthi;
                    }
                }
                else
                {
                    vtemp = vto + .5;
                    if (vnew <= vtemp)
                    {
                        if (delv > vtstlo)
                        {
                            vnew = vold + vtstlo;
                        }
                    }
                    else
                    {
                        vnew = vtemp;
                    }
                }
            }
            return (vnew);
        }

        /// <summary>
        /// Limiting function PN
        /// </summary>
        /// <param name="vnew">New voltage</param>
        /// <param name="vold">Old voltage</param>
        /// <param name="vt">Threshold</param>
        /// <param name="vcrit">Critical</param>
        /// <param name="check">Checking variable</param>
        /// <returns></returns>
        public static double DEVpnjlim(double vnew, double vold, double vt, double vcrit, ref int icheck)
        {
            double arg;

            if ((vnew > vcrit) && (Math.Abs(vnew - vold) > (vt + vt)))
            {
                if (vold > 0)
                {
                    arg = 1 + (vnew - vold) / vt;
                    if (arg > 0)
                    {
                        vnew = vold + vt * Math.Log(arg);
                    }
                    else
                    {
                        vnew = vcrit;
                    }
                }
                else
                {
                    vnew = vt * Math.Log(vnew / vt);
                }
                icheck = 1;
            }
            else
            {
                icheck = 0;
            }
            return (vnew);
        }

        /// <summary>
        /// Limiting function VDS
        /// </summary>
        /// <param name="vnew">New voltage</param>
        /// <param name="vold">Old voltage</param>
        /// <returns></returns>
        public static double DEVlimvds(double vnew, double vold)
        {
            if (vold >= 3.5)
            {
                if (vnew > vold)
                {
                    vnew = Math.Min(vnew, (3 * vold) + 2);
                }
                else
                {
                    if (vnew < 3.5)
                    {
                        vnew = Math.Max(vnew, 2);
                    }
                }
            }
            else
            {
                if (vnew > vold)
                {
                    vnew = Math.Min(vnew, 4);
                }
                else
                {
                    vnew = Math.Max(vnew, -.5);
                }
            }
            return (vnew);
        }

        /// <summary>
        /// QMeyer method for calculating capacitances
        /// </summary>
        /// <param name="vgs">Gate-source voltage</param>
        /// <param name="vgd">Gate-drain voltage</param>
        /// <param name="vgb">Gate-bulk voltage</param>
        /// <param name="von">Von</param>
        /// <param name="vdsat">Vdsat</param>
        /// <param name="capgs">Gate-source capacitance</param>
        /// <param name="capgd">Gate-drain capacitance</param>
        /// <param name="capgb">Gate-bulk capacitance</param>
        /// <param name="phi">Phi</param>
        /// <param name="cox">Cox</param>
        public static void DEVqmeyer(double vgs, double vgd, double vgb, double von, double vdsat, out double capgs, out double capgd, out double capgb, double phi, double cox)
        {
            double vds;
            double vddif;
            double vddif1;
            double vddif2;
            double vgst;


            vgst = vgs - von;
            if (vgst <= -phi)
            {
                capgb = cox / 2;
                capgs = 0;
                capgd = 0;
            }
            else if (vgst <= -phi / 2)
            {
                capgb = -vgst * cox / (2 * phi);
                capgs = 0;
                capgd = 0;
            }
            else if (vgst <= 0)
            {
                capgb = -vgst * cox / (2 * phi);
                capgs = vgst * cox / (1.5 * phi) + cox / 3;
                capgd = 0;
            }
            else
            {
                vds = vgs - vgd;
                if (vdsat <= vds)
                {
                    capgs = cox / 3;
                    capgd = 0;
                    capgb = 0;
                }
                else
                {
                    vddif = 2.0 * vdsat - vds;
                    vddif1 = vdsat - vds/*-1.0e-12*/;
                    vddif2 = vddif * vddif;
                    capgd = cox * (1.0 - vdsat * vdsat / vddif2) / 3;
                    capgs = cox * (1.0 - vddif1 * vddif1 / vddif2) / 3;
                    capgb = 0;
                }
            }
        }
    }
}
