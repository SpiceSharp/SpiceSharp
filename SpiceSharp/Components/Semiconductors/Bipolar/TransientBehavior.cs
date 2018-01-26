using System;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="BJT"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        TemperatureBehavior temp;
        LoadBehavior load;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        int colNode, baseNode, emitNode, substNode, colPrimeNode, basePrimeNode, emitPrimeNode;
        protected MatrixElement ColColPrimePtr { get; private set; }
        protected MatrixElement BaseBasePrimePtr { get; private set; }
        protected MatrixElement EmitEmitPrimePtr { get; private set; }
        protected MatrixElement ColPrimeColPtr { get; private set; }
        protected MatrixElement ColPrimeBasePrimePtr { get; private set; }
        protected MatrixElement ColPrimeEmitPrimePtr { get; private set; }
        protected MatrixElement BasePrimeBasePtr { get; private set; }
        protected MatrixElement BasePrimeColPrimePtr { get; private set; }
        protected MatrixElement BasePrimeEmitPrimePtr { get; private set; }
        protected MatrixElement EmitPrimeEmitPtr { get; private set; }
        protected MatrixElement EmitPrimeColPrimePtr { get; private set; }
        protected MatrixElement EmitPrimeBasePrimePtr { get; private set; }
        protected MatrixElement ColColPtr { get; private set; }
        protected MatrixElement BaseBasePtr { get; private set; }
        protected MatrixElement EmitEmitPtr { get; private set; }
        protected MatrixElement ColPrimeColPrimePtr { get; private set; }
        protected MatrixElement BasePrimeBasePrimePtr { get; private set; }
        protected MatrixElement EmitPrimeEmitPrimePtr { get; private set; }
        protected MatrixElement SubstSubstPtr { get; private set; }
        protected MatrixElement ColPrimeSubstPtr { get; private set; }
        protected MatrixElement SubstColPrimePtr { get; private set; }
        protected MatrixElement BaseColPrimePtr { get; private set; }
        protected MatrixElement ColPrimeBasePtr { get; private set; }

        /*
        [SpiceName("qbe"), SpiceInfo("Charge storage B-E junction")]
        public double GetQBE(Circuit circuit) => circuit.State.States[0][State + Qbe];
        [SpiceName("cqbe"), SpiceInfo("Cap. due to charge storage in B-E jct.")]
        public double GetCQBE(Circuit circuit) => circuit.State.States[0][State + Cqbe];
        [SpiceName("qbc"), SpiceInfo("Charge storage B-C junction")]
        public double GetQBC(Circuit circuit) => circuit.State.States[0][State + Qbc];
        [SpiceName("cqbc"), SpiceInfo("Cap. due to charge storage in B-C jct.")]
        public double GetCQBC(Circuit circuit) => circuit.State.States[0][State + Cqbc];
        [SpiceName("qcs"), SpiceInfo("Charge storage C-S junction")]
        public double GetQCS(Circuit circuit) => circuit.State.States[0][State + Qcs];
        [SpiceName("cqcs"), SpiceInfo("Cap. due to charge storage in C-S jct.")]
        public double GetCQCS(Circuit circuit) => circuit.State.States[0][State + Cqcs];
        [SpiceName("qbx"), SpiceInfo("Charge storage B-X junction")]
        public double GetQBX(Circuit circuit) => circuit.State.States[0][State + Qbx];
        [SpiceName("cqbx"), SpiceInfo("Cap. due to charge storage in B-X jct.")]
        public double GetCQBX(Circuit circuit) => circuit.State.States[0][State + Cqbx];
        [SpiceName("cexbc"), SpiceInfo("Total Capacitance in B-X junction")]
        public double GetCEXBC(Circuit circuit) => circuit.State.States[0][State + Cexbc];
        */

        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("cpi"), PropertyInfo("Internal base to emitter capactance")]
        public double Capbe { get; internal set; }
        [PropertyName("cmu"), PropertyInfo("Internal base to collector capactiance")]
        public double Capbc { get; internal set; }
        [PropertyName("cbx"), PropertyInfo("Base to collector capacitance")]
        public double Capbx { get; internal set; }
        [PropertyName("ccs"), PropertyInfo("Collector to substrate capacitance")]
        public double Capcs { get; internal set; }

        /// <summary>
        /// States
        /// </summary>
        protected StateDerivative Qbe { get; private set; }
        protected StateDerivative Qbc { get; private set; }
        protected StateDerivative Qcs { get; private set; }
        protected StateDerivative Qbx { get; private set; }
        protected StateHistory Cexbc { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>(0);
            load = provider.GetBehavior<LoadBehavior>(0);
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new Diagnostics.CircuitException($"Pin count mismatch: 4 pins expected, {pins.Length} given");
            colNode = pins[0];
            baseNode = pins[1];
            emitNode = pins[2];
            substNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            // Get extra equations
            colPrimeNode = load.ColPrimeNode;
            basePrimeNode = load.BasePrimeNode;
            emitPrimeNode = load.EmitPrimeNode;

            // Get matrix pointers
            ColColPrimePtr = matrix.GetElement(colNode, colPrimeNode);
            BaseBasePrimePtr = matrix.GetElement(baseNode, basePrimeNode);
            EmitEmitPrimePtr = matrix.GetElement(emitNode, emitPrimeNode);
            ColPrimeColPtr = matrix.GetElement(colPrimeNode, colNode);
            ColPrimeBasePrimePtr = matrix.GetElement(colPrimeNode, basePrimeNode);
            ColPrimeEmitPrimePtr = matrix.GetElement(colPrimeNode, emitPrimeNode);
            BasePrimeBasePtr = matrix.GetElement(basePrimeNode, baseNode);
            BasePrimeColPrimePtr = matrix.GetElement(basePrimeNode, colPrimeNode);
            BasePrimeEmitPrimePtr = matrix.GetElement(basePrimeNode, emitPrimeNode);
            EmitPrimeEmitPtr = matrix.GetElement(emitPrimeNode, emitNode);
            EmitPrimeColPrimePtr = matrix.GetElement(emitPrimeNode, colPrimeNode);
            EmitPrimeBasePrimePtr = matrix.GetElement(emitPrimeNode, basePrimeNode);
            ColColPtr = matrix.GetElement(colNode, colNode);
            BaseBasePtr = matrix.GetElement(baseNode, baseNode);
            EmitEmitPtr = matrix.GetElement(emitNode, emitNode);
            ColPrimeColPrimePtr = matrix.GetElement(colPrimeNode, colPrimeNode);
            BasePrimeBasePrimePtr = matrix.GetElement(basePrimeNode, basePrimeNode);
            EmitPrimeEmitPrimePtr = matrix.GetElement(emitPrimeNode, emitPrimeNode);
            SubstSubstPtr = matrix.GetElement(substNode, substNode);
            ColPrimeSubstPtr = matrix.GetElement(colPrimeNode, substNode);
            SubstColPrimePtr = matrix.GetElement(substNode, colPrimeNode);
            BaseColPrimePtr = matrix.GetElement(baseNode, colPrimeNode);
            ColPrimeBasePtr = matrix.GetElement(colPrimeNode, baseNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            ColColPrimePtr = null;
            BaseBasePrimePtr = null;
            EmitEmitPrimePtr = null;
            ColPrimeColPtr = null;
            ColPrimeBasePrimePtr = null;
            ColPrimeEmitPrimePtr = null;
            BasePrimeBasePtr = null;
            BasePrimeColPrimePtr = null;
            BasePrimeEmitPrimePtr = null;
            EmitPrimeEmitPtr = null;
            EmitPrimeColPrimePtr = null;
            EmitPrimeBasePrimePtr = null;
            ColColPtr = null;
            BaseBasePtr = null;
            EmitEmitPtr = null;
            ColPrimeColPrimePtr = null;
            BasePrimeBasePrimePtr = null;
            EmitPrimeEmitPrimePtr = null;
            SubstSubstPtr = null;
            ColPrimeSubstPtr = null;
            SubstColPrimePtr = null;
            BaseColPrimePtr = null;
            ColPrimeBasePtr = null;
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">Pool of all states</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            // We just need a history without integration here
            Qbe = states.Create();
            Qbc = states.Create();
            Qcs = states.Create();
            Qbx = states.Create();
            Cexbc = states.CreateHistory();
        }

        /// <summary>
        /// Calculate state variables
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void GetDCstate(TimeSimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            double tf, tr, czbe, pe, xme, cdis, ctot, czbc, czbx, pc, xmc, fcpe, czcs, ps, arg, arg2, arg3,
                xms, xtf, ovtf, xjtf, argtf, tmp, sarg, f1, f2, f3, czbef2, fcpc, czbcf2, czbxf2;

            double cbe = load.Cbe;
            double cbc = load.Cbc;
            double gbe = load.Gbe;
            double gbc = load.Gbc;
            double qb = load.Qb;
            double dqbdvc = load.DqbDvc;
            double dqbdve = load.DqbDve;
            double geqcb = 0;

            double vbe = load.Vbe;
            double vbc = load.Vbc;
            double vbx = mbp.Type * (state.Solution[baseNode] - state.Solution[colPrimeNode]);
            double vcs = mbp.Type * (state.Solution[substNode] - state.Solution[colPrimeNode]);
            double td = modeltemp.ExcessPhaseFactor;

            Cexbc.Value = load.Cbe / load.Qb;

            /* 
             * charge storage elements
             */
            tf = mbp.TransitTimeF;
            tr = mbp.TransitTimeR;
            czbe = temp.TBEcap * bp.Area;
            pe = temp.TBEpot;
            xme = mbp.JunctionExpBE;
            cdis = mbp.BaseFractionBCcap;
            ctot = temp.TBCcap * bp.Area;
            czbc = ctot * cdis;
            czbx = ctot - czbc;
            pc = temp.TBCpot;
            xmc = mbp.JunctionExpBC;
            fcpe = temp.TDepCap;
            czcs = mbp.CapCS * bp.Area;
            ps = mbp.PotentialSubstrate;
            xms = mbp.ExponentialSubstrate;
            xtf = mbp.TransitTimeBiasCoeffF;
            ovtf = modeltemp.TransitTimeVBCFactor;
            xjtf = mbp.TransitTimeHighCurrentF * bp.Area;
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
                        tmp = cbe / (cbe + xjtf);
                        argtf = argtf * tmp * tmp;
                        arg2 = argtf * (3 - tmp - tmp);
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
                Qbe.Value = tf * cbe + pe * czbe * (1 - arg * sarg) / (1 - xme);
            }
            else
            {
                f1 = temp.Tf1;
                f2 = modeltemp.F2;
                f3 = modeltemp.F3;
                czbef2 = czbe / f2;
                Qbe.Value = tf * cbe + czbe * f1 + czbef2 * (f3 * (vbe - fcpe) + (xme / (pe + pe)) * (vbe * vbe -
                     fcpe * fcpe));
            }
            fcpc = temp.Tf4;
            f1 = temp.Tf5;
            f2 = modeltemp.F6;
            f3 = modeltemp.F7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                Qbc.Value = tr * cbc + pc * czbc * (1 - arg * sarg) / (1 - xmc);
            }
            else
            {
                czbcf2 = czbc / f2;
                Qbc.Value = tr * cbc + czbc * f1 + czbcf2 * (f3 * (vbc - fcpc) + (xmc / (pc + pc)) * (vbc * vbc -
                     fcpc * fcpc));
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                Qbx.Value = pc * czbx * (1 - arg * sarg) / (1 - xmc);
            }
            else
            {
                czbxf2 = czbx / f2;
                Qbx.Value = czbx * f1 + czbxf2 * (f3 * (vbx - fcpc) + (xmc / (pc + pc)) * (vbx * vbx - fcpc * fcpc));
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                Qcs.Value = ps * czcs * (1 - arg * sarg) / (1 - xms);
            }
            else
            {
                Qcs.Value = vcs * czcs * (1 + xms * vcs / (2 * ps));
            }

            // Register for excess phase calculations
            if (modeltemp.ExcessPhaseFactor > 0.0)
            {
                load.ExcessPhaseCalculation += CalculateExcessPhase;
            }
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Transient(TimeSimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            double tf, tr, czbe, pe, xme, cdis, ctot, czbc, czbx, pc, xmc, fcpe, czcs, ps, arg, arg2, arg3, capbx, capcs,
                xms, xtf, ovtf, xjtf, argtf, tmp, sarg, capbe, f1, f2, f3, czbef2, fcpc, capbc, czbcf2, czbxf2;
            

            double cbe = load.Cbe;
            double cbc = load.Cbc;
            double gbe = load.Gbe;
            double gbc = load.Gbc;
            double qb = load.Qb;
            double dqbdvc = load.DqbDvc;
            double dqbdve = load.DqbDve;
            double geqcb = 0;

            double gpi = 0.0;
            double gmu = 0.0;
            double cb = 0.0;
            double cc = 0.0;

            double vbe = load.Vbe;
            double vbc = load.Vbc;
            double vbx = mbp.Type * (state.Solution[baseNode] - state.Solution[colPrimeNode]);
            double vcs = mbp.Type * (state.Solution[substNode] - state.Solution[colPrimeNode]);
            double td = modeltemp.ExcessPhaseFactor;

            /* 
             * charge storage elements
             */
            tf = mbp.TransitTimeF;
            tr = mbp.TransitTimeR;
            czbe = temp.TBEcap * bp.Area;
            pe = temp.TBEpot;
            xme = mbp.JunctionExpBE;
            cdis = mbp.BaseFractionBCcap;
            ctot = temp.TBCcap * bp.Area;
            czbc = ctot * cdis;
            czbx = ctot - czbc;
            pc = temp.TBCpot;
            xmc = mbp.JunctionExpBC;
            fcpe = temp.TDepCap;
            czcs = mbp.CapCS * bp.Area;
            ps = mbp.PotentialSubstrate;
            xms = mbp.ExponentialSubstrate;
            xtf = mbp.TransitTimeBiasCoeffF;
            ovtf = modeltemp.TransitTimeVBCFactor;
            xjtf = mbp.TransitTimeHighCurrentF * bp.Area;
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
                        tmp = cbe / (cbe + xjtf);
                        argtf = argtf * tmp * tmp;
                        arg2 = argtf * (3 - tmp - tmp);
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
                Qbe.Value = tf * cbe + pe * czbe * (1 - arg * sarg) / (1 - xme);
                capbe = tf * gbe + czbe * sarg;
            }
            else
            {
                f1 = temp.Tf1;
                f2 = modeltemp.F2;
                f3 = modeltemp.F3;
                czbef2 = czbe / f2;
                Qbe.Value = tf * cbe + czbe * f1 + czbef2 * (f3 * (vbe - fcpe) + (xme / (pe + pe)) * (vbe * vbe -
                     fcpe * fcpe));
                capbe = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
            }
            fcpc = temp.Tf4;
            f1 = temp.Tf5;
            f2 = modeltemp.F6;
            f3 = modeltemp.F7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                Qbc.Value = tr * cbc + pc * czbc * (1 - arg * sarg) / (1 - xmc);
                capbc = tr * gbc + czbc * sarg;
            }
            else
            {
                czbcf2 = czbc / f2;
                Qbc.Value = tr * cbc + czbc * f1 + czbcf2 * (f3 * (vbc - fcpc) + (xmc / (pc + pc)) * (vbc * vbc -
                     fcpc * fcpc));
                capbc = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                Qbx.Value = pc * czbx * (1 - arg * sarg) / (1 - xmc);
                capbx = czbx * sarg;
            }
            else
            {
                czbxf2 = czbx / f2;
                Qbx.Value = czbx * f1 + czbxf2 * (f3 * (vbx - fcpc) + (xmc / (pc + pc)) * (vbx * vbx - fcpc * fcpc));
                capbx = czbxf2 * (f3 + xmc * vbx / pc);
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                Qcs.Value = ps * czcs * (1 - arg * sarg) / (1 - xms);
                capcs = czcs * sarg;
            }
            else
            {
                Qcs.Value = vcs * czcs * (1 + xms * vcs / (2 * ps));
                capcs = czcs * (1 + xms * vcs / ps);
            }

            Capbe = capbe;
            Capbc = capbc;
            Capcs = capcs;
            Capbx = capbx;

            Qbe.Integrate();
            geqcb = Qbe.Jacobian(geqcb); // Multiplies geqcb with method.Slope (ag[0])
            gpi += Qbe.Jacobian(capbe);
            cb += Qbe.Derivative;
            Qbc.Integrate();
            gmu += Qbc.Jacobian(capbc);
            cb += Qbc.Derivative;
            cc -= Qbc.Derivative;

            /* 
             * charge storage for c - s and b - x junctions
             */
            Qcs.Integrate();
            double gccs = Qcs.Jacobian(capcs);
            Qbx.Integrate();
            double geqbx = Qbx.Jacobian(capbx);

            /* 
			 * load current excitation vector
			 */
            double ceqcs = mbp.Type * (Qcs.Derivative - vcs * gccs);
            double ceqbx = mbp.Type * (Qbx.Derivative - vbx * geqbx);
            double ceqbe = mbp.Type * (cc + cb - vbe * gpi + vbc * (-geqcb));
            double ceqbc = mbp.Type * (-cc + - vbc * gmu);
            
            state.Rhs[baseNode] += (-ceqbx);
            state.Rhs[colPrimeNode] += (ceqcs + ceqbx + ceqbc);
            state.Rhs[basePrimeNode] += -ceqbe - ceqbc;
            state.Rhs[emitPrimeNode] += (ceqbe);
            state.Rhs[substNode] += (-ceqcs);

            /* 
			 * load y matrix
			 */
            BaseBasePtr.Add(geqbx);
            ColPrimeColPrimePtr.Add(gmu + gccs + geqbx);
            BasePrimeBasePrimePtr.Add(gpi + gmu + geqcb);
            EmitPrimeEmitPrimePtr.Add(gpi);
            ColPrimeBasePrimePtr.Add(-gmu);
            BasePrimeColPrimePtr.Add(-gmu - geqcb);
            BasePrimeEmitPrimePtr.Add(-gpi);
            EmitPrimeColPrimePtr.Add(geqcb);
            EmitPrimeBasePrimePtr.Add(-gpi - geqcb);
            SubstSubstPtr.Add(gccs);
            ColPrimeSubstPtr.Add(-gccs);
            SubstColPrimePtr.Add(-gccs);
            BaseColPrimePtr.Add(-geqbx);
            ColPrimeBasePtr.Add(-geqbx);
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="timestep">Current timestep</param>
        public override void Truncate(ref double timestep)
        {
            Qbe.LocalTruncationError(ref timestep);
            Qbc.LocalTruncationError(ref timestep);
            Qcs.LocalTruncationError(ref timestep);
        }

        /// <summary>
        /// Calculate excess phase
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        public void CalculateExcessPhase(object sender, ExcessPhaseEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            double arg1, arg2, denom, arg3;
            double td = modeltemp.ExcessPhaseFactor;
            if (td == 0.0)
                return;
            
            /* 
             * weil's approx. for excess phase applied with backward - 
             * euler integration
             */
            double cbe = args.cex;
            double gbe = args.gex;

            double delta = Cexbc.GetTimestep(0);
            double prevdelta = Cexbc.GetTimestep(1);
            arg1 = delta / td;
            arg2 = 3 * arg1;
            arg1 = arg2 * arg1;
            denom = 1 + arg1 + arg2;
            arg3 = arg1 / denom;
            /* Still need a place for this...
            if (state.Init == State.InitFlags.InitTransient)
            {
                state.States[1][State + Cexbc] = cbe / qb;
                state.States[2][State + Cexbc] = state.States[1][State + Cexbc];
            } */
            args.cc = (Cexbc.GetPreviousValue(1) * (1 + delta / prevdelta + arg2) 
                - Cexbc.GetPreviousValue(2) * delta / prevdelta) / denom;
            args.cex = cbe * arg3;
            args.gex = gbe * arg3;
            Cexbc.Value = args.cc + args.cex / args.qb;
        }
    }
}
