using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Parameters;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// A class that represents a circuit node
    /// </summary>
    public class CircuitNode
    {
        #region Enumerations
        public enum NodeType
        {
            Voltage = 0x03,
            Current = 0x04
        }
        #endregion

        /// <summary>
        /// Gets or sets the name of the node
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the index of the node
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the node type (voltage/current)
        /// </summary>
        public NodeType Type { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        public CircuitNode(NodeType type, int index = 0)
        {
            Type = type;
            Index = index;
        }

        /// <summary>
        /// Nice display
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Name != null)
                return "Node (" + Name + ")";
            else
                return "Created node (" + Type.ToString() + ")";
        }
    }
}
