using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;
using System;
using SpiceSharp.Components;

namespace SpiceSharpTest.Models.Bipolar
{
    [TestClass]
    public class BJTTest : Framework
    {
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

        [TestMethod]
        public void BJT_DC()
        {
            /*
             * BJT connect to only voltage sources
             * The current through the base and collector has to match the reference
             * The reference was simulated using Spice 3f5.
             */
            // Build circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "b", "0", 0),
                new VoltageSource("V2", "c", "0", 0),
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
            DC dc = new DC("dc", new SweepConfiguration[] {
                new SweepConfiguration("V1", 0, 0.8, 0.1),
                new SweepConfiguration("V2", 0, 5, 0.5)
            });

            // Create export
            Func<State, double>[] exports = new Func<State, double>[2];
            dc.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = dc.CreateExport("V2", "i");
                exports[1] = dc.CreateExport("V1", "i");
            };

            // Provided by Spice 3f5
            double[][] references =
            {
                new double[] { -7.749159418341102e-48, -7.656097977815080e-13, -1.374900193695794e-12, -2.039257651631488e-12, -2.742694960033987e-12, -3.524291969370097e-12, -4.320099833421409e-12, -5.186961971048731e-12, -6.096456672821660e-12, -7.077005648170598e-12, -8.100187187665142e-12, 8.841977827306400e-12, -2.163602630389505e-12, -2.838618229361600e-12, -3.559819106158102e-12, -4.341416115494212e-12, -5.172751116333529e-12, -6.053824108676054e-12, -6.977529665164184e-12, -7.958078640513122e-12, -8.981260180007666e-12, -1.003286342893261e-11, 4.376458554330776e-10, -1.342783662039437e-10, -1.413802408478659e-10, -1.485318534832913e-10, -1.557296513965412e-10, -1.629842927286518e-10, -1.702744611975504e-10, -1.776072622305946e-10, -1.849969066824997e-10, -1.924576054079807e-10, -1.999467258428922e-10, 2.168331802615873e-08, -1.248734982084443e-08, -1.310184316594132e-08, -1.371638091995919e-08, -1.433097907010961e-08, -1.494561274739681e-08, -1.556030326810287e-08, -1.617502221051836e-08, -1.678982641806215e-08, -1.740465904731536e-08, -1.801953430913272e-08, 1.118392232919437e-06, -1.167279563674128e-06, -1.225296003326548e-06, -1.283312485611532e-06, -1.341329031845362e-06, -1.399345634922611e-06, -1.457362245105287e-06, -1.515378954763946e-06, -1.573395692844315e-06, -1.631412473557248e-06, -1.689429325324454e-06, 6.188771226659289e-05, -1.090991015075815e-04, -1.145814656666744e-04, -1.200638293852307e-04, -1.255461928977297e-04, -1.310285561544333e-04, -1.365109192050795e-04, -1.419932820425629e-04, -1.474756446668835e-04, -1.529580070354086e-04, -1.584403691765601e-04, 3.275398651073244e-03, -1.012844747663877e-02, -1.064307370146267e-02, -1.115769751705642e-02, -1.167231893956000e-02, -1.218693796899117e-02, -1.270155460534284e-02, -1.321616884871446e-02, -1.373078069906342e-02, -1.424539015644655e-02, -1.475999722083543e-02, 4.151252071174639e-02, -6.251661192344482e-01, -6.571964478052514e-01, -6.892240252287039e-01, -7.212450477265975e-01, -7.532595094786672e-01, -7.852674132045365e-01, -8.172687616249732e-01, -8.492635574593237e-01, -8.812518034248455e-01, -9.132335022374605e-01, 9.946737270218727e-02, -4.309660066591821e+00, -4.531653532981817e+00, -4.752611214870441e+00, -4.973944322875667e+00, -5.195185187116579e+00, -5.416319633996054e+00, -5.637347314442820e+00, -5.858268298886514e+00, -6.079082670361529e+00, -6.299790512203472e+00 },
                new double[] { 9.775749704504574e-48, 2.246041725662363e-13, 2.614727067469995e-13, 2.983412409276807e-13, 3.352097751083868e-13, 3.720783092890974e-13, 4.089468434698100e-13, 4.458153776505225e-13, 4.826839118312330e-13, 5.195524460119474e-13, 5.564209801926601e-13, -9.163711456317003e-12, -1.234151669748940e-13, -8.655576255733877e-14, -4.969635813978357e-14, -1.280919814661274e-14, 2.405020627094245e-14, 6.092348847630547e-14, 9.778289289386066e-14, 1.346561750992237e-13, 1.715294573045867e-13, 2.083888617221419e-13, -4.362396277546310e-10, -1.946692806953365e-12, -1.909805646960194e-12, -1.872946242542639e-12, -1.836114593700700e-12, -1.799199678131913e-12, -1.762368029289973e-12, -1.725536380448034e-12, -1.688621464879247e-12, -1.651734304886077e-12, -1.614874900468521e-12, -2.127448267552623e-08, -9.381093124538609e-11, -9.377407184096853e-11, -9.373718468097536e-11, -9.370032527655781e-11, -9.366341036098902e-11, -9.362660646772270e-11, -9.358971930772952e-11, -9.355285990331197e-11, -9.351597274331880e-11, -9.347911333890124e-11, -1.063884238816648e-06, -8.565123721382406e-09, -8.565083031708554e-09, -8.565046116792985e-09, -8.565009201877416e-09, -8.564972342472998e-09, -8.564935538579732e-09, -8.564898734686466e-09, -8.564861764259746e-09, -8.564824960366479e-09, -8.564788045450911e-09, -5.531442661210750e-05, -8.085499940135854e-07, -8.085497492649196e-07, -8.085496922549673e-07, -8.085496352450150e-07, -8.085495781795515e-07, -8.085495211695992e-07, -8.085494641596469e-07, -8.085494070386723e-07, -8.085493500842311e-07, -8.085492930187677e-07, -2.681479351393345e-03, -7.605226249096653e-05, -7.605207619809651e-05, -7.605189896514597e-05, -7.605172173325014e-05, -7.605154450268659e-05, -7.605136727351081e-05, -7.605119004538974e-05, -7.605101281865645e-05, -7.605083559303338e-05, -7.605065836868707e-05, -3.951596310209637e-02, -5.338082814503875e-03, -5.337435769092402e-03, -5.336826073830347e-03, -5.336216637685243e-03, -5.335607383079921e-03, -5.334998309897032e-03, -5.334389418048646e-03, -5.333780707441726e-03, -5.333172177993561e-03, -5.332563829607673e-03, -1.557775293487333e-01, -6.274168022809146e-02, -6.272622161099883e-02, -6.269326326248359e-02, -6.267034294454477e-02, -6.264772942979435e-02, -6.262513987264506e-02, -6.260256606262404e-02, -6.258000774725092e-02, -6.255746490454839e-02, -6.253493751915851e-02 }
            };

