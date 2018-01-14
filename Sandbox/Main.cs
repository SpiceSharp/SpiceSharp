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
            plotOutput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;

            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6; // 0.000001;
            double tau = resistorResistance * capacitance;

            // Build circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Capacitor("C1", "OUT", "0", capacitance),
                new Resistor("R1", "IN", "OUT", resistorResistance),
                new Voltagesource("V1", "IN", "0", dcVoltage)
                );
            ckt.Nodes.IC["OUT"] = 0.0;

            // Create simulation, exports and references
            Transient tran = new Transient("tran", 1e-8, 10e-2);
            Func<State, double> export = null;
            Func<double, double> reference = (double t) => dcVoltage * (1.0 - Math.Exp(-t / tau));

            tran.InitializeSimulationExport += (object sender, SpiceSharp.Behaviors.BehaviorPool pool) =>
            {
                export = tran.CreateExport("R1", "v");
            };
            tran.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                plotInput.Points.AddXY(data.GetTime(), export(data.Circuit.State));
                plotOutput.Points.AddXY(data.GetTime(), reference(data.GetTime()));
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