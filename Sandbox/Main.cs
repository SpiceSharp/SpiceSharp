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

            // Create circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "in", "0", 0.0),
                new Voltagesource("V2", "out", "0", 0.0),
                CreateMOS2("M1", "out", "in", "0", "0",
                    "NFET", "VTO = -1.44 KP = 8.64E-6 NSUB = 1e17 TOX = 20e-9")
                );
            ckt.Objects["M1"].Parameters.Set("l", 6e-6);
            ckt.Objects["M1"].Parameters.Set("w", 1e-6);

            // Create simulation
            DC dc = new DC("dc");
            dc.Sweeps.Add(new DC.Sweep("V2", 0, 3.3, 0.3));
            dc.Sweeps.Add(new DC.Sweep("V1", 0, 3.3, 0.3));

            // Create exports
            Func<State, double>[] exports = new Func<State, double>[1];
            dc.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = dc.CreateExport("V2", "i");
            };

            // Create references
            double[][] references = new double[1][];
            references[0] = new double[] { 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, -5.306938374762972e-07, -6.622829041640560e-07, -7.937581773977502e-07, -9.251256793208626e-07, -1.056391893430447e-06, -1.187563541271432e-06, -1.318647392836713e-06, -1.449650112001313e-06, -1.580578136718209e-06, -1.711437591907900e-06, -1.842234231506578e-06, -1.972973405226006e-06, -8.638458189766005e-07, -1.127266444634827e-06, -1.390443306657313e-06, -1.653387842188881e-06, -1.916112823385289e-06, -2.178631872717426e-06, -2.440959028691271e-06, -2.703108371482166e-06, -2.965093715016955e-06, -3.226928367443185e-06, -3.488624957291339e-06, -3.750195318941171e-06, -1.006642980508283e-06, -1.402162238528042e-06, -1.797292612754292e-06, -2.192049695953252e-06, -2.586451725095741e-06, -2.980518839976069e-06, -3.374272380032708e-06, -3.767734241737990e-06, -4.160926317516499e-06, -4.553870030753595e-06, -4.946585972966656e-06, -5.339093640940220e-06, -1.013799629686942e-06, -1.492058405858433e-06, -2.019403706665254e-06, -2.546235206750328e-06, -3.072559603046376e-06, -3.598402479001807e-06, -4.123791871424482e-06, -4.648757350976099e-06, -5.173329167535178e-06, -5.697537495654653e-06, -6.221411805023045e-06, -6.744980367812900e-06, -1.014566859581403e-06, -1.493160556051296e-06, -2.067521923824644e-06, -2.719762043312447e-06, -3.378280908481190e-06, -4.036155046468613e-06, -4.693417839623045e-06, -5.350105680759867e-06, -6.006256834174742e-06, -6.661910361168374e-06, -7.317105161899917e-06, -7.971879170944456e-06, -1.015356741659893e-06, -1.494299909660686e-06, -2.069063097873367e-06, -2.740840251690813e-06, -3.506586652651430e-06, -4.296774575599588e-06, -5.086176728084939e-06, -5.874834670198922e-06, -6.662793553470293e-06, -7.450100756193913e-06, -8.236804578683890e-06, -9.022953068962966e-06, -1.016165437476091e-06, -1.495471083771302e-06, -2.070653897845618e-06, -2.742900593322288e-06, -3.513262901776043e-06, -4.382683065140698e-06, -5.304470813563661e-06, -6.225375503988383e-06, -7.145400341215117e-06, -8.064599592900621e-06, -8.983030137568078e-06, -9.900749917551870e-06, -1.016989023485014e-06, -1.496668363213843e-06, -2.072286704485665e-06, -2.745024194610477e-06, -3.515924552362666e-06, -4.385920816155342e-06, -5.355855832487864e-06, -6.403691255248455e-06, -7.456069595713784e-06, -8.507429881427832e-06, -9.557835816840147e-06, -1.060735413910678e-05, -1.017823668891155e-06, -1.497885983840097e-06, -2.073953544717280e-06, -2.747200785959000e-06, -3.518664091102590e-06, -4.389267691949250e-06, -5.359844436218669e-06, -6.431152584792028e-06, -7.596428673614574e-06, -8.780248653652516e-06, -9.962909998812256e-06, -1.114448614086207e-05, -1.018665778845741e-06, -1.499118382335761e-06, -2.075646475814351e-06, -2.749419763651192e-06, -3.521468187712419e-06, -4.392707866929063e-06, -5.363962030875167e-06, -6.435978121313036e-06, -7.609441818854552e-06, -8.884419035140827e-06, -1.019964577331805e-05, -1.151357106530835e-05, -1.019512094513571e-06, -1.500360390171152e-06, -2.077357913157599e-06, -2.751670684538682e-06, -3.524323247900116e-06, -4.396224548237776e-06, -5.368188911914150e-06, -6.440953329762988e-06, -7.615191814315274e-06, -8.891527316636293e-06, -1.027054150786247e-05, -1.171578914356212e-05 };

            // Execute simulation
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationDataEventArgs args) =>
            {
                double x = dc.Sweeps[0].CurrentValue;
                double actual = exports[0](args.Circuit.State);
                double expected = references[0][index++];
                plotInput.Points.AddXY(x, actual);
                plotOutput.Points.AddXY(x, expected);
            };
            dc.Run(ckt);
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