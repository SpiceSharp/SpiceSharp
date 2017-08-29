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
                "vinput in gnd 0 pulse(0 5 1u 1n 1n 5u 10u)",
                "rs in out 1k",
                "cl out gnd 1n",
                ".save v(in) v(out)",
                ".tran 1n 20u"
            });

            // Read
            NetlistReader nr = new NetlistReader();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(netlist));
            nr.Parse(ms);

            // Create the plots for the output
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
                            DC dc = (DC)sender;
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
