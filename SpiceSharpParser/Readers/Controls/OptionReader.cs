using SpiceSharp.Simulations;
using SpiceSharp.Parser.Readers.Extensions;
using SpiceSharp.Parser.Readers.Simulations;

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
        public override bool Read(Statement st, Netlist netlist)
        {
            // Read all options
            for (int i = 0; i < st.Parameters.Count; i++)
            {
                switch (st.Parameters[i].kind)
                {
                    case TokenConstants.ASSIGNMENT:
                        AssignmentToken at = st.Parameters[i] as AssignmentToken;
                        string key = at.Name.image.ToLower();
                        switch (key)
                        {
                            case "abstol":
                            case "reltol":
                            case "gmin":
                            case "itl1":
                            case "itl2":
                            case "itl4":
                            case "temp":
                            case "tnom":
                                double v = netlist.ParseDouble(at.Value);
                                Configuration.Defaults[at.Name.image.ToLower()] = v;
                                break;

                            case "method":
                                string s = at.Value.image.ToLower();
                                Configuration.StringDefaults[key] = s;
                                break;

                            default:
                                throw new ParseException(st.Parameters[i], "Unrecognized option");
                        }
                        break;

                    case SpiceSharpParserConstants.WORD:
                        key = st.Parameters[i].image.ToLower();
                        switch (key)
                        {
                            case "keepopinfo":
                                Configuration.FlagDefaults.Add(key);
                                break;

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
