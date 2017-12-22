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
            var plotInput = chMain.Series.Add("Input");
            plotInput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            var plotOutput = chMain.Series.Add("Output");
            plotOutput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "IN", "0", new Sine(0, 4, 1e6)),
                new Resistor("R1", "IN", "OUT", 1e3),
                new Resistor("R2", "OUT", "0", 2e3)
                );

            Transient tran = new Transient("Transient 1", 1e-6, 10e-6);
            tran.MaxStep = 1e-8;
            tran.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                plotInput.Points.AddXY(data.GetTime(), data.GetVoltage("IN"));
                plotOutput.Points.AddXY(data.GetTime(), data.GetVoltage("OUT"));
            };
            tran.Run(ckt);
        }
    }
}