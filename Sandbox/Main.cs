using System;
using System.Numerics;
using System.Collections.Generic;
using System.Windows.Forms;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace Sandbox
{
    public partial class Main : Form
    {
        public List<double> input = new List<double>();
        public List<double> output = new List<double>();

        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();

            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "IN", "0", 0),
                new Resistor("R2", "OUT", "0", 1),
                new Resistor("R1", "IN", "OUT", 1)
                );

            DC dc = new DC("DC 1");
            dc.Sweeps.Add(new DC.Sweep("V1", 1, 10, 1));
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                Console.WriteLine(data.GetVoltage("OUT"));
            };
            dc.Circuit = ckt;
            dc.SetupAndExecute();
        }
    }
}