            // Run test
            AnalyzeDC(dc, ckt, exports, references);
        }

        [TestMethod]
        public void BJT_AC()
        {
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
            ckt.Objects["V1"].Parameters.SetProperty("acmag", 1.0);

            // Create simulation
            AC ac = new AC("ac", new SpiceSharp.Simulations.Sweeps.DecadeSweep(10, 10e9, 5));

            // Create exports
            Func<ComplexState, Complex>[] exports = new Func<ComplexState, Complex>[1];
            ac.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = ac.CreateACVoltageExport("out");
            };

            // Create references
            double[] riref = { 1.334417655221981e-06, 3.471965089573855e-04, 3.351830745696013e-06, 5.502570946227631e-04, 8.418945895773095e-06, 8.720498043349111e-04, 2.114445669597715e-05, 1.381911086546095e-03, 5.309368526798768e-05, 2.189406695272188e-03, 1.332469066652801e-04, 3.466895147088519e-03, 3.339563613753496e-04, 5.482432501910041e-03, 8.341980040370838e-04, 8.640775529878610e-03, 2.066558898597437e-03, 1.350614394562201e-02, 5.017427448376650e-03, 2.069020111657962e-02, 1.162687214173939e-02, 3.025147596713823e-02, 2.444806066150139e-02, 4.013545957396424e-02, 4.357949902126839e-02, 4.514050832244691e-02, 6.329927287469508e-02, 4.136984589249247e-02, 7.720778968190564e-02, 3.183821694967992e-02, 8.460892410281305e-02, 2.201455496720340e-02, 8.796592758796802e-02, 1.444187547976473e-02, 8.937770311337523e-02, 9.259273400620089e-03, 8.995244044657570e-02, 5.881087349156842e-03, 9.018332766108905e-02, 3.722325696488050e-03, 9.027562051537531e-02, 2.354340724190566e-03, 9.031252808298670e-02, 1.491338054090604e-03, 9.032751174287083e-02, 9.494226860852369e-04, 9.033418279536445e-02, 6.121839678644588e-04, 9.033858322545092e-02, 4.068213923146900e-04, 9.034456220674270e-02, 2.884413123461178e-04, 9.035667499179965e-02, 2.293512095670885e-04, 9.038149464070865e-02, 2.102372862964381e-04, 9.042443030168182e-02, 2.125125991591739e-04, 9.048191631184549e-02, 2.196186992196002e-04, 9.054643271040638e-02, 2.275933681030558e-04, 9.062185343008869e-02, 2.359674744810228e-04, 9.071079692976204e-02, 2.293406880105455e-04, 9.079365040405127e-02, 1.944976240778528e-04, 9.084916360239491e-02, 1.441205567769769e-04, 9.087793546139709e-02, 9.797275248012720e-05, 9.089082649598122e-02, 6.381079386729101e-05, 9.089621923422748e-02, 4.078876004594734e-05, 9.089840984364973e-02, 2.587116901301420e-05, 9.089928903096885e-02, 1.635803306436918e-05, 9.089964017582838e-02, 1.033019503722961e-05, 9.089978014956063e-02, 6.520640901850731e-06, 9.089983590285976e-02, 4.115679165634339e-06, 9.089985810348898e-02, 2.598362267948606e-06, 9.089986694319310e-02, 1.641720674824373e-06, 9.089987046435224e-02, 1.039399501946855e-06 };
            Complex[][] references = new Complex[1][];
            references[0] = new Complex[riref.Length / 2];
            for (int i = 0; i < riref.Length; i += 2)
            {
                references[0][i / 2] = new Complex(riref[i], riref[i + 1]);
            }

