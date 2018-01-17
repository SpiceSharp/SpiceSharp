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

            // Build circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "b", "0", 0),
                new Voltagesource("V2", "c", "0", 0),
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

            // Create simulation
            DC dc = new DC("dc");
            dc.Sweeps.Add(new DC.Sweep("V1", 0, 0.8, 0.1));
            dc.Sweeps.Add(new DC.Sweep("V2", 0, 5, 0.5));
            
            // Create export
            Func<State, double>[] exports = new Func<State, double>[1];
            dc.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = dc.CreateExport("V2", "i");
            };

            // Provided by Spice 3f5
            double[][] references =
            {
                new double[] { -7.749159418341102e-48, -7.656097977815080e-13, -1.374900193695794e-12, -2.039257651631488e-12, -2.742694960033987e-12, -3.524291969370097e-12, -4.320099833421409e-12, -5.186961971048731e-12, -6.096456672821660e-12, -7.077005648170598e-12, -8.100187187665142e-12, 8.841977827306400e-12, -2.163602630389505e-12, -2.838618229361600e-12, -3.559819106158102e-12, -4.341416115494212e-12, -5.172751116333529e-12, -6.053824108676054e-12, -6.977529665164184e-12, -7.958078640513122e-12, -8.981260180007666e-12, -1.003286342893261e-11, 4.376458554330776e-10, -1.342783662039437e-10, -1.413802408478659e-10, -1.485318534832913e-10, -1.557296513965412e-10, -1.629842927286518e-10, -1.702744611975504e-10, -1.776072622305946e-10, -1.849969066824997e-10, -1.924576054079807e-10, -1.999467258428922e-10, 2.168331802615873e-08, -1.248734982084443e-08, -1.310184316594132e-08, -1.371638091995919e-08, -1.433097907010961e-08, -1.494561274739681e-08, -1.556030326810287e-08, -1.617502221051836e-08, -1.678982641806215e-08, -1.740465904731536e-08, -1.801953430913272e-08, 1.118392232919437e-06, -1.167279563674128e-06, -1.225296003326548e-06, -1.283312485611532e-06, -1.341329031845362e-06, -1.399345634922611e-06, -1.457362245105287e-06, -1.515378954763946e-06, -1.573395692844315e-06, -1.631412473557248e-06, -1.689429325324454e-06, 6.188771226659289e-05, -1.090991015075815e-04, -1.145814656666744e-04, -1.200638293852307e-04, -1.255461928977297e-04, -1.310285561544333e-04, -1.365109192050795e-04, -1.419932820425629e-04, -1.474756446668835e-04, -1.529580070354086e-04, -1.584403691765601e-04, 3.275398651073244e-03, -1.012844747663877e-02, -1.064307370146267e-02, -1.115769751705642e-02, -1.167231893956000e-02, -1.218693796899117e-02, -1.270155460534284e-02, -1.321616884871446e-02, -1.373078069906342e-02, -1.424539015644655e-02, -1.475999722083543e-02, 4.151252071174639e-02, -6.251661192344482e-01, -6.571964478052514e-01, -6.892240252287039e-01, -7.212450477265975e-01, -7.532595094786672e-01, -7.852674132045365e-01, -8.172687616249732e-01, -8.492635574593237e-01, -8.812518034248455e-01, -9.132335022374605e-01, 9.946737270218727e-02, -4.309660066591821e+00, -4.531653532981817e+00, -4.752611214870441e+00, -4.973944322875667e+00, -5.195185187116579e+00, -5.416319633996054e+00, -5.637347314442820e+00, -5.858268298886514e+00, -6.079082670361529e+00, -6.299790512203472e+00 }
            };

            // Run test
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationDataEventArgs args) =>
            {
                double actual = exports[0](args.Circuit.State);
                double expected = references[0][index++];
                plotInput.Points.AddXY(dc.Sweeps[1].CurrentValue, exports[0](args.Circuit.State));
                plotOutput.Points.AddXY(dc.Sweeps[1].CurrentValue, expected);
            };
            dc.Run(ckt);
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