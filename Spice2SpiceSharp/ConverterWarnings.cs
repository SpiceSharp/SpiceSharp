using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spice2SpiceSharp
{
    public class ConverterWarnings
    {
        /// <summary>
        /// Gets the list of warnings
        /// </summary>
        public static List<string> Warnings { get; } = new List<string>();

        /// <summary>
        /// Add a warning
        /// </summary>
        /// <param name="msg"></param>
        public static void Add(string msg)
        {
            Warnings.Add(msg);
        }
    }
}
