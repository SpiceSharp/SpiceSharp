using System;
using System.Numerics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

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

            var output = chMain.Series.Add("Output");
            output.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            var reference = chMain.Series.Add("Reference");
            reference.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            chMain.ChartAreas[0].AxisX.IsLogarithmic = true;

            // Build the circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("v1", "net2", "net1", 0),
                new VoltageSource("v2", "net1", "0", 24),
                new Inductor(new Identifier("x1", "l1"), "net2", "net3", 100e-3),
                new Capacitor(new Identifier("x1", "c1"), "net3", "0", 100e-6),
                new Inductor("l2", "net3", "out", 250e-3),
                new Resistor("rload", "out", "0", 1e3)
                );
            ckt.Objects["v1"].ParameterSets.SetProperty("acmag", 24.0);

            // Create the simulation, exports and references
            AC ac = new AC("ac", new SpiceSharp.Simulations.Sweeps.DecadeSweep(1, 1e3, 30));
            Export<Complex>[] exports = { new ComplexVoltageExport(ac, "net3") };

            ac.OnExportSimulationData += (sender, args) =>
            {
                double frequency = args.Frequency;
                double amplitude = exports[0].Value.Magnitude;
                output.Points.AddXY(frequency, 20 * Math.Log10(amplitude));
            };
            ac.Run(ckt);
        }

        /// <summary>
        /// Create a voltage switch
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="pos">Positive node</param>
        /// <param name="neg">Negative node</param>
        /// <param name="contSource">Controlling source</param>
        /// <param name="modelName">Model name</param>
        /// <param name="modelParameters">Model parameters</param>
        /// <returns></returns>
        CurrentSwitch CreateCurrentSwitch(Identifier name, Identifier pos, Identifier neg, Identifier contSource, Identifier modelName, string modelParameters)
        {
            CurrentSwitchModel model = new CurrentSwitchModel(modelName);
            ApplyParameters(model, modelParameters);

            CurrentSwitch vsw = new CurrentSwitch(name, pos, neg, contSource);
            vsw.SetModel(model);
            return vsw;
        }

        /// <summary>
        /// Apply a parameter definition to an entity
        /// Parameters are a series of assignments [name]=[value] delimited by spaces.
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="definition">Definition string</param>
        static void ApplyParameters(Entity entity, string definition)
        {
            // Get all assignments
            definition = Regex.Replace(definition, @"\s*\=\s*", "=");
            string[] assignments = definition.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var assignment in assignments)
            {
                // Get the name and value
                string[] parts = assignment.Split('=');
                if (parts.Length != 2)
                    throw new Exception("Invalid assignment");
                string name = parts[0].ToLower();
                double value = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

                // Set the entity parameter
                entity.ParameterSets.SetProperty(name, value);
            }
        }
    }
}