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
using MathNet.Numerics.Interpolation;

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

            NetlistReader nr = new NetlistReader();
            string netlist = string.Join(Environment.NewLine,
                "V1 in 0 PWL(0 0 1u 1 2u 0)",
                ".SAVE v(in)",
                ".tran 1n 10u"
                );
            nr.Parse(new MemoryStream(Encoding.UTF8.GetBytes(netlist)));

            Series inp = chMain.Series.Add("Input");
            inp.ChartType = SeriesChartType.FastLine;

            nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                inp.Points.AddXY(data.GetTime(), nr.Netlist.Exports[0].Extract(data));
            };
            nr.Netlist.Simulate();

            chMain.ChartAreas[0].AxisX.RoundAxisValues();
        }
    }
}
