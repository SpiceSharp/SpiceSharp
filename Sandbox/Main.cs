using System;
using System.Numerics;
using System.Collections.Generic;
using System.Windows.Forms;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace Sandbox
{
    public partial class Main : Form
    {
        public List<double> input = new List<double>();
        public List<double> output = new List<double>();

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
                new Voltagesource("V1", "IN", "0", new Pulse(0, 5, 1e-3, 1e-6, 1e-6, 1e-3, 2e-3))
                );

            // Add a 100 RC stages
            string node = "IN";
            for (int i = 1; i <= 100; i++)
            {
                string outnode = "out" + i;
                ckt.Objects.Add(new Resistor("R" + i, node, outnode, 1e3));
                ckt.Objects.Add(new Capacitor("C" + i, outnode, "0", 1e-6));
                node = outnode;
            }

            Transient tran = new Transient("Transient 1", 1e-9, 10e-3);
            ckt.Simulation = tran;
            tran.Circuit = ckt;
            tran.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                plotInput.Points.AddXY(data.GetTime(), data.GetVoltage("IN"));
                plotOutput.Points.AddXY(data.GetTime(), data.GetVoltage("out5"));
            };
            tran.SetupAndExecute();

            chMain.ChartAreas[0].AxisX.RoundAxisValues();
        }
    }
}