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
            chMain.ChartAreas[0].AxisX.IsLogarithmic = true;
            // Build circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageSource("Vsupply", "vdd", "0", 5.0),
                new Resistor("R1", "vdd", "out", 1.0e3),
                new Resistor("R2", "out", "b", 10.0e3),
                new Capacitor("Cin", "in", "b", 1e-6),
                CreateBJT("Q1", "c", "b", "0", "0", "mjd44h11", string.Join(" ",
                    "IS = 1.45468e-14 BF = 135.617 NF = 0.85 VAF = 10",
                    "IKF = 5.15565 ISE = 2.02483e-13 NE = 3.99964 BR = 13.5617",
                    "NR = 0.847424 VAR = 100 IKR = 8.44427 ISC = 1.86663e-13",
                    "NC = 1.00046 RB = 1.35729 IRB = 0.1 RBM = 0.1",
                    "RE = 0.0001 RC = 0.037687 XTB = 0.90331 XTI = 1",
                    "EG = 1.20459 CJE = 3.02297e-09 VJE = 0.649408 MJE = 0.351062",
                    "TF = 2.93022e-09 XTF = 1.5 VTF = 1.00001 ITF = 0.999997",
                    "CJC = 3.0004e-10 VJC = 0.600008 MJC = 0.409966 XCJC = 0.8",
                    "FC = 0.533878 CJS = 0 VJS = 0.75 MJS = 0.5",
                    "TR = 2.73328e-08 PTF = 0 KF = 0 AF = 1"))
                );
            ckt.Objects["V1"].ParameterSets.SetProperty("acmag", 1.0);

            // Create simulation
            AC ac = new AC("ac", new SpiceSharp.Simulations.Sweeps.DecadeSweep(10, 10e9, 5));

            // Create an export
            var export = new ComplexVoltageExport(ac, "out");
            var export2 = new RealPropertyExport(ac, "R1", "i");

            // Create references
            double[] riref = { 1.334417655221981e-06, 3.471965089573855e-04, 3.351830745696013e-06, 5.502570946227631e-04, 8.418945895773095e-06, 8.720498043349111e-04, 2.114445669597715e-05, 1.381911086546095e-03, 5.309368526798768e-05, 2.189406695272188e-03, 1.332469066652801e-04, 3.466895147088519e-03, 3.339563613753496e-04, 5.482432501910041e-03, 8.341980040370838e-04, 8.640775529878610e-03, 2.066558898597437e-03, 1.350614394562201e-02, 5.017427448376650e-03, 2.069020111657962e-02, 1.162687214173939e-02, 3.025147596713823e-02, 2.444806066150139e-02, 4.013545957396424e-02, 4.357949902126839e-02, 4.514050832244691e-02, 6.329927287469508e-02, 4.136984589249247e-02, 7.720778968190564e-02, 3.183821694967992e-02, 8.460892410281305e-02, 2.201455496720340e-02, 8.796592758796802e-02, 1.444187547976473e-02, 8.937770311337523e-02, 9.259273400620089e-03, 8.995244044657570e-02, 5.881087349156842e-03, 9.018332766108905e-02, 3.722325696488050e-03, 9.027562051537531e-02, 2.354340724190566e-03, 9.031252808298670e-02, 1.491338054090604e-03, 9.032751174287083e-02, 9.494226860852369e-04, 9.033418279536445e-02, 6.121839678644588e-04, 9.033858322545092e-02, 4.068213923146900e-04, 9.034456220674270e-02, 2.884413123461178e-04, 9.035667499179965e-02, 2.293512095670885e-04, 9.038149464070865e-02, 2.102372862964381e-04, 9.042443030168182e-02, 2.125125991591739e-04, 9.048191631184549e-02, 2.196186992196002e-04, 9.054643271040638e-02, 2.275933681030558e-04, 9.062185343008869e-02, 2.359674744810228e-04, 9.071079692976204e-02, 2.293406880105455e-04, 9.079365040405127e-02, 1.944976240778528e-04, 9.084916360239491e-02, 1.441205567769769e-04, 9.087793546139709e-02, 9.797275248012720e-05, 9.089082649598122e-02, 6.381079386729101e-05, 9.089621923422748e-02, 4.078876004594734e-05, 9.089840984364973e-02, 2.587116901301420e-05, 9.089928903096885e-02, 1.635803306436918e-05, 9.089964017582838e-02, 1.033019503722961e-05, 9.089978014956063e-02, 6.520640901850731e-06, 9.089983590285976e-02, 4.115679165634339e-06, 9.089985810348898e-02, 2.598362267948606e-06, 9.089986694319310e-02, 1.641720674824373e-06, 9.089987046435224e-02, 1.039399501946855e-06 };
            Complex[][] references = new Complex[1][];
            references[0] = new Complex[riref.Length / 2];
            for (int i = 0; i < riref.Length; i += 2)
            {
                references[0][i / 2] = new Complex(riref[i], riref[i + 1]);
            }

            // Execute simulation
            int index = 0;
            ac.OnExportSimulationData += (object sender, ExportDataEventArgs args) =>
            {
                double freq = args.Frequency;
                double actual = export.Value.Magnitude;
                double expected = references[0][index++].Magnitude;
                plotInput.Points.AddXY(freq, export.Decibels);
                plotOutput.Points.AddXY(freq, 20 * Math.Log10(expected));
            };
            ac.Run(ckt);
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