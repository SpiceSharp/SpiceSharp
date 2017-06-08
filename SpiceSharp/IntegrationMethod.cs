using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp
{
    /// <summary>
    /// An abstract class that will describe integration methods
    /// </summary>
    public abstract class IntegrationMethod
    {
        /// <summary>
        /// Get the maximum order for the integration method
        /// </summary>
        public virtual int MaxOrder { get; } = 0;

        /// <summary>
        /// Gets the current time
        /// </summary>
        public double Time { get; private set; }
    }
}
