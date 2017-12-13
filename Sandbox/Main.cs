using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

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
            var plotOutput = chMain.Series.Add("Output");
            plotOutput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "IN", "0", new Pulse(0, 5, 1e-6, 0.1e-6, 0.1e-6, 0.9e-6, 2e-6)),
                new Resistor("R1", "IN", "OUT", 1e4),
                new Resistor("R2", "OUT", "0", 1e4)
                );

            var current = ckt.Objects["V1"].GetBehavior(typeof(SpiceSharp.Behaviors.CircuitObjectBehaviorLoad)).CreateGetter(ckt, "i");

            DC dc = new DC("DC 1", "V1", 0, 10, 1e-3);
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                plotOutput.Points.AddXY(dc.Sweeps[0].CurrentValue, current());
            };
            dc.Run(ckt);
        }
    }
}