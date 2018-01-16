using System;
using System.Numerics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
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
            plotOutput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            chMain.ChartAreas[0].AxisX.IsLogarithmic = true;

            // Create circuit
            double resistance = 1;
            double inductance = 1e-3;
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "IN", "0", 0.0),
                new Inductor("L1", "IN", "OUT", inductance),
                new Resistor("R1", "OUT", "0", resistance));
            ckt.Objects["V1"].Parameters.Set("acmag", 1.0);

            // Create simulation
            AC ac = new AC("ac", "dec", 10, 0.1, 1.0e6);

            // Create exports
            Func<State, Complex>[] exports = new Func<State, Complex>[1];
            ac.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = ac.CreateAcVoltageExport("OUT");
            };

            // Create references
            Func<double, Complex>[] references = { (double f) => 1.0 / new Complex(1.0, inductance / resistance * 2 * Math.PI * f) };

            ac.OnExportSimulationData += (object sender, SimulationDataEventArgs args) =>
            {
                double actual = exports[0](args.Circuit.State).Magnitude;
                double expected = references[0](args.GetFrequency()).Magnitude;
                plotInput.Points.AddXY(args.GetFrequency(), 20 * Math.Log10(actual));
                plotOutput.Points.AddXY(args.GetFrequency(), 20 * Math.Log10(expected));
            };
            ac.Run(ckt);
        }

        /// <summary>
        /// Create a diode with a model
        /// </summary>
        /// <param name="name">Diode name</param>
        /// <param name="anode">Anode</param>
        /// <param name="cathode">Cathode</param>
        /// <param name="model">Model</param>
        /// <param name="modelparams">Model parameters</param>
        /// <returns></returns>
        protected static Diode CreateDiode(Identifier name, Identifier anode, Identifier cathode, Identifier model, string modelparams)
        {
            Diode d = new Diode(name);
            DiodeModel dm = new DiodeModel(model);
            ApplyParameters(dm, modelparams);
            d.SetModel(dm);
            d.Connect(anode, cathode);
            return d;
        }

        /// <summary>
        /// Apply a parameter definition to an entity
        /// Parameters are a series of assignments [name]=[value] delimited by spaces.
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="definition">Definition string</param>
        protected static void ApplyParameters(Entity entity, string definition)
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
                entity.Parameters.Set(name, value);
            }
        }
    }
}