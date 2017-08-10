using SpiceSharp.Simulations;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read all options
    /// </summary>
    public class OptionReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OptionReader() : base(StatementType.Control) { }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Statement st, Netlist netlist)
        {
            if (!st.Name.TryReadLiteral("options"))
                return false;

            // Read all options
            for (int i = 0; i < st.Parameters.Count; i++)
            {
                if (st.Parameters[i].TryReadAssignment(out string pname, out string pvalue))
                {
                    if (pvalue.TryReadValue(out pvalue))
                    {
                        double v = (double)Parameters.SpiceMember.ConvertType(this, pvalue, typeof(double));
                        switch (pname)
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
                        switch (pname)
                        {
                            case "method":
                                switch (pvalue)
                                {
                                    case "trap":
                                    case "trapezoidal":
                                        Transient.Default.Method = new IntegrationMethods.Trapezoidal();
                                        break;
                                    default:
                                        throw new ParseException(st.Parameters[i], $"Invalid integration method {pvalue}");
                                }
                                break;
                        }
                    }
                }
                else if (st.Parameters[i].TryReadWord(out pname))
                {
                    switch (pname)
                    {
                        case "keepopinfo": AC.Default.KeepOpInfo = true; break;
                    }
                }
                else
                    throw new ParseException(st.Parameters[i], $"Unrecognized option " + st.Parameters[i].Image());
            }

            return true;
        }
    }
}
