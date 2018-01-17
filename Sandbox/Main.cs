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
            double r1 = 100.0;
            double r2 = 500.0;
            double l1 = 10e-3;
            double l2 = 2e-3;
            double k = 0.693;
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "IN", "0", 0.0),
                new Resistor("R1", "IN", "1", r1),
                new Inductor("L1", "1", "0", l1),
                new Inductor("L2", "OUT", "0", l2),
                new Resistor("R2", "OUT", "0", r2),
                new MutualInductance("M1", "L1", "L2", k)
                );
            ckt.Objects["V1"].Parameters.Set("acmag", 1.0);

            // Create simulation
            AC ac = new AC("ac", "dec", 10, 1, 1.0e8);

            // Create exports
            Func<State, Complex>[] exports = new Func<State, Complex>[1];
            ac.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = ac.CreateAcVoltageExport("OUT");
            };

            // Create references
            double mut = k * Math.Sqrt(l1 * l2);
            double a = l1 * l2 - mut * mut;
            double b = r1 * l2 + r2 * l1;
            double c = r1 * r2;
            double num = mut * r2;
            Func<double, Complex>[] references = {
                (double f) =>
                {
                    Complex s = new Complex(0.0, 2.0 * Math.PI * f);
                    Complex denom = (a * s + b) * s + c;
                    return num * s / denom;
                }
            };
            
            // Run simulation
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