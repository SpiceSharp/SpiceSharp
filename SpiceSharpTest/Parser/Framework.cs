using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SpiceSharp.Parser;

namespace SpiceSharpTest.Parser
{
    public class Framework
    {
        /// <summary>
        /// Run a netlist using the standard parser
        /// </summary>
        /// <param name="netlist">The netlist string</param>
        /// <returns></returns>
        public Netlist Run(string netlist)
        {
            MemoryStream m = new MemoryStream(Encoding.UTF8.GetBytes(netlist));

            // Create the parser and run it
            NetlistReader r = new NetlistReader();
            r.Parse(m);

            // Return the generated netlist
            return r.Netlist;
        }
    }
}
