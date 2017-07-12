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
        public string[] States { get; private set; } = null;

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

            // Open the definitions file
            using (StreamReader sr = new StreamReader(Path.Combine(dev.Folder, dev.Def)))
                code = sr.ReadToEnd();

            // Find all the states
            Dictionary<string, int> list = new Dictionary<string, int>();
            Regex r = new Regex($@"#define\s*(?<name>\w+)\s*{setup.StatesVariable}\s*\+\s*(?<offset>\d+)");
            var ms = r.Matches(code);
            int max = 0;
            foreach (Match m in ms)
            {
                int offset = Convert.ToInt32(m.Groups["offset"].Value);
                list.Add(m.Groups["name"].Value, offset);
                max = Math.Max(offset + 1, max);
                StateNames.Add(m.Groups["name"].Value);
            }

            // Store the ordered states
            States = new string[max];
            foreach (var s in list)
                States[s.Value] = s.Key;
        }
    }
}
