using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Components
{
    public interface ICircuitComponent
    {
        /// <summary>
        /// Get the model of the component
        /// </summary>
        ICircuitObject Model { get; }

        /// <summary>
        /// Connect the component
        /// </summary>
        /// <param name="nodes">Nodes</param>
        void Connect(params string[] nodes);
    }
}
