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

            // The netlist
            string netlist = string.Join(Environment.NewLine, new string[]
            {
                ".MODEL MM NMOS LEVEL=1 IS=1e-32",
                "+VTO=3.03646 LAMBDA=0 KP=5.28747",
                "+CGSO=6.5761e-06 CGDO=1e-11",
                "vinput in gnd 0 pulse(0 5 1u 1n 1n 5u 10u)",
                "mstage out in gnd gnd MM l = 100u w = 100u",
                "rload vdd out 100",
                "cload out gnd 100n",
                "vsupply vdd gnd 5.0",
                ".save v(in) v(out)",
                ".tran 1n 20u"
            });

            chMain.ChartAreas[0].AxisX.Minimum = 0;
            chMain.ChartAreas[0].AxisX.Maximum = 20e-6;

            // Read
            NetlistReader nr = new NetlistReader();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(netlist));
            nr.Parse(ms);

            SpiceSharp.Circuits.CircuitCheck check = new SpiceSharp.Circuits.CircuitCheck();
            check.Check(nr.Netlist.Circuit);

            // Create the plots for the output using the export list
            Series[] plots = new Series[nr.Netlist.Exports.Count];
            for (int i = 0; i < plots.Length; i++)
            {
                plots[i] = chMain.Series.Add(nr.Netlist.Exports[i].Name);
                plots[i].ChartType = SeriesChartType.FastLine;
            }

            // Simulate
            nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                for (int i = 0; i < plots.Length; i++)
                {
                    var export = nr.Netlist.Exports[i];
                    double x = 0.0;
                    switch (data.Circuit.State.Domain)
                    {
                        case SpiceSharp.Circuits.CircuitState.DomainTypes.Time: x = data.GetTime(); break;
                        case SpiceSharp.Circuits.CircuitState.DomainTypes.Frequency: x = data.GetFrequency(); break;
                        case SpiceSharp.Circuits.CircuitState.DomainTypes.None:
                            DC dc = (DC)data.Circuit.Simulation;
                            x = dc.Sweeps[dc.Sweeps.Count - 1].CurrentValue;
                            break;
                        default:
                            throw new Exception("Unknown type");
                    }
                    plots[i].Points.AddXY(x, export.Extract(data));
                }
            };
            nr.Netlist.Simulate();

            chMain.ChartAreas[0].AxisX.RoundAxisValues();

        }

        private void CircuitWarning_WarningGenerated(object sender, SpiceSharp.Diagnostics.WarningArgs e)
        {
            throw new Exception(e.Message);
        }
    }
}
