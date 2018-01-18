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
                new Voltagesource("V1", "in", "0", new Pulse(0, 5, 1e-6, 1e-9, 0.5e-6, 2e-6, 6e-6)),
                new Voltagesource("Vsupply", "vdd", "0", 5.0),
                new Resistor("R1", "vdd", "out", 10.0e3),
                new Resistor("R2", "in", "b", 1.0e3),
                CreateBJT("Q1", "out", "b", "0", "0", "mjd44h11", string.Join(" ",
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

            // Create simulation
            Transient tran = new Transient("tran", 1e-9, 10e-6);

            // Create exports
            Func<State, double>[] exports = new Func<State, double>[1];
            tran.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = tran.CreateVoltageExport("out");
            };

            // Create references
            double[] reft = { 0.000000000000000e+00, 1.000000000000000e-11, 2.000000000000000e-11, 4.000000000000000e-11, 8.000000000000001e-11, 1.600000000000000e-10, 3.200000000000000e-10, 6.400000000000001e-10, 1.280000000000000e-09, 2.560000000000000e-09, 5.120000000000001e-09, 1.024000000000000e-08, 2.048000000000000e-08, 4.096000000000000e-08, 8.192000000000001e-08, 1.638400000000000e-07, 3.276800000000000e-07, 5.276800000000000e-07, 7.276800000000000e-07, 9.276800000000000e-07, 1.000000000000000e-06, 1.000100000000000e-06, 1.000300000000000e-06, 1.000700000000000e-06, 1.001000000000000e-06, 1.001049696459596e-06, 1.001149089378787e-06, 1.001347875217170e-06, 1.001745446893937e-06, 1.002540590247469e-06, 1.004130876954533e-06, 1.007311450368662e-06, 1.013672597196919e-06, 1.026394890853435e-06, 1.051839478166465e-06, 1.102728652792526e-06, 1.204507002044648e-06, 1.404507002044648e-06, 1.604507002044648e-06, 1.795021509131585e-06, 1.948987782233362e-06, 2.108098212473162e-06, 2.308098212473162e-06, 2.508098212473162e-06, 2.708098212473161e-06, 2.908098212473161e-06, 3.001000000000000e-06, 3.021000000000000e-06, 3.061000000000000e-06, 3.141000000000000e-06, 3.301000000000000e-06, 3.501000000000000e-06, 3.520999999999999e-06, 3.560999999999999e-06, 3.641000000000000e-06, 3.801000000000000e-06, 3.979595922338221e-06, 4.179595922338221e-06, 4.379595922338221e-06, 4.579595922338221e-06, 4.779595922338220e-06, 4.979595922338220e-06, 5.179595922338220e-06, 5.379595922338220e-06, 5.579595922338220e-06, 5.779595922338219e-06, 5.979595922338219e-06, 6.179595922338219e-06, 6.379595922338219e-06, 6.579595922338218e-06, 6.779595922338218e-06, 6.979595922338218e-06, 7.000000000000000e-06, 7.000100000000000e-06, 7.000300000000000e-06, 7.000700000000000e-06, 7.001000000000000e-06, 7.001079999999999e-06, 7.001239999999999e-06, 7.001559999999999e-06, 7.002199999999999e-06, 7.003479999999999e-06, 7.006039999999998e-06, 7.011159999999996e-06, 7.021399999999993e-06, 7.041879999999985e-06, 7.082839999999971e-06, 7.164759999999943e-06, 7.306390803451833e-06, 7.318468698900779e-06, 7.342624489798672e-06, 7.348663437523145e-06, 7.360741332972092e-06, 7.384897123869984e-06, 7.433208705665770e-06, 7.505414609501787e-06, 7.622232720197830e-06, 7.743020365228342e-06, 7.943020365228342e-06, 8.143020365228341e-06, 8.343020365228341e-06, 8.543020365228341e-06, 8.743020365228341e-06, 8.943020365228340e-06, 9.000999999999999e-06, 9.020999999999999e-06, 9.060999999999998e-06, 9.140999999999998e-06, 9.300999999999998e-06, 9.500999999999998e-06, 9.520999999999997e-06, 9.560999999999997e-06, 9.640999999999996e-06, 9.800999999999996e-06, 9.978973520830014e-06, 9.999999999999999e-06 };
            double[] refv = { 4.999999919200253e+00, 4.999999919200253e+00, 4.999999919200256e+00, 4.999999919200265e+00, 4.999999919200269e+00, 4.999999919200270e+00, 4.999999919200274e+00, 4.999999919200310e+00, 4.999999919200295e+00, 4.999999919200242e+00, 4.999999919200256e+00, 4.999999919200442e+00, 4.999999919200887e+00, 4.999999919202219e+00, 4.999999919203312e+00, 4.999999919203345e+00, 4.999999919206253e+00, 4.999999919222164e+00, 4.999999919212636e+00, 4.999999919216738e+00, 4.999999919222017e+00, 5.000124015022816e+00, 5.000449651951924e+00, 5.001329576247615e+00, 5.002151293341414e+00, 5.002257929417378e+00, 5.002438724039524e+00, 5.002759846404913e+00, 5.003418712876405e+00, 5.004723224530464e+00, 5.007336318800409e+00, 5.012530149220850e+00, 5.022833882134534e+00, 5.042977334556558e+00, 5.081660920713526e+00, 5.152804197265928e+00, 5.271902529014626e+00, 5.255961725511093e+00, 1.875617294698409e+00, 1.655361247606981e-02, 9.556306793041053e-03, 8.143095659325651e-03, 7.985097494789570e-03, 7.763414179333034e-03, 7.883787330306542e-03, 7.763690924611548e-03, 7.859821345931772e-03, 7.719451331631350e-03, 7.459392533437188e-03, 7.089071427095065e-03, 6.635610778872991e-03, 6.022467496745492e-03, 6.343863966130725e-03, 7.492855234706399e-03, 1.052275592393182e-02, 2.183241153816638e-02, 5.191584805748978e-02, 1.338907237316577e-01, 2.754693238928807e-01, 4.714359645347004e-01, 7.100201039096334e-01, 9.802510338232866e-01, 1.271772571759655e+00, 1.575010844052070e+00, 1.881305068174714e+00, 2.183111109781683e+00, 2.474135133404021e+00, 2.749486790768636e+00, 3.005688322429076e+00, 3.240642802699120e+00, 3.453443398605113e+00, 3.644179134223966e+00, 3.662399421927875e+00, 3.662585980380271e+00, 3.663068357316088e+00, 3.664214225053304e+00, 3.665186716351549e+00, 3.665385093496764e+00, 3.665735976785826e+00, 3.666393731870234e+00, 3.667733083761534e+00, 3.670388572950689e+00, 3.675700454677043e+00, 3.686242649710791e+00, 3.707066783973132e+00, 3.747384438419909e+00, 3.816652716661827e+00, 3.570754348234623e+00, 1.137869825844853e+00, 9.015485310391745e-01, 5.114374218051296e-01, 4.317883404067391e-01, 2.896439882102821e-01, 7.880030660984598e-02, 1.607176535743225e-02, 1.218672633846727e-02, 8.940034780949789e-03, 8.466396310908189e-03, 7.810926065821902e-03, 7.931482402276063e-03, 7.741615150852236e-03, 7.888344042805985e-03, 7.757571072370122e-03, 7.870701571456922e-03, 7.786125081560294e-03, 7.700257099983869e-03, 7.468686766496058e-03, 7.081173512162859e-03, 6.644980558710726e-03, 6.001915555808019e-03, 6.334377328642827e-03, 7.494774724254995e-03, 1.052198511477223e-02, 2.183511028512625e-02, 5.173267278225494e-02, 5.736729685240333e-02 };

            // Run simulation
            tran.OnExportSimulationData += (object sender, SimulationDataEventArgs args) =>
            {
                double actual = exports[0](args.Circuit.State);
                plotInput.Points.AddXY(args.GetTime(), actual);
            };
            tran.Run(ckt);

            // Reference
            for (int i = 0; i < reft.Length; i++)
                plotOutput.Points.AddXY(reft[i], refv[i]);
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
        static BJT CreateBJT(Identifier name,
            Identifier c, Identifier b, Identifier e, Identifier subst,
            Identifier model, string modelparams)
        {
            // Create the model
            BJTModel bjtmodel = new BJTModel(model);
            ApplyParameters(bjtmodel, modelparams);

            // Create the transistor
            BJT bjt = new BJT(name);
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
                entity.Parameters.Set(name, value);
            }
        }
    }
}