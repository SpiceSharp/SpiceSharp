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
            plotInput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            var plotOutput = chMain.Series.Add("Output");
            plotOutput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            // Create the circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Currentsource("I1", "IN", "0", 0.0),
                new Voltagesource("Vmeas", "IN", "0", 0.0),
                new CurrentControlledCurrentsource("H1", "OUT", "IN", "Vmeas", 2.0),
                new Resistor("R1", "OUT", "0", 1e3));

            // Create and run simulation
            DC dc = new DC("DC 1");
            dc.Sweeps.Add(new DC.Sweep("I1", -1e-3, 1e-3, 1e-5));
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                plotInput.Points.AddXY(dc.Sweeps[0].CurrentValue, dc.Sweeps[0].CurrentValue * 1e3);
                plotOutput.Points.AddXY(dc.Sweeps[0].CurrentValue, data.GetVoltage("OUT"));
            };
            dc.Run(ckt);
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