using System;
using System.Collections.Generic;

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

            // Execute all the steps
            for (int i = 0; i < Steps.Count; i++)
                Steps[i].Execute(ckt);
        }
    }
}
