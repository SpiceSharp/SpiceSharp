using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Parameters;

namespace SpiceSharp
{
    /// <summary>
    /// An abstract class for circuit models
    /// Each model can contain multiple CircuitInstance objects
    /// </summary>
    public abstract class CircuitModel : Parameterized
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public CircuitModel(string name) : base(name)
        {
        }
    }
}
