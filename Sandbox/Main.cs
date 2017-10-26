using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Parameters;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Diagnostics;
using MathNet.Numerics.Interpolation;

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

            int stages = 100;

            Series ns = chMain.Series.Add("Output");
            ns.ChartType = SeriesChartType.FastLine;

            // Build a circuit with a lot of capacitors
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "in", "0", new Pulse(0.0, 5.0, 1e-6, 1e-10, 1e-10, 1e-6, 2e-6))
                );
            string lnode = "in", rnode = "";
            for (int i = 0; i < stages; i++)
            {
                rnode = $"out{i}";
                ckt.Objects.Add(
                    new Resistor($"R{i}", lnode, rnode, 1e2),
                    new Capacitor($"C{i}", rnode, "0", 1e-9)
                    );
                lnode = rnode;
            }

            using (StreamWriter sw = new StreamWriter("test.net"))
            {
                sw.WriteLine("Test circuit");

                sw.WriteLine("V1 in 0 PULSE(0 5 1u 10n 10n 1u 2u)");
                lnode = "in";
                rnode = "";
                for (int i = 0; i < stages; i++)
                {
                    rnode = $"out{i}";
                    sw.WriteLine($"R{i} {lnode} {rnode} 10");
                    sw.WriteLine($"C{i} {rnode} 0 1n");
                    lnode = rnode;
                }
            }

            Transient tran = new Transient("Transient 1", 1e-12, 50e-6);
            tran.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                input.Add(data.GetTime());
                output.Add(data.GetVoltage(rnode));
            };
            ckt.Simulation = tran;
            tran.Circuit = ckt;
            tran.SetupAndExecute();

            for (int i = 0; i < input.Count; i++)
            {
                ns.Points.AddXY(input[i], output[i]);
            }

            MessageBox.Show("Total transient simulation time: " + ckt.Statistics.TransientTime.ElapsedMilliseconds);
        }
    }
}