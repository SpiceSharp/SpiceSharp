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

            string netlist = string.Join(Environment.NewLine,
                ".MODEL diomod D is=1e-14",
                "Vinput IN GND 0.0",
                "Rseries IN OUT {1k * 10}",
                "Dload OUT GND diomod",
                ".SAVE v(OUT)",
                ".DC Vinput -5 5 50m"
                );
            NetlistReader nr = new NetlistReader();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(netlist));
            nr.Parse(ms);
            nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double inp = data.GetVoltage("in");
                double outp = nr.Netlist.Exports[0].Extract(data);
                output.Points.AddXY(inp, outp);
            };
            nr.Netlist.Simulate();

            chMain.ChartAreas[0].AxisX.RoundAxisValues();
        }
    }
}
