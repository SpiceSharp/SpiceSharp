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
                new Capacitor("Cin", "in", "g", 1e-6),
                CreateMOS1("M1", "out", "g", "0", "0",
                    "MM", "IS = 1e-32 VTO = 3.03646 LAMBDA = 0 KP = 5.28747 CGSO = 6.5761e-06 CGDO = 1e-11 KF = 1e-25")
                );
            ckt.Objects["V1"].Parameters.Set("acmag", 1.0);
            ckt.Objects["M1"].Parameters.Set("w", 100e-6);
            ckt.Objects["M1"].Parameters.Set("l", 100e-6);

            // Create simulation
            Noise noise = new Noise("noise", "out", "V1", "dec", 10, 10.0, 10e9);

            // Create exports
            Func<State, double>[] exports = new Func<State, double>[2];
            noise.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = noise.CreateNoiseDensityExport(true);
                exports[1] = noise.CreateNoiseDensityExport(false);
            };

            // References
            double[][] references = new double[2][];
            references[0] = new double[] { 2.815379564675864e-12, 1.644868840839549e-12, 1.010121306101684e-12, 6.537880726721799e-13, 4.448524865997785e-13, 3.160333336157883e-13, 2.323257132196350e-13, 1.751655483262268e-13, 1.344386048382044e-13, 1.044322995501785e-13, 8.177248667148335e-14, 6.436215686569252e-14, 5.082789164771354e-14, 4.022525188195949e-14, 3.187747693804598e-14, 2.528380544270212e-14, 2.006491938974725e-14, 1.592876491956225e-14, 1.264799208788146e-14, 1.004433497404203e-14, 7.977357413355818e-15, 6.336091407348683e-15, 5.032685020810389e-15, 3.997501981674192e-15, 3.175301978989497e-15, 2.522243079680978e-15, 2.003518973571566e-15, 1.591491353272600e-15, 1.264211017007844e-15, 1.004245449843196e-15, 7.977486981440409e-16, 6.337231268378405e-16, 5.034333047893085e-16, 3.999405843517482e-16, 3.177334786554185e-16, 2.524340970390206e-16, 2.005649771592597e-16, 1.593638826030865e-16, 1.266366961773762e-16, 1.006405713131741e-16, 7.999111715405073e-17, 6.358867367592469e-17, 5.055975025237714e-17, 4.021050881748337e-17, 3.198981431322685e-17, 2.545988466054381e-17, 2.027297722561883e-17, 1.615287023394526e-17, 1.288015294111360e-17, 1.028054120362606e-17, 8.215596208788723e-18, 6.575352100860561e-18, 5.272459896934062e-18, 4.237535834307345e-18, 3.415466431655031e-18, 2.762473494901996e-18, 2.243782768585602e-18, 1.831772079846699e-18, 1.504500356938407e-18, 1.244539187109050e-18, 1.038044690046574e-18, 8.740202807553306e-19, 7.437310612966987e-19, 6.402386556161864e-19, 5.580317157140351e-19, 4.927324222647428e-19, 4.408633497727489e-19, 3.996622809833569e-19, 3.669351087408033e-19, 3.409389917808688e-19, 3.202895420778084e-19, 3.038871011332858e-19, 2.908581791507522e-19, 2.805089385176244e-19, 2.722882444208535e-19, 2.657583149061083e-19, 2.605714073887111e-19, 2.564513000872298e-19, 2.531785821973098e-19, 2.505789694518999e-19, 2.485140228257554e-19, 2.468737761164706e-19, 2.455708797860733e-19, 2.445359491890543e-19, 2.437138694434664e-19, 2.430608601350282e-19, 2.425421434899278e-19, 2.421300917602111e-19, 2.418027550399525e-19, 2.415426909176026e-19, 2.413360333294781e-19 };
            references[1] = new double[] { 1.101938772677249e-12, 1.020351798553683e-12, 9.930914904110209e-13, 1.018704151068570e-12, 1.098549779218339e-12, 1.236872518232169e-12, 1.441022728380007e-12, 1.721840550874621e-12, 2.094217487153999e-12, 2.577858124390719e-12, 3.198267097692130e-12, 3.987983099859464e-12, 4.988064706261346e-12, 6.249786808519018e-12, 7.836401914012315e-12, 9.824601582961330e-12, 1.230487975385098e-11, 1.537918793701761e-11, 1.915286183784488e-11, 2.371564318075358e-11, 2.910415518231322e-11, 3.523777729653164e-11, 4.182805832714524e-11, 4.828952148482491e-11, 5.372934740827330e-11, 5.712303094004328e-11, 5.769531786489053e-11, 5.530480561891652e-11, 5.052178742786626e-11, 4.432835400406183e-11, 3.770017350647562e-11, 3.134531801206027e-11, 2.565584102156048e-11, 2.077916019829584e-11, 1.671370313489508e-11, 1.338397388087558e-11, 1.068731671781018e-11, 8.518878155014311e-12, 6.783039968978641e-12, 5.397456921217182e-12, 4.293446467982328e-12, 3.414789199740856e-12, 2.715987838819629e-12, 2.160479000532345e-12, 1.719006299308645e-12, 1.368223534346394e-12, 1.089532434335449e-12, 8.681329107747076e-13, 6.922553133235565e-13, 5.525438940163232e-13, 4.415637162479891e-13, 3.534072935825517e-13, 2.833812867202454e-13, 2.277572142155273e-13, 1.835732224046120e-13, 1.484765190559835e-13, 1.205981604962914e-13, 9.845356466124910e-14, 8.086347233940640e-14, 6.689115769628357e-14, 5.579254942299496e-14, 4.697660884412611e-14, 3.997385626141668e-14, 3.441137009036628e-14, 2.999292772256991e-14, 2.648323069715085e-14, 2.369537418245358e-14, 2.148089349130362e-14, 1.972185755582100e-14, 1.832458826459864e-14, 1.721467110986965e-14, 1.633299130424805e-14, 1.563258405234841e-14, 1.507613088730997e-14, 1.463396817940087e-14, 1.428250087013048e-14, 1.400293554605381e-14, 1.378026315480551e-14, 1.360243367639641e-14, 1.345967328032927e-14, 1.334389898454301e-14, 1.324818629450979e-14, 1.316624109090014e-14, 1.309181726819219e-14, 1.301800572736213e-14, 1.293629969876902e-14, 1.283532383362070e-14, 1.269912466960263e-14, 1.250502043061476e-14, 1.222133156026873e-14, 1.180607868779705e-14 };

            // Execute simulation
            int index = 0;
            noise.OnExportSimulationData += (object sender, SimulationDataEventArgs args) =>
            {
                double actual = exports[1](args.Circuit.State);
                double expected = references[1][index++];
                plotInput.Points.AddXY(args.GetFrequency(), 20 * Math.Log10(actual));
                plotOutput.Points.AddXY(args.GetFrequency(), 20 * Math.Log10(expected));
            };
            noise.Run(ckt);
        }

        /// <summary>
        /// Create a MOSFET
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="d">Drain</param>
        /// <param name="g">Gate</param>
        /// <param name="s">Source</param>
        /// <param name="b">Bulk</param>
        /// <param name="modelname">Model name</param>
        /// <param name="modelparams">Model parameters</param>
        /// <returns></returns>
        static MOS1 CreateMOS1(Identifier name, Identifier d, Identifier g, Identifier s, Identifier b,
            Identifier modelname, string modelparams)
        {
            // Create model
            MOS1Model model = new MOS1Model(modelname);
            ApplyParameters(model, modelparams);

            // Create mosfet
            MOS1 mos = new MOS1(name);
            mos.SetModel(model);
            mos.Connect(d, g, s, b);
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