using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read all options
    /// </summary>
    public class OptionReader : Reader
    {
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (!name.TryReadLiteral("options"))
                return false;

            // Read all options
            for (int i = 0; i < parameters.Count; i++)
            {
                string pname, pvalue;
                if (parameters[i].TryReadAssignment(out pname, out pvalue))
                {
                    if (pvalue.TryReadValue(out pvalue))
                    {
                        double v = (double)Parameters.SpiceMember.ConvertType(this, pvalue, typeof(double));
                        switch (pname.ToLower())
                        {
                            case "abstol":
                                DC.Default.AbsTol = v;
                                AC.Default.AbsTol = v;
                                Transient.Default.AbsTol = v;
                                break;
                            case "gmin":
                                DC.Default.Gmin = v;
                                AC.Default.Gmin = v;
                                Transient.Default.Gmin = v;
                                break;
                            case "itl1":
                                AC.Default.DcMaxIterations = (int)v;
                                Transient.Default.DcMaxIterations = (int)v;
                                break;
                            case "itl2":
                                DC.Default.MaxIterations = (int)v;
                                break;
                            case "itl4":
                                Transient.Default.TranMaxIterations = (int)v;
                                break;
                            case "reltol":
                                DC.Default.RelTol = v;
                                AC.Default.RelTol = v;
                                Transient.Default.RelTol = v;
                                break;
                            case "temp":
                                netlist.Circuit.State.Temperature = v + Circuit.CONSTCtoK;
                                break;
                            case "tnom":
                                netlist.Circuit.State.NominalTemperature = v + Circuit.CONSTCtoK;
                                break;
                            case "vntol":
                                DC.Default.VoltTol = v;
                                AC.Default.VoltTol = v;
                                Transient.Default.VoltTol = v;
                                break;
                            default:
                                Diagnostics.CircuitWarning.Warning(this, $"Unrecognized option {pname}");
                                break;
                        }
                    }
                    else if (pvalue.TryReadWord(out pvalue))
                    {
                        switch (pname.ToLower())
                        {
                            case "method":
                                switch (pvalue.ToLower())
                                {
                                    case "trap":
                                    case "trapezoidal":
                                        Transient.Default.Method = new IntegrationMethods.Trapezoidal();
                                        break;
                                    default:
                                        throw new ParseException(parameters[i], $"Invalid integration method {pvalue}");
                                }
                                break;
                        }
                    }
                }
                else if (parameters[i].TryReadWord(out pname))
                {
                    switch (pname.ToLower())
                    {
                        case "keepopinfo": AC.Default.KeepOpInfo = true; break;
                    }
                }
                else
                    throw new ParseException(parameters[i], $"Unrecognized option " + parameters[i].Image());
            }

            return true;
        }
    }
}
