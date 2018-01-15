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
            plotOutput.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;

            // Build circuit
            double capacitance = 1e-3;
            double inductance = 1e-6;
            double initialCurrent = 1e-3;
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Inductor("L1", "OUT", "0", inductance),
                new Capacitor("C1", "OUT", "0", capacitance)
                );
            ckt.Nodes.IC["OUT"] = 0.0;
            ckt.Objects["L1"].Parameters.Set("ic", initialCurrent);

            // Create simulation
            Transient tran = new Transient("tran", 1e-9, 1e-3);
            tran.MaxStep = 1e-7;

            // Create exports
            Func<State, double>[] exports = new Func<State, double>[1];
            tran.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = tran.CreateExport("C1", "v");
            };

            // Create reference function
            double amplitude = Math.Sqrt(inductance / capacitance) * initialCurrent;
            double omega = 1.0 / Math.Sqrt(inductance * capacitance);
            Func<double, double>[] references = { (double t) => -amplitude * Math.Sin(omega * t) };

            // Run test
            tran.OnExportSimulationData += (object sender, SimulationDataEventArgs data) =>
            {
                double t = data.GetTime();
                double actual = exports[0](data.Circuit.State);
                double expected = references[0](t);
                plotInput.Points.AddXY(data.GetTime(), actual);
                plotOutput.Points.AddXY(data.GetTime(), actual - expected);
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