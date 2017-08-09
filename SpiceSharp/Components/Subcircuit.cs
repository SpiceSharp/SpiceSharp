using System.Collections.Generic;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class logically groups components together
    /// </summary>
    public class Subcircuit : CircuitComponent
    {
        /// <summary>
        /// The components in the subcircuit
        /// </summary>
        public CircuitComponents Components { get; } = new CircuitComponents();

        /// <summary>
        /// Gets or sets the delimiter nodes in subcircuits
        /// </summary>
        public static string Delimiter { get; set; } = ".";

        private Dictionary<string, string> map = new Dictionary<string, string>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the subcircuit</param>
        public Subcircuit(string name, params string[] terminals)
            : base(name, terminals)
        {
        }

        /// <summary>
        /// Connect the subcircuit
        /// </summary>
        /// <param name="nodes"></param>
        public override void Connect(params string[] nodes)
        {
            base.Connect(nodes);

            // Keep a map for the setup
            map.Clear();
            for (int i = 0; i < terminals.Length; i++)
                map.Add(terminals[i], nodes[i]);
        }

        /// <summary>
        /// No model
        /// </summary>
        /// <returns></returns>
        public override CircuitModel GetModel() => null;

        /// <summary>
        /// Setup all components in the subcircuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            // Order the components
            Components.BuildOrderedComponentList();

            // Introduce a pin new map
            ckt.Nodes.PushPinMap(Name + Delimiter, map);

            // Setup all the components
            foreach (var c in Components)
                c.Setup(ckt);

            // Restore the previous pin map
            ckt.Nodes.PopPinMap();
        }

        /// <summary>
        /// Set the IC of all components in the subcircuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void SetIc(Circuit ckt)
        {
            foreach (var c in Components)
                c.SetIc(ckt);
        }

        /// <summary>
        /// Do temperature-dependent calculations for all components in the subcircuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            foreach (var c in Components)
                c.Temperature(ckt);
        }

        /// <summary>
        /// Load all components in the subcircuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            foreach (var c in Components)
                c.Load(ckt);
        }

        /// <summary>
        /// Load all components in the subcircuit for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            foreach (var c in Components)
                c.AcLoad(ckt);
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Accept(Circuit ckt)
        {
            foreach (var c in Components)
                c.Accept(ckt);
        }

        /// <summary>
        /// Unsetup/destroy all components in the subcircuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup(Circuit ckt)
        {
            foreach (var c in Components)
                c.Unsetup(ckt);
        }
    }
}
