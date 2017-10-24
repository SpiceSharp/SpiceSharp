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

            Series ns = chMain.Series.Add("Real part");
            ns.ChartType = SeriesChartType.FastLine;

            NetlistReader nr = new NetlistReader();
            string netlist = string.Join(Environment.NewLine,
                ".MODEL mjd44h11 npn",
                "+ IS = 1.45468e-14 BF = 135.617 NF = 0.85 VAF = 10",
                "+ IKF = 5.15565 ISE = 2.02483e-13 NE = 3.99964 BR = 13.5617",
                "+ NR = 0.847424 VAR = 100 IKR = 8.44427 ISC = 1.86663e-13",
                "+ NC = 1.00046 RB = 1.35729 IRB = 0.1 RBM = 0.1",
                "+ RE = 0.0001 RC = 0.037687 XTB = 0.90331 XTI = 1",
                "+ EG = 1.20459 CJE = 3.02297e-09 VJE = 0.649408 MJE = 0.351062",
                "+ TF = 2.93022e-09 XTF = 1.5 VTF = 1.00001 ITF = 0.999997",
                "+ CJC = 3.0004e-10 VJC = 0.600008 MJC = 0.409966 XCJC = 0.8",
                "+ FC = 0.533878 CJS = 0 VJS = 0.75 MJS = 0.5",
                "+ TR = 2.73328e-08 PTF = 0 KF = 0 AF = 1",
                "V1 in 0 PULSE(0 5 1u 1n 0.5u 2u 6u)",
                "Vsupply vdd 0 5",
                "R1 vdd out 10k",
                "R2 in b 1k",
                "Q1 out b 0 0 mjd44h11",
                ".SAVE v(out)",
                ".tran 1n 10u"
                );
            nr.Parse(new MemoryStream(Encoding.UTF8.GetBytes(netlist)));
            
            nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                ns.Points.AddXY(data.GetTime(), data.GetVoltage(new SpiceSharp.Circuits.CircuitIdentifier("out")));
            };
            nr.Netlist.Simulate();

            MessageBox.Show("Total transient simulation time: " + nr.Netlist.Circuit.Statistics.TransientTime.ElapsedMilliseconds);
        }
    }
}
