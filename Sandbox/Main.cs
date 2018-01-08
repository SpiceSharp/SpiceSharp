using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;

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
                new Voltagesource("V1", "IN", "0", new Pulse(0, 5, 1e-6, 1e-10, 1e-10, 1e-6, 2e-6)),
                new Resistor("R1", "IN", "OUT", 1e3),
                new Capacitor("C1", "OUT", "0", 1e-9)
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

        /// <summary>
        /// Assign parameters to an entity
        /// </summary>
        /// <param name="src">String with parameters</param>
        /// <param name="e">Entity</param>
        void AssignParameters(string src, Entity e)
        {
            // Split in parameters
            string[] assignments = src.Split(' ');

            // Assign parameters
            foreach (var assignment in assignments)
            {
                // Split in name and value
                if (string.IsNullOrWhiteSpace(assignment))
                    continue;
                string[] parts = assignment.Split('=');
                if (parts.Length != 2)
                    continue;
                string parameter = parts[0].Trim();
                double value = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

                // Assign the parameter
                e.Parameters.Set(parameter.ToLower(), value);
            }
        }
    }
}