using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers.Waveforms
{
    /// <summary>
    /// Reads <see cref="Int"/>
    /// </summary>
    public class PwlReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PwlReader()
            : base(StatementType.Waveform)
        {
            Identifier = "pwl";
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="st">Statement</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            Interpolated ip = new Interpolated();

            if (st.Parameters.Count < 4)
                throw new ParseException(st.Parameters[st.Parameters.Count - 1], "At least 2 points expected", false);
            if (st.Parameters.Count % 2 != 0)
                throw new ParseException(st.Parameters[st.Parameters.Count - 1], "Value expected", false);
            ip.Time = new double[st.Parameters.Count / 2];
            ip.Value = new double[st.Parameters.Count / 2];
            for (int i = 0; i < st.Parameters.Count / 2; i ++)
            {
                ip.Time[i] = netlist.ParseDouble(st.Parameters[i * 2]);
                ip.Value[i] = netlist.ParseDouble(st.Parameters[i * 2 + 1]);
            }

            Generated = ip;
            return true;
        }
    }
}
