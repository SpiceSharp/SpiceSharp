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

            // Build circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                CreateDiode("D1", "0", "OUT", "1N914", "Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9"),
                new Voltagesource("V1", "OUT", "0", 1.0)
                );
            ckt.Objects["V1"].Parameters.Set("acmag", 1.0);

            // Create simulation
            AC ac = new AC("ac", "dec", 5, 1.0e3, 10.0e6);

            // Create exports
            Func<State, Complex>[] exports = new Func<State, Complex>[1];
            ac.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = ac.CreateAcExport("V1", "i");
            };

            // Create references
            double[] ri_ref = { -1.945791742986885e-12, -1.904705637099517e-08, -1.946103289747125e-12, -3.018754997881332e-08, -1.946885859826953e-12, -4.784404245850086e-08, -1.948851586992178e-12, -7.582769719229839e-08, -1.953789270386556e-12, -1.201788010800761e-07, -1.966192170307985e-12, -1.904705637099495e-07, -1.997346846331992e-12, -3.018754997881245e-07, -2.075603854314768e-12, -4.784404245849736e-07, -2.272176570837208e-12, -7.582769719228451e-07, -2.765944910274710e-12, -1.201788010800207e-06, -4.006234902415568e-12, -1.904705637097290e-06, -7.121702504803603e-12, -3.018754997872460e-06, -1.494740330300116e-11, -4.784404245814758e-06, -3.460467495474045e-11, -7.582769719089195e-06, -8.398150889530617e-11, -1.201788010744768e-05, -2.080105080892987e-10, -1.904705636876583e-05, -5.195572682013223e-10, -3.018754996993812e-05, -1.302127347221150e-09, -4.784404242316795e-05, -3.267854507347871e-09, -7.582769705163549e-05, -8.205537869558709e-09, -1.201788005200868e-04, -2.060843758802494e-08, -1.904705614805916e-04 };
            Complex[][] references = new Complex[1][];
            references[0] = new Complex[ri_ref.Length / 2];
            for (int i = 0; i < ri_ref.Length; i += 2)
            {
                references[0][i / 2] = new Complex(ri_ref[i], ri_ref[i + 1]);
            }

            int index = 0;
            ac.OnExportSimulationData += (object sender, SimulationDataEventArgs args) =>
            {
                double amplitude = exports[0](args.Circuit.State).Magnitude;
                double expected = references[0][index++].Magnitude;
                plotInput.Points.AddXY(args.GetFrequency(), 20 * Math.Log10(amplitude));
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