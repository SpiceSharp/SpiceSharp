using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A subcircuit
    /// </summary>
    public class Subcircuit : CircuitComponent<Subcircuit>
    {
        /// <summary>
        /// The objects in the subcircuit
        /// </summary>
        public CircuitObjects Objects { get; } = new CircuitObjects();

        /// <summary>
        /// Gets or sets the delimiter used for nodes in the subcircuit.
        /// Internal nodes are mapped to SUBCKT.NODE, where the . is the delimiter.
        /// </summary>
        public string Delimiter { get; set; } = ".";

        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<string, string> map = new Dictionary<string, string>();

        /// <summary>
        /// Gets the pin names of the subcircuit
        /// </summary>
        public string[] Pins { get; private set; } = null;

        /// <summary>
        /// Gets the number of nodes
        /// </summary>
        public override int PinCount => Pins.Length;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the subcircuit</param>
        public Subcircuit(string name, params string[] pins)
            : base(name)
        {
            Pins = pins;
        }

        /// <summary>
        /// Connect the subcircuit
        /// </summary>
        /// <param name="nodes"></param>
        public override void Connect(params string[] nodes)
        {
            if (Pins.Length != nodes.Length)
                throw new CircuitException($"{Name}: Node count mismatch. {nodes.Length} given, {Pins.Length} expected.");

            // Keep a map for the setup
            map.Clear();
            for (int i = 0; i < Pins.Length; i++)
                map.Add(Pins[i], nodes[i]);
        }

        /// <summary>
        /// Get the node
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns></returns>
        public override string GetNode(int i)
        {
            return map[Pins[i]];
        }

        /// <summary>
        /// Setup all objects in the subcircuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            // Introduce a pin new map
            ckt.Nodes.PushPinMap(Name + Delimiter, map);

            // Setup all the objects
            foreach (var c in Objects)
                c.Setup(ckt);

            // Restore the previous pin map
            ckt.Nodes.PopPinMap();
        }

        /// <summary>
        /// Set the IC of all objects in the subcircuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void SetIc(Circuit ckt)
        {
            foreach (var c in Objects)
                c.SetIc(ckt);
        }

        /// <summary>
        /// Do temperature-dependent calculations for all objects in the subcircuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            foreach (var c in Objects)
                c.Temperature(ckt);
        }

        /// <summary>
        /// Load all objects in the subcircuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            foreach (var c in Objects)
                c.Load(ckt);
        }

        /// <summary>
        /// Load all objects in the subcircuit for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            foreach (var c in Objects)
                c.AcLoad(ckt);
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Accept(Circuit ckt)
        {
            foreach (var c in Objects)
                c.Accept(ckt);
        }

        /// <summary>
        /// Unsetup/destroy all objects in the subcircuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup(Circuit ckt)
        {
            foreach (var c in Objects)
                c.Unsetup(ckt);
        }
    }
}
