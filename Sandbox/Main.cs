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
            plotOutput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            chMain.ChartAreas[0].AxisX.IsLogarithmic = true;

            // Create circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "in", "0", 0.0),
                new Voltagesource("V2", "vdd", "0", 5.0),
                new Resistor("R1", "vdd", "out", 10e3),
                new Resistor("R2", "out", "g", 10e3),
                new Capacitor("C1", "in", "g", 1e-6),
                CreateMOS2("M1", "out", "g", "0", "0",
                    "NFET", "VTO = -1.44 KP = 8.64E-6 NSUB = 1e17 TOX = 20e-9")
                );
            ckt.Objects["V1"].Parameters.Set("acmag", 1.0);
            ckt.Objects["M1"].Parameters.Set("l", 6e-6);
            ckt.Objects["M1"].Parameters.Set("w", 1e-6);

            // Create simulation
            AC ac = new AC("ac", "dec", 5, 10, 10e9);

            // Create exports
            Func<State, Complex>[] exports = new Func<State, Complex>[1];
            ac.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = ac.CreateAcVoltageExport("out");
            };

            // Create references
            double[] riref = { 2.701114568959397e-01, 2.297592026993491e-01, 3.614367501477384e-01, 1.939823511070750e-01, 4.176532699259306e-01, 1.414313855763639e-01, 4.452214249772693e-01, 9.512747415960789e-02, 4.572366767698547e-01, 6.164118367404970e-02, 4.622024752123699e-01, 3.931535278467341e-02, 4.642095432402752e-01, 2.491402951340239e-02, 4.650134308270042e-01, 1.574691222820917e-02, 4.653342396231024e-01, 9.942484429536930e-03, 4.654620791267926e-01, 6.275007008504113e-03, 4.655129925001772e-01, 3.959694832353351e-03, 4.655332645790356e-01, 2.498507336198679e-03, 4.655413355303657e-01, 1.576478884956591e-03, 4.655445487118463e-01, 9.946977962705516e-04, 4.655458279147781e-01, 6.276136046196179e-04, 4.655463371765942e-01, 3.959978465130569e-04, 4.655465399176849e-01, 2.498578584664746e-04, 4.655466206304160e-01, 1.576496782075489e-04, 4.655466527627409e-01, 9.947022918549027e-05, 4.655466655548509e-01, 6.276147338624847e-05, 4.655466706474820e-01, 3.959981301663537e-05, 4.655466726748949e-01, 2.498579297169927e-05, 4.655466734820225e-01, 1.576496961048729e-05, 4.655466738033459e-01, 9.947023368109509e-06, 4.655466739312669e-01, 6.276147451549338e-06, 4.655466739821933e-01, 3.959981330028886e-06, 4.655466740024674e-01, 2.498579304294980e-06, 4.655466740105386e-01, 1.576496962838461e-06, 4.655466740137518e-01, 9.947023372605108e-07, 4.655466740150311e-01, 6.276147452678582e-07, 4.655466740155403e-01, 3.959981330312538e-07, 4.655466740157432e-01, 2.498579304366230e-07, 4.655466740158237e-01, 1.576496962856358e-07, 4.655466740158559e-01, 9.947023372650060e-08, 4.655466740158686e-01, 6.276147452689869e-08, 4.655466740158738e-01, 3.959981330315371e-08, 4.655466740158758e-01, 2.498579304366941e-08, 4.655466740158766e-01, 1.576496962856536e-08, 4.655466740158769e-01, 9.947023372650503e-09, 4.655466740158770e-01, 6.276147452689979e-09, 4.655466740158772e-01, 3.959981330315399e-09, 4.655466740158772e-01, 2.498579304366946e-09, 4.655466740158771e-01, 1.576496962856537e-09, 4.655466740158772e-01, 9.947023372650507e-10, 4.655466740158772e-01, 6.276147452689978e-10, 4.655466740158771e-01, 3.959981330315396e-10 };
            Complex[][] references = new Complex[1][];
            references[0] = new Complex[riref.Length / 2];
            for (int i = 0; i < riref.Length; i += 2)
            {
                references[0][i / 2] = new Complex(riref[i], riref[i + 1]);
            }
            
            // Execute simulation
            int index = 0;
            ac.OnExportSimulationData += (object sender, SimulationDataEventArgs args) =>
            {
                double x = args.GetFrequency();
                double actual = exports[0](args.Circuit.State).Magnitude;
                double expected = references[0][index++].Magnitude;
                plotInput.Points.AddXY(x, 20 * Math.Log10(actual));
                plotOutput.Points.AddXY(x, 20 * Math.Log10(expected));
            };
            ac.Run(ckt);
        }

        /// <summary>
        /// Create a MOS2 transistor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="d">Drain</param>
        /// <param name="g">Gate</param>
        /// <param name="s">Source</param>
        /// <param name="b">Bulk</param>
        /// <param name="modelname">Model name</param>
        /// <param name="modelparams">Model parameters</param>
        /// <returns></returns>
        static MOS2 CreateMOS2(Identifier name, Identifier d, Identifier g, Identifier s, Identifier b,
            Identifier modelname, string modelparams)
        {
            // Create model
            MOS2Model model = new MOS2Model(modelname);
            ApplyParameters(model, modelparams);

            // Create transistor
            MOS2 mos = new MOS2(name);
            mos.Connect(d, g, s, b);
            mos.SetModel(model);
            return mos;
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