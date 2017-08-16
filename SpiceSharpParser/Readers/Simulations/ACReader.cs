using SpiceSharp.Simulations;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read AC analysis
    /// </summary>
    public class ACReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ACReader() : base(StatementType.Control)
        {
            Identifier = "ac";
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="st">Statement</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            AC ac = new AC("AC " + (netlist.Simulations.Count + 1));
            switch (st.Parameters.Count)
            {
                case 0: throw new ParseException(st.Name, "LIN, DEC or OCT expected");
                case 1: throw new ParseException(st.Parameters[0], "Number of points expected");
                case 2: throw new ParseException(st.Parameters[1], "Starting frequency expected");
                case 3: throw new ParseException(st.Parameters[2], "Stopping frequency expected");
            }

            // Standard st.Parameters
            string t = st.Parameters[0].image.ToLower();
            switch (t)
            {
                case "lin": ac.StepType = AC.StepTypes.Linear; break;
                case "oct": ac.StepType = AC.StepTypes.Octave; break;
                case "dec": ac.StepType = AC.StepTypes.Decade; break;
                default:
                    throw new ParseException(st.Parameters[0], "LIN, DEC or OCT expected");
            }
            ac.NumberSteps = (int)(netlist.ParseDouble(st.Parameters[1]) + 0.25);
            ac.StartFreq = netlist.ParseDouble(st.Parameters[2]);
            ac.StopFreq = netlist.ParseDouble(st.Parameters[3]);

            Generated = ac;
            netlist.Simulations.Add(ac);
            return true;
        }
    }
}
