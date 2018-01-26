using System;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// General behavior for <see cref="BJT"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        TemperatureBehavior temp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("vbe"), PropertyInfo("B-E voltage")]
        public double Vbe { get; protected set; }
        [PropertyName("vbc"), PropertyInfo("B-C voltage")]
        public double Vbc { get; protected set; }
        [PropertyName("cc"), PropertyInfo("Current at collector node")]
        public double Cc { get; protected set; }
        [PropertyName("cb"), PropertyInfo("Current at base node")]
        public double Cb { get; protected set; }
        [PropertyName("gpi"), PropertyInfo("Small signal input conductance - pi")]
        public double Gpi { get; protected set; }
        [PropertyName("gmu"), PropertyInfo("Small signal conductance - mu")]
        public double Gmu { get; protected set; }
        [PropertyName("gm"), PropertyInfo("Small signal transconductance")]
        public double Gm { get; protected set; }
        [PropertyName("go"), PropertyInfo("Small signal output conductance")]
        public double Go { get; protected set; }
        public double Gx { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int colNode, baseNode, emitNode, substNode;
        public int ColPrimeNode { get; private set; }
        public int BasePrimeNode { get; private set; }
        public int EmitPrimeNode { get; private set; }
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

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double Cbe { get; protected set; }
        public double Gbe { get; protected set; }
        public double Cbc { get; protected set; }
        public double Gbc { get; protected set; }
        public double Qb { get; protected set; }
        public double DqbDvc { get; protected set; }
        public double DqbDve { get; protected set; }

        /// <summary>
        /// Event called when excess phase calculation is needed
        /// </summary>
        public event ExcessPhaseEventHandler ExcessPhaseCalculation;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

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
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            // Add a series collector node if necessary
            if (mbp.CollectorResist.Value > 0)
                ColPrimeNode = nodes.Create(Name.Grow("#col")).Index;
            else
                ColPrimeNode = colNode;

            // Add a series base node if necessary
            if (mbp.BaseResist.Value > 0)
                BasePrimeNode = nodes.Create(Name.Grow("#base")).Index;
            else
                BasePrimeNode = baseNode;

            // Add a series emitter node if necessary
            if (mbp.EmitterResist.Value > 0)
                EmitPrimeNode = nodes.Create(Name.Grow("#emit")).Index;
            else
                EmitPrimeNode = emitNode;

            // Get matrix pointers
            ColColPrimePtr = matrix.GetElement(colNode, ColPrimeNode);
            BaseBasePrimePtr = matrix.GetElement(baseNode, BasePrimeNode);
            EmitEmitPrimePtr = matrix.GetElement(emitNode, EmitPrimeNode);
            ColPrimeColPtr = matrix.GetElement(ColPrimeNode, colNode);
            ColPrimeBasePrimePtr = matrix.GetElement(ColPrimeNode, BasePrimeNode);
            ColPrimeEmitPrimePtr = matrix.GetElement(ColPrimeNode, EmitPrimeNode);
            BasePrimeBasePtr = matrix.GetElement(BasePrimeNode, baseNode);
            BasePrimeColPrimePtr = matrix.GetElement(BasePrimeNode, ColPrimeNode);
            BasePrimeEmitPrimePtr = matrix.GetElement(BasePrimeNode, EmitPrimeNode);
            EmitPrimeEmitPtr = matrix.GetElement(EmitPrimeNode, emitNode);
            EmitPrimeColPrimePtr = matrix.GetElement(EmitPrimeNode, ColPrimeNode);
            EmitPrimeBasePrimePtr = matrix.GetElement(EmitPrimeNode, BasePrimeNode);
            ColColPtr = matrix.GetElement(colNode, colNode);
            BaseBasePtr = matrix.GetElement(baseNode, baseNode);
            EmitEmitPtr = matrix.GetElement(emitNode, emitNode);
            ColPrimeColPrimePtr = matrix.GetElement(ColPrimeNode, ColPrimeNode);
            BasePrimeBasePrimePtr = matrix.GetElement(BasePrimeNode, BasePrimeNode);
            EmitPrimeEmitPrimePtr = matrix.GetElement(EmitPrimeNode, EmitPrimeNode);
            SubstSubstPtr = matrix.GetElement(substNode, substNode);
            ColPrimeSubstPtr = matrix.GetElement(ColPrimeNode, substNode);
            SubstColPrimePtr = matrix.GetElement(substNode, ColPrimeNode);
            BaseColPrimePtr = matrix.GetElement(baseNode, ColPrimeNode);
            ColPrimeBasePtr = matrix.GetElement(ColPrimeNode, baseNode);
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
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var state = simulation.State;
            double vt;
            double ceqcs, ceqbx, csat, rbpr, rbpi, gcpr, gepr, oik, c2, vte, oikr, c4, vtc, xjrb, vbe, vbc, vbx, vcs;
            bool icheck;
            double vce;
            bool ichk1;
            double vtn, evbe, gben, evben, cben, evbc, gbcn, evbcn, cbcn, q1, q2, arg, sqarg, cc, cex,
                gex, arg1, arg2, cb, gx, gpi, gmu, go, gm;
            double ceqbe, ceqbc;

            vt = bp.Temperature * Circuit.KOverQ;

            ceqcs = 0;
            ceqbx = 0;

            /* 
			 * dc model paramters
			 */
            csat = temp.TSatCur * bp.Area;
            rbpr = mbp.MinBaseResist / bp.Area;
            rbpi = mbp.BaseResist / bp.Area - rbpr;
            gcpr = modeltemp.CollectorConduct * bp.Area;
            gepr = modeltemp.EmitterConduct * bp.Area;
            oik = modeltemp.InvRollOffF / bp.Area;
            c2 = temp.TBEleakCur * bp.Area;
            vte = mbp.LeakBEemissionCoeff * vt;
            oikr = modeltemp.InvRollOffR / bp.Area;
            c4 = temp.TBCleakCur * bp.Area;
            vtc = mbp.LeakBCemissionCoeff * vt;
            xjrb = mbp.BaseCurrentHalfResist * bp.Area;

            /* 
			* initialization
			*/
            icheck = false;
            if (state.Init == State.InitFlags.InitJct && state.Domain == State.DomainTypes.Time && state.UseDC && state.UseIC)
            {
                vbe = mbp.Type * bp.InitialVBE;
                vce = mbp.Type * bp.InitialVCE;
                vbc = vbe - vce;
                vbx = vbc;
                vcs = 0;
            }
            else if (state.Init == State.InitFlags.InitJct && !bp.Off)
            {
                vbe = temp.TVcrit;
                vbc = 0;
                vcs = 0;
                vbx = 0;
            }
            else if (state.Init == State.InitFlags.InitJct || (state.Init == State.InitFlags.InitFix && bp.Off))
            {
                vbe = 0;
                vbc = 0;
                vcs = 0;
                vbx = 0;
            }
            else
            {
                /* 
                 * compute new nonlinear branch voltages
                 */
                vbe = mbp.Type * (state.Solution[BasePrimeNode] - state.Solution[EmitPrimeNode]);
                vbc = mbp.Type * (state.Solution[BasePrimeNode] - state.Solution[ColPrimeNode]);
                vbx = mbp.Type * (state.Solution[baseNode] - state.Solution[ColPrimeNode]);
                vcs = mbp.Type * (state.Solution[substNode] - state.Solution[ColPrimeNode]);

                /* 
				 * limit nonlinear branch voltages
				 */
                ichk1 = true;
                vbe = Semiconductor.DEVpnjlim(vbe, Vbe, vt, temp.TVcrit, ref icheck);
                vbc = Semiconductor.DEVpnjlim(vbc, Vbc, vt, temp.TVcrit, ref ichk1);
                if (ichk1 == true)
                    icheck = true;
            }

            /* 
			 * determine dc current and derivitives
			 */
            vtn = vt * mbp.EmissionCoeffF;
            if (vbe > -5 * vtn)
            {
                evbe = Math.Exp(vbe / vtn);
                Cbe = csat * (evbe - 1) + state.Gmin * vbe;
                Gbe = csat * evbe / vtn + state.Gmin;
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
                Gbe = -csat / vbe + state.Gmin;
                Cbe = Gbe * vbe;
                gben = -c2 / vbe;
                cben = gben * vbe;
            }

            vtn = vt * mbp.EmissionCoeffR;
            if (vbc > -5 * vtn)
            {
                evbc = Math.Exp(vbc / vtn);
                Cbc = csat * (evbc - 1) + state.Gmin * vbc;
                Gbc = csat * evbc / vtn + state.Gmin;
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
                Gbc = -csat / vbc + state.Gmin;
                Cbc = Gbc * vbc;
                gbcn = -c4 / vbc;
                cbcn = gbcn * vbc;
            }

            /* 
			 * determine base charge terms
			 */
            q1 = 1 / (1 - modeltemp.InvEarlyVoltF * vbc - modeltemp.InvEarlyVoltR * vbe);
            if (oik == 0 && oikr == 0)
            {
                Qb = q1;
                DqbDve = q1 * Qb * modeltemp.InvEarlyVoltR;
                DqbDvc = q1 * Qb * modeltemp.InvEarlyVoltF;
            }
            else
            {
                q2 = oik * Cbe + oikr * Cbc;
                arg = Math.Max(0, 1 + 4 * q2);
                sqarg = 1;
                if (arg != 0)
                    sqarg = Math.Sqrt(arg);
                Qb = q1 * (1 + sqarg) / 2;
                DqbDve = q1 * (Qb * modeltemp.InvEarlyVoltR + oik * Gbe / sqarg);
                DqbDvc = q1 * (Qb * modeltemp.InvEarlyVoltF + oikr * Gbc / sqarg);
            }

            // Excess phase calculation
            ExcessPhaseEventArgs ep = new ExcessPhaseEventArgs()
            {
                cc = 0.0,
                cex = Cbe,
                gex = Gbe,
                qb = Qb
            };
            ExcessPhaseCalculation?.Invoke(this, ep);
            cc = ep.cc;
            cex = ep.cex;
            gex = ep.gex;

            /* 
			 * determine dc incremental conductances
			 */
            cc = cc + (cex - Cbc) / Qb - Cbc / temp.TBetaR - cbcn;
            cb = Cbe / temp.TBetaF + cben + Cbc / temp.TBetaR + cbcn;
            gx = rbpr + rbpi / Qb;
            if (xjrb != 0)
            {
                arg1 = Math.Max(cb / xjrb, 1e-9);
                arg2 = (-1 + Math.Sqrt(1 + 14.59025 * arg1)) / 2.4317 / Math.Sqrt(arg1);
                arg1 = Math.Tan(arg2);
                gx = rbpr + 3 * rbpi * (arg1 - arg2) / arg2 / arg1 / arg1;
            }
            if (gx != 0)
                gx = 1 / gx;
            gpi = Gbe / temp.TBetaF + gben;
            gmu = Gbc / temp.TBetaR + gbcn;
            go = (Gbc + (cex - Cbc) * DqbDvc / Qb) / Qb;
            gm = (gex - (cex - Cbc) * DqbDve / Qb) / Qb - go;

            /* 
			 * check convergence
			 */
            if (state.Init != State.InitFlags.InitFix || !bp.Off)
            {
                if (icheck)
                    state.IsCon = false;
            }

            Vbe = vbe;
            Vbc = vbc;
            Cc = cc;
            Cb = cb;
            Gpi = gpi;
            Gmu = gmu;
            Gm = gm;
            Go = go;
            Gx = gx;

            /* 
			 * load current excitation vector
			 */
            ceqbe = mbp.Type * (cc + cb - vbe * (gm + go + gpi) + vbc * go);
            ceqbc = mbp.Type * (-cc + vbe * (gm + go) - vbc * (gmu + go));

            state.Rhs[baseNode] += (-ceqbx);
            state.Rhs[ColPrimeNode] += (ceqcs + ceqbx + ceqbc);
            state.Rhs[BasePrimeNode] += (-ceqbe - ceqbc);
            state.Rhs[EmitPrimeNode] += (ceqbe);
            state.Rhs[substNode] += (-ceqcs);

            /* 
			 * load y matrix
			 */
            ColColPtr.Add(gcpr);
            BaseBasePtr.Add(gx);
            EmitEmitPtr.Add(gepr);
            ColPrimeColPrimePtr.Add(gmu + go + gcpr);
            BasePrimeBasePrimePtr.Add(gx + gpi + gmu);
            EmitPrimeEmitPrimePtr.Add(gpi + gepr + gm + go);
            ColColPrimePtr.Add(-gcpr);
            BaseBasePrimePtr.Add(-gx);
            EmitEmitPrimePtr.Add(-gepr);
            ColPrimeColPtr.Add(-gcpr);
            ColPrimeBasePrimePtr.Add(-gmu + gm);
            ColPrimeEmitPrimePtr.Add(-gm - go);
            BasePrimeBasePtr.Add(-gx);
            BasePrimeColPrimePtr.Add(-gmu);
            BasePrimeEmitPrimePtr.Add(-gpi);
            EmitPrimeEmitPtr.Add(-gepr);
            EmitPrimeColPrimePtr.Add(-go);
            EmitPrimeBasePrimePtr.Add(-gpi - gm);
        }

        /// <summary>
        /// Check if the BJT is convergent
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        /// <returns></returns>
        public override bool IsConvergent(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.State;
            var config = simulation.BaseConfiguration;

            double vbe, vbc, delvbe, delvbc, cchat, cbhat, cc, cb;

            vbe = mbp.Type * (state.Solution[BasePrimeNode] - state.Solution[EmitPrimeNode]);
            vbc = mbp.Type * (state.Solution[BasePrimeNode] - state.Solution[ColPrimeNode]);
            delvbe = vbe - Vbe;
            delvbc = vbc - Vbe;
            cchat = Cc + (Gm + Go) * delvbe - (Go + Gmu) * delvbc;
            cbhat = Cb + Gpi * delvbe + Gmu * delvbc;
            cc = Cc;
            cb = Cb;

            /*
             *   check convergence
             */
            // NOTE: access configuration in some way here!
            double tol = config.RelTol * Math.Max(Math.Abs(cchat), Math.Abs(cc)) + config.AbsTol;
            if (Math.Abs(cchat - cc) > tol)
            {
                state.IsCon = false;
                return false;
            }

            tol = config.RelTol * Math.Max(Math.Abs(cbhat), Math.Abs(cb)) + config.AbsTol;
            if (Math.Abs(cbhat - cb) > tol)
            {
                state.IsCon = false;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Delegate for excess phase calculation
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="args">Arguments</param>
    public delegate void ExcessPhaseEventHandler(object sender, ExcessPhaseEventArgs args);
}
