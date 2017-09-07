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
using SpiceSharp.Designer;
using SpiceSharp.Diagnostics;

namespace Sandbox
{
    public partial class Main : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();
            Series input = chMain.Series.Add("Input");
            input.ChartType = SeriesChartType.Line;
            Series output = chMain.Series.Add("Output");
            output.ChartType = SeriesChartType.Line;
            chMain.ChartAreas[0].AxisX.IsLogarithmic = true;
            
            // Build the circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "IN", "GND", 0.0),
                new Resistor("R1", "IN", "OUT", 1e3),
                new Capacitor("C1", "OUT", "GND", 1e-6)
                );
            (ckt.Objects["V1"] as IParameterized).Set("acmag", 1.0);

            // Simulation
            AC ac = new AC("AC 1", "dec", 100, 1.0, 1e6);
            ac.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                output.Points.AddXY(data.GetFrequency(), data.GetDb("OUT"));
            };
            ckt.Simulate(ac);
            
            chMain.ChartAreas[0].AxisX.RoundAxisValues();
        }
    }
}
