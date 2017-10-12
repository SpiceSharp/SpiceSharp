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
        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();

            Series ns = chMain.Series.Add("Output noise density");
            ns.ChartType = SeriesChartType.FastLine;

            NetlistReader nr = new NetlistReader();
            string netlist = string.Join(Environment.NewLine,
                ".model 1N914 D(Is= 2.52n Rs = .568 N= 1.752 Cjo= 4p M = .4 tt= 20n Kf=1e-14 Af=0.9)",
                "V1 A 0 DC 0 PULSE(0 5 1u 10n 10n 2u 4u)",
                "X1 A B rcfilter",
                ".SUBCKT rcfilter in out R=1k C=1n",
                "R1 in out {R}",
                "C1 out 0 {C}",
                "R2 out out2 {R}",
                ".ENDS",
                ".SAVE V(B)",
                ".tran 1n 10u");
            nr.Parse(new MemoryStream(Encoding.UTF8.GetBytes(netlist)));

            nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                ns.Points.AddXY(data.GetTime(), nr.Netlist.Exports[0].Extract(data));
            };
            nr.Netlist.Simulate();
        }
    }
}
