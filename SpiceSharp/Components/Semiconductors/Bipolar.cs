using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Semiconductors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class describes a bipolar transistor
    /// </summary>
    public class Bipolar : CircuitComponent
    {
        /// <summary>
        /// The model for the bipolar transistor
        /// </summary>
        public BipolarModel Model { get; set; }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("area"), SpiceInfo("Area factor")]
        public Parameter<double> BJTarea { get; } = new Parameter<double>(1.0);
        [SpiceName("temp"), SpiceInfo("Instance temperature")]
        public Parameter<double> BJTtemp { get; } = new Parameter<double>();
        [SpiceName("off"), SpiceInfo("Device initially off")]
        public bool BJToff { get; set; } = false;
        [SpiceName("icvbe"), SpiceInfo("Initial B-E voltage")]
        public Parameter<double> BJTicVBE { get; } = new Parameter<double>();
        [SpiceName("icvce"), SpiceInfo("Initial C-E voltage")]
        public Parameter<double> BJTicVCE { get; } = new Parameter<double>();

        [SpiceName("ic"), SpiceInfo("Current at collector node")]
        public double GetCC(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcc];
        [SpiceName("ib"), SpiceInfo("Current at base node")]
        public double GetCB(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcb];
        [SpiceName("ie"), SpiceInfo("Emitter current")]
        public double GetCE(Circuit ckt)
        {
            double value = -ckt.State.States[0][BJTstate + BJTcc];
            value -= ckt.State.States[0][BJTstate + BJTcb];
            if (ckt.State.Domain == CircuitState.DomainTypes.Time)
                value += ckt.State.States[0][BJTstate + BJTcqcs];
            return value;
        }
        [SpiceName("is"), SpiceInfo("Substrate current")]
        public double GetCS(Circuit ckt) => -ckt.State.States[0][BJTstate + BJTcqcs];
        [SpiceName("vbe"), SpiceInfo("B-E voltage")]
        public double GetVBE(Circuit ckt) => ckt.State.States[0][BJTstate + BJTvbe];
        [SpiceName("vbc"), SpiceInfo("B-C voltage")]
        public double GetVBC(Circuit ckt) => ckt.State.States[0][BJTstate + BJTvbc];
        [SpiceName("gm"), SpiceInfo("Small signal transconductance")]
        public double GetGM(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgm];
        [SpiceName("gpi"), SpiceInfo("Small signal input conductance - pi")]
        public double GetGPI(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgpi];
        [SpiceName("gmu"), SpiceInfo("Small signal conductance - mu")]
        public double GetGMU(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgmu];
        [SpiceName("gx"), SpiceInfo("Conductance from base to internal base")]
        public double GetGX(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgx];
        [SpiceName("go"), SpiceInfo("Small signal output conductance")]
        public double GetGO(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgo];
        [SpiceName("geqcb"), SpiceInfo("d(Ibe)/d(Vbc)")]
        public double GetGEQCB(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgeqcb];
        [SpiceName("gccs"), SpiceInfo("Internal C-S cap. equiv. cond.")]
        public double GetGCCS(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgccs];
        [SpiceName("geqbx"), SpiceInfo("Internal C-B-base cap. equiv. cond.")]
        public double GetGEQBX(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgeqbx];
        [SpiceName("cqbe"), SpiceInfo("Cap. due to charge storage in B-E jct.")]
        public double GetCQBE(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcqbe];
        [SpiceName("cqbc"), SpiceInfo("Cap. due to charge storage in B-C jct.")]
        public double GetCQBC(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcqbc];
        [SpiceName("cqcs"), SpiceInfo("Cap. due to charge storage in C-S jct.")]
        public double GetCQCS(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcqcs];
        [SpiceName("cqbx"), SpiceInfo("Cap. due to charge storage in B-X jct.")]
        public double GetCQBX(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcqbx];
        [SpiceName("cexbc"), SpiceInfo("Total Capacitance in B-X junction")]
        public double GetCEXBC(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcexbc];
        [SpiceName("qbe"), SpiceInfo("Charge storage B-E junction")]
        public double GetQBE(Circuit ckt) => ckt.State.States[0][BJTstate + BJTqbe];
        [SpiceName("qbc"), SpiceInfo("Charge storage B-C junction")]
        public double GetQBC(Circuit ckt) => ckt.State.States[0][BJTstate + BJTqbc];
        [SpiceName("qcs"), SpiceInfo("Charge storage C-S junction")]
        public double GetQCS(Circuit ckt) => ckt.State.States[0][BJTstate + BJTqcs];
        [SpiceName("qbx"), SpiceInfo("Charge storage B-X junction")]
        public double GetQBX(Circuit ckt) => ckt.State.States[0][BJTstate + BJTqbx];
        [SpiceName("p"), SpiceInfo("Power dissipation")]
        public double GetPower(Circuit ckt)
        {
            double value = ckt.State.States[0][BJTstate + BJTcc] * ckt.State.Real.Solution[BJTcolNode];
            value += ckt.State.States[0][BJTstate + BJTcb] * ckt.State.Real.Solution[BJTbaseNode];
            if (ckt.State.Domain == CircuitState.DomainTypes.Time)
                value -= ckt.State.States[0][BJTstate + BJTcqcs] * ckt.State.Real.Solution[BJTsubstNode];
            double tmp = -ckt.State.States[0][BJTstate + BJTcc];
            tmp -= ckt.State.States[0][BJTstate + BJTcb];
            if (ckt.State.Domain == CircuitState.DomainTypes.Time)
                tmp += ckt.State.States[0][BJTstate + BJTcqcs];
            value += tmp * ckt.State.Real.Solution[BJTemitNode];
            return value;
        }
        [SpiceName("cpi"), SpiceInfo("Internal base to emitter capactance")]
        public double BJTcapbe { get; private set; }
        [SpiceName("cmu"), SpiceInfo("Internal base to collector capactiance")]
        public double BJTcapbc { get; private set; }
        [SpiceName("cbx"), SpiceInfo("Base to collector capacitance")]
        public double BJTcapbx { get; private set; }
        [SpiceName("ccs"), SpiceInfo("Collector to substrate capacitance")]
        public double BJTcapcs { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private double BJTtSatCur, BJTtBetaF, BJTtBetaR, BJTtBEleakCur, BJTtBCleakCur, BJTtBEcap, BJTtBEpot, BJTtBCcap, BJTtBCpot, BJTtDepCap, BJTtf1, BJTtf4, BJTtf5, BJTtVcrit;

        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("substnode"), SpiceInfo("Number of the substrate node")]
        public int BJTsubstNode { get; private set; }
        [SpiceName("colnode"), SpiceInfo("Number of the collector node")]
        public int BJTcolNode { get; private set; }
        [SpiceName("basenode"), SpiceInfo("Number of the base node")]
        public int BJTbaseNode { get; private set; }
        [SpiceName("emitnode"), SpiceInfo("Number of the emitter node")]
        public int BJTemitNode { get; private set; }
        [SpiceName("baseprimenode"), SpiceInfo("Number of the internal base node")]
        public int BJTbasePrimeNode { get; private set; }
        [SpiceName("colprimenode"), SpiceInfo("Number of the internal collector node")]
        public int BJTcolPrimeNode { get; private set; }
        [SpiceName("emitprimenode"), SpiceInfo("Number of the internal emitter node")]
        public int BJTemitPrimeNode { get; private set; }
        public int BJTstate { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        private const int BJTvbe = 0;
        private const int BJTvbc = 1;
        private const int BJTcc = 2;
        private const int BJTcb = 3;
        private const int BJTgpi = 4;
        private const int BJTgmu = 5;
        private const int BJTgm = 6;
        private const int BJTgo = 7;
        private const int BJTqbe = 8;
        private const int BJTcqbe = 9;
        private const int BJTqbc = 10;
        private const int BJTcqbc = 11;
        private const int BJTqcs = 12;
        private const int BJTcqcs = 13;
        private const int BJTqbx = 14;
        private const int BJTcqbx = 15;
        private const int BJTgx = 16;
        private const int BJTcexbc = 17;
        private const int BJTgeqcb = 18;
        private const int BJTgccs = 19;
        private const int BJTgeqbx = 20;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public Bipolar(string name) : base(name, 4) { }

        /// <summary>
        /// Setup the bipolar transistor
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            // Add extra nodes if the bipolar transistor has conductances at the collector or emitter
            List<CircuitNode.NodeType> extra = new List<CircuitNode.NodeType>();
            if (Model.BJTcollectorResist > 0.0)
                extra.Add(CircuitNode.NodeType.Voltage);
            if (Model.BJTemitterResist > 0.0)
                extra.Add(CircuitNode.NodeType.Voltage);
            if (Model.BJTbaseResist > 0.0)
                extra.Add(CircuitNode.NodeType.Voltage);

            // Bind the nodes
            CircuitNode[] nodes = BindNodes(ckt, extra.ToArray());
            int index = 4;
            BJTcolNode = nodes[0].Index;
            BJTbaseNode = nodes[1].Index;
            BJTemitNode = nodes[2].Index;
            BJTsubstNode = nodes[3].Index;
            BJTcolPrimeNode = Model.BJTcollectorResist == 0.0 ? BJTcolNode : nodes[index++].Index;
            BJTbasePrimeNode = Model.BJTbaseResist == 0.0 ? BJTbaseNode : nodes[index++].Index;
            BJTemitPrimeNode = Model.BJTemitterResist == 0.0 ? BJTemitNode : nodes[index++].Index;

            // Reserve states
            BJTstate = ckt.State.GetState(21);
        }

        /// <summary>
        /// Get the model for this bipolar transistor
        /// </summary>
        /// <returns></returns>
        public override CircuitModel GetModel() => Model;

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double vt;
            double ratlog;
            double ratio1;
            double factlog;
            double bfactor;
            double factor;
            double fact2;
            double pbo;
            double pbfact;
            double gmaold;
            double gmanew;
            double egfet;
            double arg;

            if (!BJTtemp.Given)
                BJTtemp.Value = ckt.State.Temperature;
            vt = BJTtemp * Circuit.CONSTKoverQ;
            fact2 = BJTtemp / Circuit.CONSTRefTemp;
            egfet = 1.16 - (7.02e-4 * BJTtemp * BJTtemp) /
                    (BJTtemp + 1108);
            arg = -egfet / (2 * Circuit.CONSTBoltz * BJTtemp) +
                    1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            ratlog = Math.Log(BJTtemp / Model.BJTtnom);
            ratio1 = BJTtemp / Model.BJTtnom - 1;
            factlog = ratio1 * Model.BJTenergyGap / vt +
                    Model.BJTtempExpIS * ratlog;
            factor = Math.Exp(factlog);
            BJTtSatCur = Model.BJTsatCur * factor;
            bfactor = Math.Exp(ratlog * Model.BJTbetaExp);
            BJTtBetaF = Model.BJTbetaF * bfactor;
            BJTtBetaR = Model.BJTbetaR * bfactor;
            BJTtBEleakCur = Model.BJTleakBEcurrent *
                    Math.Exp(factlog / Model.BJTleakBEemissionCoeff) / bfactor;
            BJTtBCleakCur = Model.BJTleakBCcurrent *
                    Math.Exp(factlog / Model.BJTleakBCemissionCoeff) / bfactor;

            pbo = (Model.BJTpotentialBE - pbfact) / Model.fact1;
            gmaold = (Model.BJTpotentialBE - pbo) / pbo;
            BJTtBEcap = Model.BJTdepletionCapBE /
                    (1 + Model.BJTjunctionExpBE *
                    (4e-4 * (Model.BJTtnom - Circuit.CONSTRefTemp) - gmaold));
            BJTtBEpot = fact2 * pbo + pbfact;
            gmanew = (BJTtBEpot - pbo) / pbo;
            BJTtBEcap *= 1 + Model.BJTjunctionExpBE *
                    (4e-4 * (BJTtemp - Circuit.CONSTRefTemp) - gmanew);

            pbo = (Model.BJTpotentialBC - pbfact) / Model.fact1;
            gmaold = (Model.BJTpotentialBC - pbo) / pbo;
            BJTtBCcap = Model.BJTdepletionCapBC /
                    (1 + Model.BJTjunctionExpBC *
                    (4e-4 * (Model.BJTtnom - Circuit.CONSTRefTemp) - gmaold));
            BJTtBCpot = fact2 * pbo + pbfact;
            gmanew = (BJTtBCpot - pbo) / pbo;
            BJTtBCcap *= 1 + Model.BJTjunctionExpBC *
                    (4e-4 * (BJTtemp - Circuit.CONSTRefTemp) - gmanew);

            BJTtDepCap = Model.BJTdepletionCapCoeff * BJTtBEpot;
            BJTtf1 = BJTtBEpot * (1 - Math.Exp((1 -
                    Model.BJTjunctionExpBE) * Model.xfc)) /
                    (1 - Model.BJTjunctionExpBE);
            BJTtf4 = Model.BJTdepletionCapCoeff * BJTtBCpot;
            BJTtf5 = BJTtBCpot * (1 - Math.Exp((1 -
                    Model.BJTjunctionExpBC) * Model.xfc)) /
                    (1 - Model.BJTjunctionExpBC);
            BJTtVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * Model.BJTsatCur));
        }

        /// <summary>
        /// Load the bipolar transistor
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;

            double arg1, arg2, arg3, arg, argtf, c2, c4, capbc, capbe, cb, cbc, cbcn, cbe, cben, cbhat, cc, cchat, cdis, ceq, ceqbc, 
                ceqbe, ceqbx, ceqcs, cex, csat, ctot, czbc, czbcf2, czbe, czbef2, czbx, czbxf2, czcs, delvbc, delvbe, denom, dqbdvc, 
                dqbdve, evbc, evbcn, evbe, evben, f1, f2, f3, fcpc, fcpe, gbc, gbcn, gbe, gben, gccs, gcpr, gepr, geq, geqbx, geqcb, 
                gex, gm, gmu, go, gpi, gx, oik, oikr, ovtf, pc, pe, ps, q1, q2, qb, rbpi, rbpr, sarg, sqarg, td, temp, tf, tr, vbc, 
                vbe, vbx, vce, vcs, vt, vtc, vte, vtn, xjrb, xjtf, xmc, xme, xms, xtf, capbx = 0, capcs = 0;
            bool icheck, ichk1;

            vt = BJTtemp * Circuit.CONSTKoverQ;

            gccs = 0;
            ceqcs = 0;
            geqbx = 0;
            ceqbx = 0;
            geqcb = 0;

            // dc model paramters
            csat = BJTtSatCur * BJTarea;
            rbpr = Model.BJTminBaseResist / BJTarea;
            rbpi = Model.BJTbaseResist / BJTarea - rbpr;
            gcpr = Model.BJTcollectorConduct * BJTarea;
            gepr = Model.BJTemitterConduct * BJTarea;
            oik = Model.BJTinvRollOffF / BJTarea;
            c2 = BJTtBEleakCur * BJTarea;
            vte = Model.BJTleakBEemissionCoeff * vt;
            oikr = Model.BJTinvRollOffR / BJTarea;
            c4 = BJTtBCleakCur * BJTarea;
            vtc = Model.BJTleakBCemissionCoeff * vt;
            td = Model.BJTexcessPhaseFactor;
            xjrb = Model.BJTbaseCurrentHalfResist * BJTarea;

            // initialization
            // I have to admit I don't really understand all these modes for initialization
            // Will look into this later
            icheck = true;
            if (state.UseSmallSignal)
            {
                vbe = state.States[0][BJTstate + BJTvbe];
                vbc = state.States[0][BJTstate + BJTvbc];
                vbx = Model.BJTtype * (
                    rstate.OldSolution[BJTbaseNode] -
                    rstate.OldSolution[BJTcolPrimeNode]);
                vcs = Model.BJTtype * (
                    rstate.OldSolution[BJTsubstNode] -
                    rstate.OldSolution[BJTcolPrimeNode]);
            }
            else if (state.Init == CircuitState.InitFlags.InitJct && state.UseIC)
            {
                vbe = Model.BJTtype * BJTicVBE;
                vce = Model.BJTtype * BJTicVCE;
                vbc = vbe - vce;
                vbx = vbc;
                vcs = 0;
            }
            else if (state.Init == CircuitState.InitFlags.InitJct && !BJToff)
            {
                vbe = BJTtVcrit;
                vbc = 0;
                vcs = vbx = 0;
            }
            else if (state.Init == CircuitState.InitFlags.InitJct ||
                (state.Init == CircuitState.InitFlags.InitFix && BJToff))
            {
                vbe = 0;
                vbc = 0;
                vcs = vbx = 0;
            }
            else
            {
                // compute new nonlinear branch voltages
                vbe = Model.BJTtype * (
                    rstate.OldSolution[BJTbasePrimeNode] -
                    rstate.OldSolution[BJTemitPrimeNode]);
                vbc = Model.BJTtype * (
                    rstate.OldSolution[BJTbasePrimeNode] -
                    rstate.OldSolution[BJTcolPrimeNode]);
                delvbe = vbe - state.States[0][BJTstate + BJTvbe];
                delvbc = vbc - state.States[0][BJTstate + BJTvbc];
                vbx = Model.BJTtype * (
                    rstate.OldSolution[BJTbaseNode] -
                    rstate.OldSolution[BJTcolPrimeNode]);
                vcs = Model.BJTtype * (
                    rstate.OldSolution[BJTsubstNode] -
                    rstate.OldSolution[BJTcolPrimeNode]);
                cchat = state.States[0][BJTstate + BJTcc] + (state.States[0][BJTstate + BJTgm] + state.States[0][BJTstate + BJTgo]) * delvbe -
                        (state.States[0][BJTstate + BJTgo] + state.States[0][BJTstate + BJTgmu]) * delvbc;
                cbhat = state.States[0][BJTstate + BJTcb] + state.States[0][BJTstate + BJTgpi] * delvbe + state.States[0][BJTstate + BJTgmu] *
                        delvbc;

                // limit nonlinear branch voltages
                ichk1 = true;
                vbe = Semiconductor.pnjlim(vbe, state.States[0][BJTstate + BJTvbe], vt,
                        BJTtVcrit, ref icheck);
                vbc = Semiconductor.pnjlim(vbc, state.States[0][BJTstate + BJTvbc], vt,
                        BJTtVcrit, ref ichk1);
                if (ichk1) icheck = true;
            }

            // determine dc current and derivatives 
            vtn = vt * Model.BJTemissionCoeffF;
            if (vbe > -5 * vtn)
            {
                evbe = Math.Exp(vbe / vtn);
                cbe = csat * (evbe - 1) + state.Gmin * vbe;
                gbe = csat * evbe / vtn + state.Gmin;
                if (c2 == 0)
                {
                    cben = 0;
                    gben = 0;
                }
                else
                {
                    evben = Math.Exp(vbe / vte);
                    cben = c2 * (evben - 1);
                    gben = c2 * evben / vte;
                }
            }
            else
            {
                gbe = -csat / vbe + state.Gmin;
                cbe = gbe * vbe;
                gben = -c2 / vbe;
                cben = gben * vbe;
            }
            vtn = vt * Model.BJTemissionCoeffR;
            if (vbc > -5 * vtn)
            {
                evbc = Math.Exp(vbc / vtn);
                cbc = csat * (evbc - 1) + state.Gmin * vbc;
                gbc = csat * evbc / vtn + state.Gmin;
                if (c4 == 0)
                {
                    cbcn = 0;
                    gbcn = 0;
                }
                else
                {
                    evbcn = Math.Exp(vbc / vtc);
                    cbcn = c4 * (evbcn - 1);
                    gbcn = c4 * evbcn / vtc;
                }
            }
            else
            {
                gbc = -csat / vbc + state.Gmin;
                cbc = gbc * vbc;
                gbcn = -c4 / vbc;
                cbcn = gbcn * vbc;
            }

            // determine base charge terms
            q1 = 1 / (1 - Model.BJTinvEarlyVoltF * vbc - Model.BJTinvEarlyVoltR * vbe);
            if (oik == 0 && oikr == 0)
            {
                qb = q1;
                dqbdve = q1 * qb * Model.BJTinvEarlyVoltR;
                dqbdvc = q1 * qb * Model.BJTinvEarlyVoltF;
            }
            else
            {
                q2 = oik * cbe + oikr * cbc;
                arg = Math.Max(0, 1 + 4 * q2);
                sqarg = 1;
                if (arg != 0) sqarg = Math.Sqrt(arg);
                qb = q1 * (1 + sqarg) / 2;
                dqbdve = q1 * (qb * Model.BJTinvEarlyVoltR + oik * gbe / sqarg);
                dqbdvc = q1 * (qb * Model.BJTinvEarlyVoltF + oikr * gbc / sqarg);
            }

            // weil's approx. for excess phase applied with backward-
            // euler integration
            cc = 0;
            cex = cbe;
            gex = gbe;
            if ((method != null) && td != 0)
            {
                arg1 = method.Delta / td;
                arg2 = 3 * arg1;
                arg1 = arg2 * arg1;
                denom = 1 + arg1 + arg2;
                arg3 = arg1 / denom;
                cc = (state.States[1][BJTstate + BJTcexbc] * (1 + method.Delta /
                        method.DeltaOld[1] + arg2) -
                        state.States[2][BJTstate + BJTcexbc] * method.Delta /
                        method.DeltaOld[1]) / denom;
                cex = cbe * arg3;
                gex = gbe * arg3;
                state.States[0][BJTstate + BJTcexbc] = cc + cex / qb;
            }

            // determine dc incremental conductances
            cc = cc + (cex - cbc) / qb - cbc / BJTtBetaR - cbcn;
            cb = cbe / BJTtBetaF + cben + cbc / BJTtBetaR + cbcn;
            gx = rbpr + rbpi / qb;
            if (xjrb != 0)
            {
                arg1 = Math.Max(cb / xjrb, 1e-9);
                arg2 = (-1 + Math.Sqrt(1 + 14.59025 * arg1)) / 2.4317 / Math.Sqrt(arg1);
                arg1 = Math.Tan(arg2);
                gx = rbpr + 3 * rbpi * (arg1 - arg2) / arg2 / arg1 / arg1;
            }
            if (gx != 0) gx = 1 / gx;
            gpi = gbe / BJTtBetaF + gben;
            gmu = gbc / BJTtBetaR + gbcn;
            go = (gbc + (cex - cbc) * dqbdvc / qb) / qb;
            gm = (gex - (cex - cbc) * dqbdve / qb) / qb - go;

            {
                // charge storage elements
                tf = Model.BJTtransitTimeF;
                tr = Model.BJTtransitTimeR;
                czbe = BJTtBEcap * BJTarea;
                pe = BJTtBEpot;
                xme = Model.BJTjunctionExpBE;
                cdis = Model.BJTbaseFractionBCcap;
                ctot = BJTtBCcap * BJTarea;
                czbc = ctot * cdis;
                czbx = ctot - czbc;
                pc = BJTtBCpot;
                xmc = Model.BJTjunctionExpBC;
                fcpe = BJTtDepCap;
                czcs = Model.BJTcapCS * BJTarea;
                ps = Model.BJTpotentialSubstrate;
                xms = Model.BJTexponentialSubstrate;
                xtf = Model.BJTtransitTimeBiasCoeffF;
                ovtf = Model.BJTtransitTimeVBCFactor;
                xjtf = Model.BJTtransitTimeHighCurrentF * BJTarea;
                if (tf != 0 && vbe > 0)
                {
                    argtf = 0;
                    arg2 = 0;
                    arg3 = 0;
                    if (xtf != 0)
                    {
                        argtf = xtf;
                        if (ovtf != 0)
                        {
                            argtf = argtf * Math.Exp(vbc * ovtf);
                        }
                        arg2 = argtf;
                        if (xjtf != 0)
                        {
                            temp = cbe / (cbe + xjtf);
                            argtf = argtf * temp * temp;
                            arg2 = argtf * (3 - temp - temp);
                        }
                        arg3 = cbe * argtf * ovtf;
                    }
                    cbe = cbe * (1 + argtf) / qb;
                    gbe = (gbe * (1 + arg2) - cbe * dqbdve) / qb;
                    geqcb = tf * (arg3 - cbe * dqbdvc) / qb;
                }
                if (vbe < fcpe)
                {
                    arg = 1 - vbe / pe;
                    sarg = Math.Exp(-xme * Math.Log(arg));
                    state.States[0][BJTstate + BJTqbe] = tf * cbe + pe * czbe *
                            (1 - arg * sarg) / (1 - xme);
                    capbe = tf * gbe + czbe * sarg;
                }
                else
                {
                    f1 = BJTtf1;
                    f2 = Model.BJTf2;
                    f3 = Model.BJTf3;
                    czbef2 = czbe / f2;
                    state.States[0][BJTstate + BJTqbe] = tf * cbe + czbe * f1 + czbef2 *
                            (f3 * (vbe - fcpe) + (xme / (pe + pe)) * (vbe * vbe - fcpe * fcpe));
                    capbe = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
                }
                fcpc = BJTtf4;
                f1 = BJTtf5;
                f2 = Model.BJTf6;
                f3 = Model.BJTf7;
                if (vbc < fcpc)
                {
                    arg = 1 - vbc / pc;
                    sarg = Math.Exp(-xmc * Math.Log(arg));
                    state.States[0][BJTstate + BJTqbc] = tr * cbc + pc * czbc * (
                            1 - arg * sarg) / (1 - xmc);
                    capbc = tr * gbc + czbc * sarg;
                }
                else
                {
                    czbcf2 = czbc / f2;
                    state.States[0][BJTstate + BJTqbc] = tr * cbc + czbc * f1 + czbcf2 *
                            (f3 * (vbc - fcpc) + (xmc / (pc + pc)) * (vbc * vbc - fcpc * fcpc));
                    capbc = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
                }
                if (vbx < fcpc)
                {
                    arg = 1 - vbx / pc;
                    sarg = Math.Exp(-xmc * Math.Log(arg));
                    state.States[0][BJTstate + BJTqbx] =
                        pc * czbx * (1 - arg * sarg) / (1 - xmc);
                    capbx = czbx * sarg;
                }
                else
                {
                    czbxf2 = czbx / f2;
                    state.States[0][BJTstate + BJTqbx] = czbx * f1 + czbxf2 *
                            (f3 * (vbx - fcpc) + (xmc / (pc + pc)) * (vbx * vbx - fcpc * fcpc));
                    capbx = czbxf2 * (f3 + xmc * vbx / pc);
                }
                if (vcs < 0)
                {
                    arg = 1 - vcs / ps;
                    sarg = Math.Exp(-xms * Math.Log(arg));
                    state.States[0][BJTstate + BJTqcs] = ps * czcs * (1 - arg * sarg) /
                            (1 - xms);
                    capcs = czcs * sarg;
                }
                else
                {
                    state.States[0][BJTstate + BJTqcs] = vcs * czcs * (1 + xms * vcs /
                            (2 * ps));
                    capcs = czcs * (1 + xms * vcs / ps);
                }
                BJTcapbe = capbe;
                BJTcapbc = capbc;
                BJTcapcs = capcs;
                BJTcapbx = capbx;

                // store small-signal parameters
                if (state.UseSmallSignal)
                {
                    state.States[0][BJTstate + BJTcqbe] = capbe;
                    state.States[0][BJTstate + BJTcqbc] = capbc;
                    state.States[0][BJTstate + BJTcqcs] = capcs;
                    state.States[0][BJTstate + BJTcqbx] = capbx;
                    state.States[0][BJTstate + BJTcexbc] = geqcb;
                    return; /* go to 1000 */
                }

                // transient analysis
                if (method != null)
                {
                    var result = method.Integrate(state, BJTstate + BJTqbe, capbe);
                    geq = result.Geq;
                    ceq = result.Ceq;
                    geqcb = geqcb * method.Slope;
                    gpi = gpi + geq;
                    cb = cb + state.States[0][BJTstate + BJTcqbe];
                    result = method.Integrate(state, BJTstate + BJTqbc, capbc);
                    geq = result.Geq;
                    ceq = result.Ceq;
                    gmu = gmu + geq;
                    cb = cb + state.States[0][BJTstate + BJTcqbc];
                    cc = cc - state.States[0][BJTstate + BJTcqbc];
                }
            }

            // check convergence
            if (state.Init != CircuitState.InitFlags.InitFix || (!BJToff))
            {
                if (icheck)
                    state.IsCon = false;
            }

            // charge storage for c-s and b-x junctions
            if (method != null)
            {
                var result = method.Integrate(state, BJTstate + BJTqcs, capcs);
                gccs = result.Geq;
                ceq = result.Ceq;
                result = method.Integrate(state, BJTstate + BJTqbx, capbx);
                geqbx = result.Geq;
                ceq = result.Ceq;
            }

            // next2:
            state.States[0][BJTstate + BJTvbe] = vbe;
            state.States[0][BJTstate + BJTvbc] = vbc;
            state.States[0][BJTstate + BJTcc] = cc;
            state.States[0][BJTstate + BJTcb] = cb;
            state.States[0][BJTstate + BJTgpi] = gpi;
            state.States[0][BJTstate + BJTgmu] = gmu;
            state.States[0][BJTstate + BJTgm] = gm;
            state.States[0][BJTstate + BJTgo] = go;
            state.States[0][BJTstate + BJTgx] = gx;
            state.States[0][BJTstate + BJTgeqcb] = geqcb;
            state.States[0][BJTstate + BJTgccs] = gccs;
            state.States[0][BJTstate + BJTgeqbx] = geqbx;

            // load current excitation vector
            ceqcs = Model.BJTtype * (state.States[0][BJTstate + BJTcqcs] -
                    vcs * gccs);
            ceqbx = Model.BJTtype * (state.States[0][BJTstate + BJTcqbx] -
                    vbx * geqbx);
            ceqbe = Model.BJTtype * (cc + cb - vbe * (gm + go + gpi) + vbc *
                    (go - geqcb));
            ceqbc = Model.BJTtype * (-cc + vbe * (gm + go) - vbc * (gmu + go));

            rstate.Rhs[BJTbaseNode] += (-ceqbx);
            rstate.Rhs[BJTcolPrimeNode] +=
                    (ceqcs + ceqbx + ceqbc);
            rstate.Rhs[BJTbasePrimeNode] +=
                    (-ceqbe - ceqbc);
            rstate.Rhs[BJTemitPrimeNode] += (ceqbe);
            rstate.Rhs[BJTsubstNode] += (-ceqcs);
            /*
             *  load y matrix
             */
            rstate.Matrix[BJTcolNode, BJTcolNode] += (gcpr);
            rstate.Matrix[BJTbaseNode, BJTbaseNode] += (gx + geqbx);
            rstate.Matrix[BJTemitNode, BJTemitNode] += (gepr);
            rstate.Matrix[BJTcolPrimeNode, BJTcolPrimeNode] += (gmu + go + gcpr + gccs + geqbx);
            rstate.Matrix[BJTbasePrimeNode, BJTbasePrimeNode] += (gx + gpi + gmu + geqcb);
            rstate.Matrix[BJTemitPrimeNode, BJTemitPrimeNode] += (gpi + gepr + gm + go);
            rstate.Matrix[BJTcolNode, BJTcolPrimeNode] += (-gcpr);
            rstate.Matrix[BJTbaseNode, BJTbasePrimeNode] += (-gx);
            rstate.Matrix[BJTemitNode, BJTemitPrimeNode] += (-gepr);
            rstate.Matrix[BJTcolPrimeNode, BJTcolNode] += (-gcpr);
            rstate.Matrix[BJTcolPrimeNode, BJTbasePrimeNode] += (-gmu + gm);
            rstate.Matrix[BJTcolPrimeNode, BJTemitPrimeNode] += (-gm - go);
            rstate.Matrix[BJTbasePrimeNode, BJTbaseNode] += (-gx);
            rstate.Matrix[BJTbasePrimeNode, BJTcolPrimeNode] += (-gmu - geqcb);
            rstate.Matrix[BJTbasePrimeNode, BJTemitPrimeNode] += (-gpi);
            rstate.Matrix[BJTemitPrimeNode, BJTemitNode] += (-gepr);
            rstate.Matrix[BJTemitPrimeNode, BJTcolPrimeNode] += (-go + geqcb);
            rstate.Matrix[BJTemitPrimeNode, BJTbasePrimeNode] += (-gpi - gm - geqcb);
            rstate.Matrix[BJTsubstNode, BJTsubstNode] += (gccs);
            rstate.Matrix[BJTcolPrimeNode, BJTsubstNode] += (-gccs);
            rstate.Matrix[BJTsubstNode, BJTcolPrimeNode] += (-gccs);
            rstate.Matrix[BJTbaseNode, BJTcolPrimeNode] += (-geqbx);
            rstate.Matrix[BJTcolPrimeNode, BJTbaseNode] += (-geqbx);
        }

        /// <summary>
        /// Load the bipolar transistor for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var state = ckt.State;
            var cstate = state.Complex;

            double gcpr, gepr;
            Complex gpi, gmu, gm;
            double go, td, gx;
            Complex xcbx, xccs, xcmcb;

            gcpr = Model.BJTcollectorConduct * BJTarea; // [EDIT] Changed model.BJTcollectorResist to model.BJTcollectorConduct
            gepr = Model.BJTemitterConduct * BJTarea; // [EDIT] Changed model.BJTemitterResist to model.BJTemitterConduct

            gpi = state.States[0][BJTstate + BJTgpi] + state.States[0][BJTstate + BJTcqbe] * cstate.Laplace;
            gmu = state.States[0][BJTstate + BJTgmu] + state.States[0][BJTstate + BJTcqbc] * cstate.Laplace;
            gm = state.States[0][BJTstate + BJTgm];
            go = state.States[0][BJTstate + BJTgo];
            td = Model.BJTexcessPhaseFactor;
            if (td != 0.0)
            {
                gm = gm + go;
                gm = gm * Complex.Exp(-cstate.Laplace) - go;
            }
            gx = state.States[0][BJTstate + BJTgx];
            xcbx = state.States[0][BJTstate + BJTcqbx] * cstate.Laplace;
            xccs = state.States[0][BJTstate + BJTcqcs] * cstate.Laplace;
            xcmcb = state.States[0][BJTstate + BJTcexbc] * cstate.Laplace;

            cstate.Matrix[BJTcolNode, BJTcolNode] += gcpr;
            cstate.Matrix[BJTbaseNode, BJTbaseNode] += gx + xcbx;
            cstate.Matrix[BJTemitNode, BJTemitNode] += gepr;
            cstate.Matrix[BJTcolPrimeNode, BJTcolPrimeNode] += gmu + go + gcpr + xcbx + xccs;
            cstate.Matrix[BJTbasePrimeNode, BJTbasePrimeNode] += gx + gpi + gmu + xcmcb;
            cstate.Matrix[BJTemitPrimeNode, BJTemitPrimeNode] += gpi + gepr + gm + go;
            cstate.Matrix[BJTcolNode, BJTcolPrimeNode] -= gcpr;
            cstate.Matrix[BJTbaseNode, BJTbasePrimeNode] -= gx;
            cstate.Matrix[BJTemitNode, BJTemitPrimeNode] -= gepr;
            cstate.Matrix[BJTcolNode, BJTcolPrimeNode] -= gcpr;
            cstate.Matrix[BJTcolPrimeNode, BJTbasePrimeNode] += -gmu + gm;
            cstate.Matrix[BJTcolPrimeNode, BJTemitPrimeNode] -= gm + go;
            cstate.Matrix[BJTbasePrimeNode, BJTbaseNode] -= gx;
            cstate.Matrix[BJTbasePrimeNode, BJTcolPrimeNode] -= gmu + xcmcb;
            cstate.Matrix[BJTbasePrimeNode, BJTemitPrimeNode] -= gpi;
            cstate.Matrix[BJTemitPrimeNode, BJTemitNode] -= gepr;
            cstate.Matrix[BJTemitPrimeNode, BJTcolPrimeNode] += -go + xcmcb;
            cstate.Matrix[BJTemitPrimeNode, BJTbasePrimeNode] -= gpi + gm + xcmcb;
            cstate.Matrix[BJTsubstNode, BJTsubstNode] += xccs;
            cstate.Matrix[BJTcolPrimeNode, BJTsubstNode] -= xccs;
            cstate.Matrix[BJTsubstNode, BJTcolPrimeNode] -= xccs;
            cstate.Matrix[BJTbaseNode, BJTcolPrimeNode] -= xcbx;
            cstate.Matrix[BJTcolPrimeNode, BJTbaseNode] -= xcbx;
        }
    }
}
