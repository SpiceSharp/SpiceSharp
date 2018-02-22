using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.NewSparse;

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

            /*
             * Step function generator connect to a resistor-inductor in series, coupled to an inductor shunted by another resistor.
             * This linear circuit can be solved analytically. The result may deviate because of truncation errors (going to discrete
             * time points).
             */
            // Create circuit
            double r1 = 100.0;
            double r2 = 500.0;
            double l1 = 10e-3;
            double l2 = 2e-3;
            double k = 0.693;
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "IN", "0", 1.0),
                new Resistor("R1", "IN", "1", r1),
                new Inductor("L1", "1", "0", l1),
                new Inductor("L2", "OUT", "0", l2),
                new Resistor("R2", "OUT", "0", r2),
                new MutualInductance("M1", "L1", "L2", k)
                );
            ckt.Nodes.InitialConditions["1"] = 0;
            ckt.Objects["L1"].ParameterSets.SetProperty("ic", 0);

            // Create simulation
            Transient tran = new Transient("tran", 1e-9, 1e-4, 1e-6);

            // Create exports
            Export<double>[] exports = new Export<double>[1];
            exports[0] = new RealVoltageExport(tran, "OUT");

            // Create references
            double mut = k * Math.Sqrt(l1 * l2);
            double a = l1 * l2 - mut * mut;
            double b = r1 * l2 + r2 * l1;
            double c = r1 * r2;
            double D = Math.Sqrt(b * b - 4 * a * c);
            double invtau1 = (-b + D) / (2.0 * a);
            double invtau2 = (-b - D) / (2.0 * a);
            double factor = mut * r2 / a / (invtau1 - invtau2);
            Func<double, double>[] references = { (double t) => factor * (Math.Exp(t * invtau1) - Math.Exp(t * invtau2)) };

            tran.OnExportSimulationData += (object sender, ExportDataEventArgs args) =>
            {
                double actual = exports[0].Value;
                double expected = references[0](args.Time);
                output.Points.AddXY(args.Time, actual);
                reference.Points.AddXY(args.Time, expected);
            };
            tran.Run(ckt);
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
        static Diode CreateDiode(Identifier name, Identifier anode, Identifier cathode, Identifier model, string modelparams)
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