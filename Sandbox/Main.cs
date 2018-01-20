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
                    "NFET", "VTO = -1.44 KP = 8.64E-6 NSUB = 1e17 TOX = 20e-9 KF = 0.5e-25")
                );
            ckt.Objects["V1"].Parameters.Set("acmag", 1.0);
            ckt.Objects["M1"].Parameters.Set("l", 6e-6);
            ckt.Objects["M1"].Parameters.Set("w", 1e-6);

            // Create simulation
            Noise noise = new Noise("noise", "out", "V1", "dec", 10, 10.0, 10.0e9);

            // Create exports
            Func<State, double>[] exports = new Func<State, double>[2];
            noise.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = noise.CreateNoiseDensityExport(true);
                exports[1] = noise.CreateNoiseDensityExport(false);
            };

            // Create references
            double[][] references = new double[2][];
            references[0] = new double[] { 2.362277630616173e-06, 1.379945267414784e-06, 8.473007758572772e-07, 5.483251555734933e-07, 3.730472062832258e-07, 2.649962499739830e-07, 1.947939731585481e-07, 1.468616981231575e-07, 1.127127241713318e-07, 8.755438258118984e-08, 6.855631329051349e-08, 5.395973684199731e-08, 4.261291387932214e-08, 3.372393096425626e-08, 2.672536768434284e-08, 2.119738853640898e-08, 1.682198426685170e-08, 1.335430849281782e-08, 1.060376070211410e-08, 8.420890083617284e-09, 6.687960230375347e-09, 5.311939245547357e-09, 4.219174575568831e-09, 3.351284863236070e-09, 2.661957766932026e-09, 2.114436968645719e-09, 1.679541365117180e-09, 1.334099339452888e-09, 1.059708909993047e-09, 8.417548116877004e-10, 6.686287034417421e-10, 5.311102416204870e-10, 4.218756922498825e-10, 3.351077295963854e-10, 2.661855491980500e-10, 2.114387464861675e-10, 1.679518309568926e-10, 1.334089539422986e-10, 1.059705753459854e-10, 8.417549847903336e-11, 6.686305453153985e-11, 5.311129198609113e-11, 4.218787896666749e-11, 3.351110370990360e-11, 2.661889619930598e-11, 2.114422120523689e-11, 1.679553229713451e-11, 1.334124592122790e-11, 1.059740872594686e-11, 8.417901372215624e-12, 6.686657144343612e-12, 5.311480973435567e-12, 4.219139713410938e-12, 3.351462208743194e-12, 2.662241468212711e-12, 2.114773974082943e-12, 1.679905085917544e-12, 1.334476449652446e-12, 1.060092730788699e-12, 8.421419957485428e-13, 6.690175731282215e-13, 5.314999561210558e-13, 4.222658301605114e-13, 3.354980797147467e-13, 2.665760056722277e-13, 2.118292562645285e-13, 1.683423674506338e-13, 1.337995038254496e-13, 1.063611319397393e-13, 8.456605843605668e-14, 6.725361617419147e-14, 5.350185447355857e-14, 4.257844187754605e-14, 3.390166683299062e-14, 2.700945942874924e-14, 2.153478448798461e-14, 1.718609560659779e-14, 1.373180924408069e-14, 1.098797205551033e-14, 8.808464705142402e-15, 7.077220478956046e-15, 5.702044308892841e-15, 4.609703049291635e-15, 3.742025544836112e-15, 3.052804804411985e-15, 2.505337310335527e-15, 2.070468422196848e-15, 1.725039785945140e-15, 1.450656067088105e-15, 1.232705332051313e-15, 1.059580909432678e-15 };
            references[1] = new double[] { 2.970552105465777e-07, 2.053392001294538e-07, 1.425716401150457e-07, 1.005637052541777e-07, 7.253421438447867e-08, 5.355847444717597e-08, 4.037521102279383e-08, 3.093870755193646e-08, 2.399259594639712e-08, 1.876085041593681e-08, 1.475173003376309e-08, 1.164174697973875e-08, 9.209128460130984e-09, 7.295852862170559e-09, 5.785653492254035e-09, 4.590866976751766e-09, 3.644227374777685e-09, 2.893495350636724e-09, 2.297774776401085e-09, 1.824881474232857e-09, 1.449401554896117e-09, 1.151223737306888e-09, 9.144110142143362e-10, 7.263231967747751e-10, 5.769293633775424e-10, 4.582664502662651e-10, 3.640115671591474e-10, 2.891434647260522e-10, 2.296742247661058e-10, 1.824364329323833e-10, 1.449142737768106e-10, 1.151094398350513e-10, 9.143465704637279e-11, 7.262912784315901e-11, 5.769137465933203e-11, 4.582590036912043e-11, 3.640082154126390e-11, 2.891421652629465e-11, 2.296739538836536e-11, 1.824366775621906e-11, 1.449147767750264e-11, 1.151100723242943e-11, 9.143535443489147e-12, 7.262985775837507e-12, 5.769212087651685e-12, 4.582665475664515e-12, 3.640158002365927e-12, 2.891497706098719e-12, 2.296815695164321e-12, 1.824442983501082e-12, 1.449224001466344e-12, 1.151176969908153e-12, 9.144297975040658e-13, 7.263748339915787e-13, 5.769974668031978e-13, 4.583428064215170e-13, 3.640920595011466e-13, 2.892260300796565e-13, 2.297578290890759e-13, 1.825205579743038e-13, 1.449986597966671e-13, 1.151939566537973e-13, 9.151923941987867e-14, 7.271374307188275e-14, 5.777600635467486e-14, 4.591054031732387e-14, 3.648546562569634e-14, 2.899886268375259e-14, 2.305204258479738e-14, 1.832831547337173e-14, 1.457612565563390e-14, 1.159565534135988e-14, 9.228183617974503e-15, 7.347633983178169e-15, 5.853860311459011e-15, 4.667313707724732e-15, 3.724806238562389e-15, 2.976145944368217e-15, 2.381463934472801e-15, 1.909091223330287e-15, 1.533872241556531e-15, 1.235825210129142e-15, 9.990780377906112e-16, 8.110230743109810e-16, 6.616457071390669e-16, 5.429910467656397e-16, 4.487402998494061e-16, 3.738742704299892e-16, 3.144060694404477e-16, 2.671687983261964e-16, 2.296469001488209e-16 };

            // Execute simulation
            int index = 0;
            noise.OnExportSimulationData += (object sender, SimulationDataEventArgs args) =>
            {
                double x = args.GetFrequency();
                double actual = exports[0](args.Circuit.State);
                double expected = references[0][index++];
                plotInput.Points.AddXY(x, 20 * Math.Log10(actual));
                plotOutput.Points.AddXY(x, 20 * Math.Log10(expected));
            };
            noise.Run(ckt);
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