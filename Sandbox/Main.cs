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

            Circuit ckt = new Circuit();
            Diode d = new Diode("D1");
            d.Connect("OUT", "GND");
            d.SetModel(new DiodeModel("DiodeModel"));
            ckt.Objects.Add(
                new Voltagesource("V1", "IN", "GND", new Pulse(-5, 5, 1e-3, 1e-5, 1e-5, 1e-3, 2e-3)),
                new Resistor("R1", "IN", "OUT", 1e3),
                d
                );
            DC dc = new DC("DC 1");
            dc.Sweeps.Add(new DC.Sweep("V1", -5.0, 5.0, 10e-3));
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                output.Points.AddXY(dc.Sweeps[0].CurrentValue, data.GetVoltage("OUT"));
            };
            ckt.Simulate(dc);

            chMain.ChartAreas[0].AxisX.RoundAxisValues();
        }
    }
}
