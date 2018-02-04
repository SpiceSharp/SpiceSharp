using System;
using System.Numerics;
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
            plotOutput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            Circuit ckt = new Circuit();
            var src = new VoltageSource("V1", "A", "GND", 0);
            src.ParameterSets.SetProperty("acmag", 1.0);
            ckt.Objects.Add(
              src,
              new Resistor("R1", "A", "B", 10),
              new Inductor("L1", "B", "GND", 0.000018));

            ckt.Validate();
        }
        /// <summary>
        /// Create a BJT with a model
        /// </summary>
        /// <param name="name">Device name</param>
        /// <param name="c">Collector</param>
        /// <param name="b">Base</param>
        /// <param name="e">Emitter</param>
        /// <param name="subst">Substrate</param>
        /// <param name="model">Model name</param>
        /// <param name="modelparams">Model parameters</param>
        BipolarJunctionTransistor CreateBJT(Identifier name,
            Identifier c, Identifier b, Identifier e, Identifier subst,
            Identifier model, string modelparams)
        {
            // Create the model
            BipolarJunctionTransistorModel bjtmodel = new BipolarJunctionTransistorModel(model);
            ApplyParameters(bjtmodel, modelparams);

            // Create the transistor
            BipolarJunctionTransistor bjt = new BipolarJunctionTransistor(name);
            bjt.Connect(c, b, e, subst);
            bjt.SetModel(bjtmodel);
            return bjt;
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
                entity.ParameterSets.SetProperty(name, value);
            }
        }
    }
}