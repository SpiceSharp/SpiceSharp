using System;
using System.IO;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.Transistors
{
    internal static class BSIM3v24Helpers
    {
        /// <summary>
        /// BSIM3 check model
        /// </summary>
        /// <returns></returns>
        public static bool BSIM3checkModel(this BSIM3v24 bsim3)
        {
            var model = bsim3.Model as BSIM3v24Model;
            bool Fatal_Flag = false;

            using (StreamWriter sw = new StreamWriter("b3v3check.log", true))
            {
                sw.WriteLine("BSIM3v3.2.4 Parameter Checking.");
                if (model.BSIM3version != "3.2.4")
                {
                    sw.WriteLine("Warning: This model is BSIM3v3.2.4; you specified a wrong version number.");
                    CircuitWarning.Warning(bsim3, "Warning: This model is BSIM3v3.2.4; you specified a wrong version number.");
                }

                sw.WriteLine($"Model = {bsim3.Name}");
                if (bsim3.pParam.BSIM3nlx < -bsim3.pParam.BSIM3leff)
                {
                    sw.WriteLine($"Fatal: Nlx = {bsim3.pParam.BSIM3nlx} is less than -Leff.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Nlx = {bsim3.pParam.BSIM3nlx} is less than -Leff.");
                    Fatal_Flag = true;
                }

                if (model.BSIM3tox <= 0.0)
                {
                    sw.WriteLine($"Fatal: Tox = {model.BSIM3tox} is not positive.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Tox = {model.BSIM3tox} is not positive.");
                    Fatal_Flag = true;
                }

                if (model.BSIM3toxm <= 0.0)
                {
                    sw.WriteLine($"Fatal: Toxm = {model.BSIM3toxm} is not positive.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Toxm = {model.BSIM3toxm} is not positive.");
                    Fatal_Flag = true;
                }

                if (bsim3.pParam.BSIM3npeak <= 0.0)
                {
                    sw.WriteLine($"Fatal: Nch = {bsim3.pParam.BSIM3npeak} is not positive.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Nch = {bsim3.pParam.BSIM3npeak} is not positive.");
                    Fatal_Flag = true;
                }
                if (bsim3.pParam.BSIM3nsub <= 0.0)
                {
                    sw.WriteLine($"Fatal: Nsub = {bsim3.pParam.BSIM3nsub} is not positive.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Nsub = {bsim3.pParam.BSIM3nsub} is not positive.");
                    Fatal_Flag = true;
                }
                if (bsim3.pParam.BSIM3ngate < 0.0)
                {
                    sw.WriteLine($"Fatal: Ngate = {bsim3.pParam.BSIM3ngate} is not positive.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Ngate = {bsim3.pParam.BSIM3ngate} Ngate is not positive.");
                    Fatal_Flag = true;
                }
                if (bsim3.pParam.BSIM3ngate > 1.0e25)
                {
                    sw.WriteLine($"Fatal: Ngate = {bsim3.pParam.BSIM3ngate} is too high.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Ngate = {bsim3.pParam.BSIM3ngate} Ngate is too high");
                    Fatal_Flag = true;
                }
                if (bsim3.pParam.BSIM3xj <= 0.0)
                {
                    sw.WriteLine($"Fatal: Xj = {bsim3.pParam.BSIM3xj} is not positive.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Xj = {bsim3.pParam.BSIM3xj} is not positive.");
                    Fatal_Flag = true;
                }

                if (bsim3.pParam.BSIM3dvt1 < 0.0)
                {
                    sw.WriteLine($"Fatal: Dvt1 = {bsim3.pParam.BSIM3dvt1} is negative.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Dvt1 = {bsim3.pParam.BSIM3dvt1} is negative.");
                    Fatal_Flag = true;
                }

                if (bsim3.pParam.BSIM3dvt1w < 0.0)
                {
                    sw.WriteLine($"Fatal: Dvt1w = {bsim3.pParam.BSIM3dvt1w} is negative.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Dvt1w = {bsim3.pParam.BSIM3dvt1w} is negative.");
                    Fatal_Flag = true;
                }

                if (bsim3.pParam.BSIM3w0 == -bsim3.pParam.BSIM3weff)
                {
                    sw.WriteLine("Fatal: (W0 + Weff) = 0 causing divided-by-zero.");

                    CircuitWarning.Warning(bsim3, "Fatal: (W0 + Weff) = 0 causing divided-by-zero.");
                    Fatal_Flag = true;
                }

                if (bsim3.pParam.BSIM3dsub < 0.0)
                {
                    sw.WriteLine($"Fatal: Dsub = {bsim3.pParam.BSIM3dsub} is negative.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Dsub = {bsim3.pParam.BSIM3dsub} is negative.");
                    Fatal_Flag = true;
                }
                if (bsim3.pParam.BSIM3b1 == -bsim3.pParam.BSIM3weff)
                {
                    sw.WriteLine("Fatal: (B1 + Weff) = 0 causing divided-by-zero.");
                    CircuitWarning.Warning(bsim3, "Fatal: (B1 + Weff) = 0 causing divided-by-zero.");
                    Fatal_Flag = true;
                }
                if (bsim3.pParam.BSIM3u0temp <= 0.0)
                {
                    sw.WriteLine($"Fatal: u0 at current temperature = {bsim3.pParam.BSIM3u0temp} is not positive.");
                    CircuitWarning.Warning(bsim3, $"Fatal: u0 at current temperature = {bsim3.pParam.BSIM3u0temp} is not positive.");
                    Fatal_Flag = true;
                }

                /* Check delta parameter */
                if (bsim3.pParam.BSIM3delta < 0.0)
                {
                    sw.WriteLine($"Fatal: Delta = {bsim3.pParam.BSIM3delta} is less than zero.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Delta = {bsim3.pParam.BSIM3delta} is less than zero.");
                    Fatal_Flag = true;
                }

                if (bsim3.pParam.BSIM3vsattemp <= 0.0)
                {
                    sw.WriteLine($"Fatal: Vsat at current temperature = {bsim3.pParam.BSIM3vsattemp} is not positive.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Vsat at current temperature = {bsim3.pParam.BSIM3vsattemp} is not positive.");
                    Fatal_Flag = true;
                }
                /* Check Rout parameters */
                if (bsim3.pParam.BSIM3pclm <= 0.0)
                {
                    sw.WriteLine($"Fatal: Pclm = {bsim3.pParam.BSIM3pclm} is not positive.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Pclm = {bsim3.pParam.BSIM3pclm} is not positive.");
                    Fatal_Flag = true;
                }

                if (bsim3.pParam.BSIM3drout < 0.0)
                {
                    sw.WriteLine($"Fatal: Drout = {bsim3.pParam.BSIM3drout} is negative.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Drout = {bsim3.pParam.BSIM3drout} is negative.");
                    Fatal_Flag = true;
                }

                if (bsim3.pParam.BSIM3pscbe2 <= 0.0)
                {
                    sw.WriteLine($"Warning: Pscbe2 = {bsim3.pParam.BSIM3pscbe2} is not positive.");
                    CircuitWarning.Warning(bsim3, $"Warning: Pscbe2 = {bsim3.pParam.BSIM3pscbe2} is not positive.");
                }

                if (model.BSIM3unitLengthSidewallJctCap > 0.0 || model.BSIM3unitLengthGateSidewallJctCap > 0.0)
                {
                    if (bsim3.BSIM3drainPerimeter < bsim3.pParam.BSIM3weff)
                    {
                        sw.WriteLine($"Warning: Pd = {bsim3.BSIM3drainPerimeter} is less than W.");
                        CircuitWarning.Warning(bsim3, $"Warning: Pd = {bsim3.BSIM3drainPerimeter} is less than W.");
                    }
                    if (bsim3.BSIM3sourcePerimeter < bsim3.pParam.BSIM3weff)
                    {
                        sw.WriteLine($"Warning: Ps = {bsim3.BSIM3sourcePerimeter} is less than W.");
                        CircuitWarning.Warning(bsim3, $"Warning: Ps = {bsim3.BSIM3sourcePerimeter} is less than W.");
                    }
                }

                if (bsim3.pParam.BSIM3noff < 0.1)
                {
                    sw.WriteLine($"Warning: Noff = {bsim3.pParam.BSIM3noff} is too small.");
                    CircuitWarning.Warning(bsim3, $"Warning: Noff = {bsim3.pParam.BSIM3noff} is too small.");
                }
                if (bsim3.pParam.BSIM3noff > 4.0)
                {
                    sw.WriteLine($"Warning: Noff = {bsim3.pParam.BSIM3noff} is too large.");
                    CircuitWarning.Warning(bsim3, $"Warning: Noff = {bsim3.pParam.BSIM3noff} is too large.");
                }

                if (bsim3.pParam.BSIM3voffcv < -0.5)
                {
                    sw.WriteLine($"Warning: Voffcv = {bsim3.pParam.BSIM3voffcv} is too small.");
                    CircuitWarning.Warning(bsim3, $"Warning: Voffcv = {bsim3.pParam.BSIM3voffcv} is too small.");
                }
                if (bsim3.pParam.BSIM3voffcv > 0.5)
                {
                    sw.WriteLine($"Warning: Voffcv = {bsim3.pParam.BSIM3voffcv} is too large.");
                    CircuitWarning.Warning(bsim3, $"Warning: Voffcv = {bsim3.pParam.BSIM3voffcv} is too large.");
                }

                if (model.BSIM3ijth < 0.0)
                {
                    sw.WriteLine($"Fatal: Ijth = {model.BSIM3ijth} cannot be negative.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Ijth = {model.BSIM3ijth} cannot be negative.");
                    Fatal_Flag = true;
                }

                /* Check capacitance parameters */
                if (bsim3.pParam.BSIM3clc < 0.0)
                {
                    sw.WriteLine($"Fatal: Clc = {bsim3.pParam.BSIM3clc} is negative.");
                    CircuitWarning.Warning(bsim3, $"Fatal: Clc = {bsim3.pParam.BSIM3clc} is negative.");
                    Fatal_Flag = true;
                }

                if (bsim3.pParam.BSIM3moin < 5.0)
                {
                    sw.WriteLine($"Warning: Moin = {bsim3.pParam.BSIM3moin} is too small.");
                    CircuitWarning.Warning(bsim3, $"Warning: Moin = {bsim3.pParam.BSIM3moin} is too small.");
                }
                if (bsim3.pParam.BSIM3moin > 25.0)
                {
                    sw.WriteLine($"Warning: Moin = {bsim3.pParam.BSIM3moin} is too large.");
                    CircuitWarning.Warning(bsim3, $"Warning: Moin = {bsim3.pParam.BSIM3moin} is too large.");
                }

                if (model.BSIM3capMod == 3)
                {
                    if (bsim3.pParam.BSIM3acde < 0.4)
                    {
                        sw.WriteLine($"Warning:  Acde = {bsim3.pParam.BSIM3acde} is too small.");
                        CircuitWarning.Warning(bsim3, $"Warning: Acde = {bsim3.pParam.BSIM3acde} is too small.");
                    }
                    if (bsim3.pParam.BSIM3acde > 1.6)
                    {
                        sw.WriteLine($"Warning:  Acde = {bsim3.pParam.BSIM3acde} is too large.");
                        CircuitWarning.Warning(bsim3, $"Warning: Acde = {bsim3.pParam.BSIM3acde} is too large.");
                    }
                }

                if (model.BSIM3paramChk == 1)
                {
                    /* Check L and W parameters */
                    if (bsim3.pParam.BSIM3leff <= 5.0e-8)
                    {
                        sw.WriteLine($"Warning: Leff = {bsim3.pParam.BSIM3leff} may be too small.");
                        CircuitWarning.Warning(bsim3, $"Warning: Leff = {bsim3.pParam.BSIM3leff} may be too small.");
                    }

                    if (bsim3.pParam.BSIM3leffCV <= 5.0e-8)
                    {
                        sw.WriteLine($"Warning: Leff for CV = {bsim3.pParam.BSIM3leffCV} may be too small.");
                        CircuitWarning.Warning(bsim3, $"Warning: Leff for CV = {bsim3.pParam.BSIM3leffCV} may be too small.");
                    }

                    if (bsim3.pParam.BSIM3weff <= 1.0e-7)
                    {
                        sw.WriteLine($"Warning: Weff = {bsim3.pParam.BSIM3weff} may be too small.");
                        CircuitWarning.Warning(bsim3, $"Warning: Weff = {bsim3.pParam.BSIM3weff} may be too small.");
                    }

                    if (bsim3.pParam.BSIM3weffCV <= 1.0e-7)
                    {
                        sw.WriteLine($"Warning: Weff for CV = {bsim3.pParam.BSIM3weffCV} may be too small.");
                        CircuitWarning.Warning(bsim3, $"Warning: Weff for CV = {bsim3.pParam.BSIM3weffCV} may be too small.");
                    }

                    /* Check threshold voltage parameters */
                    if (bsim3.pParam.BSIM3nlx < 0.0)
                    {
                        sw.WriteLine($"Warning: Nlx = {bsim3.pParam.BSIM3nlx} is negative.");
                        CircuitWarning.Warning(bsim3, $"Warning: Nlx = {bsim3.pParam.BSIM3nlx} is negative.");
                    }
                    if (model.BSIM3tox < 1.0e-9)
                    {
                        sw.WriteLine($"Warning: Tox = {model.BSIM3tox} is less than 10A.");
                        CircuitWarning.Warning(bsim3, $"Warning: Tox = {model.BSIM3tox} is less than 10A.");
                    }

                    if (bsim3.pParam.BSIM3npeak <= 1.0e15)
                    {
                        sw.WriteLine($"Warning: Nch = {bsim3.pParam.BSIM3npeak} may be too small.");
                        CircuitWarning.Warning(bsim3, $"Warning: Nch = {bsim3.pParam.BSIM3npeak} may be too small.");
                    }
                    else if (bsim3.pParam.BSIM3npeak >= 1.0e21)
                    {
                        sw.WriteLine($"Warning: Nch = {bsim3.pParam.BSIM3npeak} may be too large.");
                        CircuitWarning.Warning(bsim3, $"Warning: Nch = {bsim3.pParam.BSIM3npeak} may be too large.");
                    }

                    if (bsim3.pParam.BSIM3nsub <= 1.0e14)
                    {
                        sw.WriteLine($"Warning: Nsub = {bsim3.pParam.BSIM3nsub} may be too small.");
                        CircuitWarning.Warning(bsim3, $"Warning: Nsub = {bsim3.pParam.BSIM3nsub} may be too small.");
                    }
                    else if (bsim3.pParam.BSIM3nsub >= 1.0e21)
                    {
                        sw.WriteLine($"Warning: Nsub = {bsim3.pParam.BSIM3nsub} may be too large.");
                        CircuitWarning.Warning(bsim3, $"Warning: Nsub = {bsim3.pParam.BSIM3nsub} may be too large.");
                    }

                    if ((bsim3.pParam.BSIM3ngate > 0.0) &&
                        (bsim3.pParam.BSIM3ngate <= 1.0e18))
                    {
                        sw.WriteLine($"Warning: Ngate = {bsim3.pParam.BSIM3ngate} is less than 1.E18cm^-3.");
                        CircuitWarning.Warning(bsim3, $"Warning: Ngate = {bsim3.pParam.BSIM3ngate} is less than 1.E18cm^-3.");
                    }

                    if (bsim3.pParam.BSIM3dvt0 < 0.0)
                    {
                        sw.WriteLine($"Warning: Dvt0 = {bsim3.pParam.BSIM3dvt0} is negative.");
                        CircuitWarning.Warning(bsim3, $"Warning: Dvt0 = {bsim3.pParam.BSIM3dvt0} is negative.");
                    }

                    if (Math.Abs(1.0e-6 / (bsim3.pParam.BSIM3w0 + bsim3.pParam.BSIM3weff)) > 10.0)
                    {
                        sw.WriteLine("Warning: (W0 + Weff) may be too small.");
                        CircuitWarning.Warning(bsim3, "Warning: (W0 + Weff) may be too small.");
                    }

                    /* Check subthreshold parameters */
                    if (bsim3.pParam.BSIM3nfactor < 0.0)
                    {
                        sw.WriteLine($"Warning: Nfactor = {bsim3.pParam.BSIM3nfactor} is negative.");
                        CircuitWarning.Warning(bsim3, $"Warning: Nfactor = {bsim3.pParam.BSIM3nfactor} is negative.");
                    }
                    if (bsim3.pParam.BSIM3cdsc < 0.0)
                    {
                        sw.WriteLine($"Warning: Cdsc = {bsim3.pParam.BSIM3cdsc} is negative.");
                        CircuitWarning.Warning(bsim3, $"Warning: Cdsc = {bsim3.pParam.BSIM3cdsc} is negative.");
                    }
                    if (bsim3.pParam.BSIM3cdscd < 0.0)
                    {
                        sw.WriteLine($"Warning: Cdscd = {bsim3.pParam.BSIM3cdscd} is negative.");
                        CircuitWarning.Warning(bsim3, $"Warning: Cdscd = {bsim3.pParam.BSIM3cdscd} is negative.");
                    }
                    /* Check DIBL parameters */
                    if (bsim3.pParam.BSIM3eta0 < 0.0)
                    {
                        sw.WriteLine($"Warning: Eta0 = {bsim3.pParam.BSIM3eta0} is negative.");
                        CircuitWarning.Warning(bsim3, $"Warning: Eta0 = {bsim3.pParam.BSIM3eta0} is negative.");
                    }

                    /* Check Abulk parameters */
                    if (Math.Abs(1.0e-6 / (bsim3.pParam.BSIM3b1 + bsim3.pParam.BSIM3weff)) > 10.0)
                    {
                        sw.WriteLine("Warning: (B1 + Weff) may be too small.");
                        CircuitWarning.Warning(bsim3, "Warning: (B1 + Weff) may be too small.");
                    }

                    /* Check Saturation parameters */
                    if (bsim3.pParam.BSIM3a2 < 0.01)
                    {
                        sw.WriteLine($"Warning: A2 = {bsim3.pParam.BSIM3a2} is too small. Set to 0.01.");
                        CircuitWarning.Warning(bsim3, $"Warning: A2 = {bsim3.pParam.BSIM3a2} is too small. Set to 0.01.");
                        bsim3.pParam.BSIM3a2 = 0.01;
                    }
                    else if (bsim3.pParam.BSIM3a2 > 1.0)
                    {
                        sw.WriteLine($"Warning: A2 = {bsim3.pParam.BSIM3a2} is larger than 1. A2 is set to 1 and A1 is set to 0.");
                        CircuitWarning.Warning(bsim3, $"Warning: A2 = {bsim3.pParam.BSIM3a2} is larger than 1. A2 is set to 1 and A1 is set to 0.");
                        bsim3.pParam.BSIM3a2 = 1.0;
                        bsim3.pParam.BSIM3a1 = 0.0;
                    }

                    if (bsim3.pParam.BSIM3rdsw < 0.0)
                    {
                        sw.WriteLine($"Warning: Rdsw = {bsim3.pParam.BSIM3rdsw} is negative. Set to zero.");
                        CircuitWarning.Warning(bsim3, $"Warning: Rdsw = {bsim3.pParam.BSIM3rdsw} is negative. Set to zero.");
                        bsim3.pParam.BSIM3rdsw = 0.0;
                        bsim3.pParam.BSIM3rds0 = 0.0;
                    }
                    else if ((bsim3.pParam.BSIM3rds0 > 0.0) && (bsim3.pParam.BSIM3rds0 < 0.001))
                    {
                        sw.WriteLine($"Warning: Rds at current temperature = {bsim3.pParam.BSIM3rds0} is less than 0.001 ohm. Set to zero.");
                        CircuitWarning.Warning(bsim3, $"Warning: Rds at current temperature = {bsim3.pParam.BSIM3rds0} is less than 0.001 ohm. Set to zero.");
                        bsim3.pParam.BSIM3rds0 = 0.0;
                    }
                    if (bsim3.pParam.BSIM3vsattemp < 1.0e3)
                    {
                        sw.WriteLine($"Warning: Vsat at current temperature = {bsim3.pParam.BSIM3vsattemp} may be too small.");
                        CircuitWarning.Warning(bsim3, $"Warning: Vsat at current temperature = {bsim3.pParam.BSIM3vsattemp} may be too small.");
                    }

                    if (bsim3.pParam.BSIM3pdibl1 < 0.0)
                    {
                        sw.WriteLine($"Warning: Pdibl1 = {bsim3.pParam.BSIM3pdibl1} is negative.");
                        CircuitWarning.Warning(bsim3, $"Warning: Pdibl1 = {bsim3.pParam.BSIM3pdibl1} is negative.");
                    }
                    if (bsim3.pParam.BSIM3pdibl2 < 0.0)
                    {
                        sw.WriteLine($"Warning: Pdibl2 = {bsim3.pParam.BSIM3pdibl2} is negative.");
                        CircuitWarning.Warning(bsim3, $"Warning: Pdibl2 = {bsim3.pParam.BSIM3pdibl2} is negative.");
                    }
                    /* Check overlap capacitance parameters */
                    if (model.BSIM3cgdo < 0.0)
                    {
                        sw.WriteLine($"Warning: cgdo = {model.BSIM3cgdo} is negative. Set to zero.");
                        CircuitWarning.Warning(bsim3, $"Warning: cgdo = {model.BSIM3cgdo} is negative. Set to zero.");
                        model.BSIM3cgdo.Value = 0.0;
                    }
                    if (model.BSIM3cgso < 0.0)
                    {
                        sw.WriteLine($"Warning: cgso = {model.BSIM3cgso} is negative. Set to zero.");
                        CircuitWarning.Warning(bsim3, $"Warning: cgso = {model.BSIM3cgso} is negative. Set to zero.");
                        model.BSIM3cgso.Value = 0.0;
                    }
                    if (model.BSIM3cgbo < 0.0)
                    {
                        sw.WriteLine($"Warning: cgbo = {model.BSIM3cgbo} is negative. Set to zero.");
                        CircuitWarning.Warning(bsim3, $"Warning: cgbo = {model.BSIM3cgbo} is negative. Set to zero.");
                        model.BSIM3cgbo.Value = 0.0;
                    }

                }
            }

            return Fatal_Flag;
        }
    }
}
