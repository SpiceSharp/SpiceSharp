using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Components
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ConnectedPins : Attribute
    {
        /// <summary>
        /// Get the connected pins
        /// </summary>
        public int[] Pins { get; }

        /// <summary>
        /// Constructor
        /// This information can be used to check for detecting open-circuit nodes
        /// </summary>
        /// <param name="pins"></param>
        public ConnectedPins(params int[] pins)
        {
            Pins = pins;
        }
    }
}
