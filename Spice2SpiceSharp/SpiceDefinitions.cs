using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace Spice2SpiceSharp
{
    public class SpiceDefinitions
    {
        /// <summary>
        /// Gets the state names (ordered)
        /// </summary>
        public List<Tuple<int, string>> States { get; } = new List<Tuple<int, string>>();

        /// <summary>
        /// Gets the number of states
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the state names
        /// </summary>
        public HashSet<string> StateNames { get; } = new HashSet<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dev">The spice device</param>
        public SpiceDefinitions(SpiceDevice dev, SpiceSetup setup)
        {
            // Get the variable
            string var = setup.StatesVariable;
            string code;
            Count = 0;

            // Open the definitions file
            using (StreamReader sr = new StreamReader(Path.Combine(dev.Folder, dev.Def)))
                code = sr.ReadToEnd();

            // Find all the states
            Regex r = new Regex($@"#define\s*(?<name>\w+)\s*{setup.StatesVariable}\s*\+\s*(?<offset>\d+)");
            var ms = r.Matches(code);
            foreach (Match m in ms)
            {
                int offset = Convert.ToInt32(m.Groups["offset"].Value);
                States.Add(new Tuple<int, string>(offset, m.Groups["name"].Value));
                StateNames.Add(m.Groups["name"].Value);
                Count = Math.Max(Count, offset + 1);
            }
        }
    }
}
