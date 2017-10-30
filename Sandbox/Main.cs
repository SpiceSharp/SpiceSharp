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
                new Currentsource("I1", "IN", "0", 1e-3),
                new Resistor("R2", "OUT", "0", 1e3),
                new Resistor("R1", "IN", "OUT", 1e3)
                );

            DC dc = new DC("DC 1");
            dc.Sweeps.Add(new DC.Sweep("I1", 1e-3, 10e-3, 1e-3));
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                Console.WriteLine(data.GetVoltage("IN"));
            };
            dc.Circuit = ckt;
            dc.SetupAndExecute();
        }
    }
}