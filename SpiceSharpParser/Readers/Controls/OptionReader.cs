using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads options (.OPTIONS)
    /// </summary>
    public class OptionReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OptionReader() : base(StatementType.Control)
        {
            Identifier = "options";
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            // Read all options
            for (int i = 0; i < st.Parameters.Count; i++)
            {
                switch (st.Parameters[i].kind)
                {
                    case ASSIGNMENT:
                        AssignmentToken at = st.Parameters[i] as AssignmentToken;
                        string key = at.Name.image.ToLower();
                        switch (key)
                        {
                            case "abstol": SimulationConfiguration.Default.AbsTol = netlist.ParseDouble(at.Value); break;
                            case "reltol": SimulationConfiguration.Default.RelTol = netlist.ParseDouble(at.Value); break;
                            case "gmin": SimulationConfiguration.Default.Gmin = netlist.ParseDouble(at.Value); break;
                            case "itl1": SimulationConfiguration.Default.DcMaxIterations = (int)Math.Round(netlist.ParseDouble(at.Value)); break;
                            case "itl2": SimulationConfiguration.Default.SweepMaxIterations = (int)Math.Round(netlist.ParseDouble(at.Value)); break;
                            case "itl4": SimulationConfiguration.Default.TranMaxIterations = (int)Math.Round(netlist.ParseDouble(at.Value)); break;
                            case "temp": netlist.Circuit.State.Temperature = netlist.ParseDouble(at.Value) + Circuit.CONSTCtoK; break;
                            case "tnom": netlist.Circuit.State.NominalTemperature = netlist.ParseDouble(at.Value) + Circuit.CONSTCtoK; break;
                            case "method":
                                switch (at.Value.image.ToLower())
                                {
                                    case "trap":
                                    case "trapezoidal":
                                        SimulationConfiguration.Default.Method = new IntegrationMethods.Trapezoidal();
                                        break;
                                }
                                break;

                            default:
                                throw new ParseException(st.Parameters[i], "Unrecognized option");
                        }
                        break;

                    case WORD:
                        key = st.Parameters[i].image.ToLower();
                        switch (key)
                        {
                            case "keepopinfo": SimulationConfiguration.Default.KeepOpInfo = true; break;
                            default:
                                throw new ParseException(st.Parameters[i], "Unrecognized option");
                        }
                        break;

                    default:
                        throw new ParseException(st.Parameters[i], "Unrecognized option");
                }
            }

            return true;
        }
    }
}