            // Run test
            AnalyzeAC(ac, ckt, exports, references);
        }

        [TestMethod]
        public void BJT_Transient()
        {
            /*
             * Transient analysis of a BJT common emitter amplifier
             * Output voltage should behave like the reference. Reference is from Spice 3f5.
             */
            // Create circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "in", "0", new Pulse(0, 5, 1e-6, 1e-9, 0.5e-6, 2e-6, 6e-6)),
                new VoltageSource("Vsupply", "vdd", "0", 5.0),
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
            double[][] references = new double[1][];
            references[0] = new double[] { 4.999999919200253e+00, 4.999999919200253e+00, 4.999999919200256e+00, 4.999999919200265e+00, 4.999999919200269e+00, 4.999999919200270e+00, 4.999999919200274e+00, 4.999999919200310e+00, 4.999999919200295e+00, 4.999999919200242e+00, 4.999999919200256e+00, 4.999999919200442e+00, 4.999999919200887e+00, 4.999999919202219e+00, 4.999999919203312e+00, 4.999999919203345e+00, 4.999999919206253e+00, 4.999999919222164e+00, 4.999999919212636e+00, 4.999999919216738e+00, 4.999999919222017e+00, 5.000124015022816e+00, 5.000449651951924e+00, 5.001329576247615e+00, 5.002151293341414e+00, 5.002257929417378e+00, 5.002438724039524e+00, 5.002759846404913e+00, 5.003418712876405e+00, 5.004723224530464e+00, 5.007336318800409e+00, 5.012530149220850e+00, 5.022833882134534e+00, 5.042977334556558e+00, 5.081660920713526e+00, 5.152804197265928e+00, 5.271902529014626e+00, 5.255961725511093e+00, 1.875617294698409e+00, 1.655361247606981e-02, 9.556306793041053e-03, 8.143095659325651e-03, 7.985097494789570e-03, 7.763414179333034e-03, 7.883787330306542e-03, 7.763690924611548e-03, 7.859821345931772e-03, 7.719451331631350e-03, 7.459392533437188e-03, 7.089071427095065e-03, 6.635610778872991e-03, 6.022467496745492e-03, 6.343863966130725e-03, 7.492855234706399e-03, 1.052275592393182e-02, 2.183241153816638e-02, 5.191584805748978e-02, 1.338907237316577e-01, 2.754693238928807e-01, 4.714359645347004e-01, 7.100201039096334e-01, 9.802510338232866e-01, 1.271772571759655e+00, 1.575010844052070e+00, 1.881305068174714e+00, 2.183111109781683e+00, 2.474135133404021e+00, 2.749486790768636e+00, 3.005688322429076e+00, 3.240642802699120e+00, 3.453443398605113e+00, 3.644179134223966e+00, 3.662399421927875e+00, 3.662585980380271e+00, 3.663068357316088e+00, 3.664214225053304e+00, 3.665186716351549e+00, 3.665385093496764e+00, 3.665735976785826e+00, 3.666393731870234e+00, 3.667733083761534e+00, 3.670388572950689e+00, 3.675700454677043e+00, 3.686242649710791e+00, 3.707066783973132e+00, 3.747384438419909e+00, 3.816652716661827e+00, 3.570754348234623e+00, 1.137869825844853e+00, 9.015485310391745e-01, 5.114374218051296e-01, 4.317883404067391e-01, 2.896439882102821e-01, 7.880030660984598e-02, 1.607176535743225e-02, 1.218672633846727e-02, 8.940034780949789e-03, 8.466396310908189e-03, 7.810926065821902e-03, 7.931482402276063e-03, 7.741615150852236e-03, 7.888344042805985e-03, 7.757571072370122e-03, 7.870701571456922e-03, 7.786125081560294e-03, 7.700257099983869e-03, 7.468686766496058e-03, 7.081173512162859e-03, 6.644980558710726e-03, 6.001915555808019e-03, 6.334377328642827e-03, 7.494774724254995e-03, 1.052198511477223e-02, 2.183511028512625e-02, 5.173267278225494e-02, 5.736729685240333e-02 };

            // Run simulation
            AnalyzeTransient(tran, ckt, exports, references);
        }

        /*
        [TestMethod]
        public void Emitter_Follower_DC()
        {
            // A test for BJT transistor (model mjd44h11) acting in an emitter-follower transistor circuit.
            // A circuit consists of a BJT transistor and resistor connected to the transistor emitter.

            var collectorVoltage = 100.0;
            var baseVoltages = new double[] { 10.0 };
            var resistancesOfResistorConnectedToEmitter = new int[] { 900, 1000, 1200 };
            var references_ve = new double[] {
                9.450319661131841e+00, // V_C = 100, V_B = 10, R = 900 (from improved spice3f5 RELTOL=1e-16 ABSTOL=1e-20)
                9.452632245347569e+00, // V_C = 100, V_B = 10, R = 1000 (from improved spice3f5 RELTOL=1e-16 ABSTOL=1e-20)
                9.456633700898708e+00, // V_C = 100, V_B = 10, R = 1200 (from improved spice3f5 RELTOL=1e-16 ABSTOL=1e-20)
            };
            double abstol = 1e-12;
            double reltol = 1e-9;

            double spice3f5AbsTol = 1e-20;
            double spice3f5RelTol = 1e-16;

            int reference_ve_index = 0;
            foreach (var baseVoltage in baseVoltages)
            {
                foreach (var resistance in resistancesOfResistorConnectedToEmitter)
                {
                    double reference_ve = references_ve[reference_ve_index];

                    var actual_ve =
                        GetEmitterVoltageFromEmitterFollowerCircuit(
                            baseVoltage,
                            collectorVoltage,
                            resistance,
                            reltol,
                            abstol);

                    double tol = reltol * Math.Abs(actual_ve) + abstol;
                    double difference = Math.Abs(actual_ve - reference_ve);
                    tol += spice3f5RelTol * Math.Abs(reference_ve) + spice3f5AbsTol; // Contribution from tolerance of Spice 3f5

                    double allowed_difference = tol;

                    Assert.IsTrue(allowed_difference > difference);

                    reference_ve_index++;
                }
            }
        }

        private double GetEmitterVoltageFromEmitterFollowerCircuit(
            double baseVoltage, 
            double collectorVoltage,
            int resistance,
            double reltol,
            double abstol)
        {
            var netlist = GetEmitterFollowerNetlist(baseVoltage, collectorVoltage, resistance, reltol, abstol);
            double emitterVoltage = 0;
            netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                emitterVoltage = netlist.Exports[0].Extract(data);
            };

            netlist.Simulate();
            return emitterVoltage;
        }

        private Netlist GetEmitterFollowerNetlist(double baseVoltage, double collectorVoltage, int resistance, double reltol, double abstol)
        {
            var netlist = Run(
                  ".MODEL mjd44h11 npn",
                  "+ IS = 1.45468e-14 BF = 135.617 NF = 0.85 VAF = 10",
                  "+ IKF = 5.15565 ISE = 2.02483e-13 NE = 3.99964 BR = 13.5617",
                  "+ NR = 0.847424 VAR = 100 IKR = 8.44427 ISC = 1.86663e-13",
                  "+ NC = 1.00046 RB = 1.35729 IRB = 0.1 RBM = 0.1",
                  "+ RE = 0.0001 RC = 0.037687 XTB = 0.90331 XTI = 1",
                  "+ EG = 1.20459 CJE = 3.02297e-09 VJE = 0.649408 MJE = 0.351062",
                  "+ TF = 2.93022e-09 XTF = 1.5 VTF = 1.00001 ITF = 0.999997",
                  "+ CJC = 3.0004e-10 VJC = 0.600008 MJC = 0.409966 XCJC = 0.8",
                  "+ FC = 0.533878 CJS = 0 VJS = 0.75 MJS = 0.5",
                  "+ TR = 2.73328e-08 PTF = 0 KF = 0 AF = 1",
                  $"V1 V_B gnd {baseVoltage}",
                  $"V2 V_C gnd {collectorVoltage}",
                  $"R1 V_E gnd {resistance}",
                  "Q1 V_C V_B V_E 0 mjd44h11",
                  ".OP",
                  ".SAVE V(V_E)",
                  $".OPTIONS RELTOL={reltol} ABSTOL={abstol}"
                  );

            return netlist;

        }
        */
    }
}
