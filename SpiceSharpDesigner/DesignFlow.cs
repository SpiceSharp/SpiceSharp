using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Designer
{
    public class DesignFlow
    {
        /// <summary>
        /// The design steps that need to be executed
        /// </summary>
        public List<DesignStep> Steps { get; } = new List<DesignStep>();

        /// <summary>
        /// Execute the design flow on a circuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public void Execute(Circuit ckt)
        {
            if (ckt == null)
                throw new ArgumentNullException(nameof(ckt));
        }
    }
}
