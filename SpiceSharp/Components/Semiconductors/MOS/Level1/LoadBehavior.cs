using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// General behavior for a <see cref="Mosfet1"/>
    /// </summary>
    public class LoadBehavior : Common.LoadBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private TemperatureBehavior _temp;
        private ModelBaseParameters _mbp;
        private BaseConfiguration _baseConfig;

        /// <summary>
        /// Shared parameters
        /// </summary>
        [ParameterName("von"), ParameterInfo("Turn-on voltage")]
        public double Von { get; protected set; }
        [ParameterName("vdsat"), ParameterInfo("Saturation drain voltage")]
        public double SaturationVoltageDs { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            base.Setup(simulation, provider);

            // Get configurations
            _baseConfig = simulation.Configurations.Get<BaseConfiguration>();

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>();

            // Reset
            SaturationVoltageDs = 0;
            Von = 0;
            Mode = 1;
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        /// <param name="simulation"></param>
        public override void Unsetup(Simulation simulation)
        {
            _baseConfig = null;
            _bp = null;
            _mbp = null;
            _temp = null;

            base.Unsetup(simulation);
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double drainSatCur, sourceSatCur,
                vgs, vds, vbs, vbd, vgd;
            double von;
            double vdsat, cdrain = 0.0, cdreq;
            int xnrm, xrev;

            var vt = Circuit.KOverQ * _bp.Temperature;
            var check = 1;

            var effectiveLength = _bp.Length - 2 * _mbp.LateralDiffusion;
            if (_temp.TempSaturationCurrentDensity.Equals(0) || _bp.DrainArea.Value <= 0 || _bp.SourceArea.Value <= 0)
            {
                drainSatCur = _temp.TempSaturationCurrent;
                sourceSatCur = _temp.TempSaturationCurrent;
            }
            else
            {
                drainSatCur = _temp.TempSaturationCurrentDensity * _bp.DrainArea;
                sourceSatCur = _temp.TempSaturationCurrentDensity * _bp.SourceArea;
            }

            var beta = _temp.TempTransconductance * _bp.Width / effectiveLength;

            if (state.Init == InitializationModes.Float || (simulation is TimeSimulation tsim && tsim.Method.BaseTime.Equals(0.0)) ||
                state.Init == InitializationModes.Fix && !_bp.Off)
            {
                // General iteration
                vbs = _mbp.MosfetType * (state.Solution[BulkNode] - state.Solution[SourceNodePrime]);
                vgs = _mbp.MosfetType * (state.Solution[GateNode] - state.Solution[SourceNodePrime]);
                vds = _mbp.MosfetType * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);

                // now some common crunching for some more useful quantities
                vbd = vbs - vds;
                vgd = vgs - vds;
                var vgdo = VoltageGs - VoltageDs;
                von = _mbp.MosfetType * Von;

                /*
				 * limiting
				 * we want to keep device voltages from changing
				 * so fast that the exponentials churn out overflows
				 * and similar rudeness
				 */
                if (VoltageDs >= 0)
                {
                    vgs = Transistor.LimitFet(vgs, VoltageGs, von);
                    vds = vgs - vgd;
                    vds = Transistor.LimitVoltageDs(vds, VoltageDs);
                }
                else
                {
                    vgd = Transistor.LimitFet(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.LimitVoltageDs(-vds, -VoltageDs);
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vbs = Transistor.LimitJunction(vbs, VoltageBs, vt, _temp.SourceVCritical, out check);
                }
                else
                {
                    vbd = Transistor.LimitJunction(vbd, VoltageBd, vt, _temp.DrainVCritical, out check);
                    vbs = vbd + vds;
                }
            }
            else
            {
                /* ok - not one of the simple cases, so we have to
				 * look at all of the possibilities for why we were
				 * called.  We still just initialize the three voltages
				 */
                if (state.Init == InitializationModes.Junction && !_bp.Off)
                {
                    vds = _mbp.MosfetType * _bp.InitialVoltageDs;
                    vgs = _mbp.MosfetType * _bp.InitialVoltageGs;
                    vbs = _mbp.MosfetType * _bp.InitialVoltageBs;

                    // TODO: At some point, check what this is supposed to do
                    if (vds.Equals(0) && vgs.Equals(0) && vbs.Equals(0) && (state.UseDc || !state.UseIc))
                    {
                        vbs = -1;
                        vgs = _mbp.MosfetType * _temp.TempVt0;
                        vds = 0;
                    }
                }
                else
                {
                    vbs = vgs = vds = 0;
                }
            }

            /*
			 * now all the preliminaries are over - we can start doing the
			 * real work
			 */
            vbd = vbs - vds;
            vgd = vgs - vds;

            /*
			 * bulk - source and bulk - drain diodes
			 * here we just evaluate the ideal diode current and the
			 * corresponding derivative (conductance).
			 */
            if (vbs <= 0)
            {
                CondBs = sourceSatCur / vt;
                BsCurrent = CondBs * vbs;
                CondBs += _baseConfig.Gmin;
            }
            else
            {
                var evbs = Math.Exp(Math.Min(Transistor.MaximumExponentArgument, vbs / vt));
                CondBs = sourceSatCur * evbs / vt + _baseConfig.Gmin;
                BsCurrent = sourceSatCur * (evbs - 1);
            }
            if (vbd <= 0)
            {
                CondBd = drainSatCur / vt;
                BdCurrent = CondBd * vbd;
                CondBd += _baseConfig.Gmin;
            }
            else
            {
                var evbd = Math.Exp(Math.Min(Transistor.MaximumExponentArgument, vbd / vt));
                CondBd = drainSatCur * evbd / vt + _baseConfig.Gmin;
                BdCurrent = drainSatCur * (evbd - 1);
            }

            /* now to determine whether the user was able to correctly
			 * identify the source and drain of his device
			 */
            if (vds >= 0)
            {
                // normal mode
                Mode = 1;
            }
            else
            {
                // inverse mode
                Mode = -1;
            }

            {
                /*
				 * this block of code evaluates the drain current and its
				 * derivatives using the shichman - hodges model and the
				 * charges associated with the gate, channel and bulk for
				 * mosfets
				 */

                /* the following 4 variables are local to this code block until
				 * it is obvious that they can be made global
				 */
                double arg;
                double sarg;

                if ((Mode > 0 ? vbs : vbd) <= 0)
                {
                    sarg = Math.Sqrt(_temp.TempPhi - (Mode > 0 ? vbs : vbd));
                }
                else
                {
                    sarg = Math.Sqrt(_temp.TempPhi);
                    sarg = sarg - (Mode > 0 ? vbs : vbd) / (sarg + sarg);
                    sarg = Math.Max(0, sarg);
                }
                von = _temp.TempVoltageBi * _mbp.MosfetType + _mbp.Gamma * sarg;
                var vgst = (Mode > 0 ? vgs : vgd) - von;
                vdsat = Math.Max(vgst, 0);
                if (sarg <= 0)
                {
                    arg = 0;
                }
                else
                {
                    arg = _mbp.Gamma / (sarg + sarg);
                }
                if (vgst <= 0)
                {
                    /*
					 * cutoff region
					 */
                    cdrain = 0;
                    Transconductance = 0;
                    CondDs = 0;
                    TransconductanceBs = 0;
                }
                else
                {
                    /*
					 * saturation region
					 */
                    var betap = beta * (1 + _mbp.Lambda * (vds * Mode));
                    if (vgst <= vds * Mode)
                    {
                        cdrain = betap * vgst * vgst * .5;
                        Transconductance = betap * vgst;
                        CondDs = _mbp.Lambda * beta * vgst * vgst * .5;
                        TransconductanceBs = Transconductance * arg;
                    }
                    else
                    {
                        /*
						* linear region
						*/
                        cdrain = betap * (vds * Mode) * (vgst - .5 * (vds * Mode));
                        Transconductance = betap * (vds * Mode);
                        CondDs = betap * (vgst - vds * Mode) + _mbp.Lambda * beta * (vds * Mode) * (vgst - .5 * (vds * Mode));
                        TransconductanceBs = Transconductance * arg;
                    }
                }
                /*
				 * finished
				 */
            }

            /* now deal with n vs p polarity */
            Von = _mbp.MosfetType * von;
            SaturationVoltageDs = _mbp.MosfetType * vdsat;

            /*
			 * COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			 */
            DrainCurrent = Mode * cdrain - BdCurrent;

            /*
			 * check convergence
			 */
            if (!_bp.Off || state.Init != InitializationModes.Fix)
            {
                if (check == 1)
                    state.IsConvergent = false;
            }

            VoltageBs = vbs;
            VoltageBd = vbd;
            VoltageGs = vgs;
            VoltageDs = vds;

            /*
			 * load current vector
			 */
            var ceqbs = _mbp.MosfetType * (BsCurrent - (CondBs - _baseConfig.Gmin) * vbs);
            var ceqbd = _mbp.MosfetType * (BdCurrent - (CondBd - _baseConfig.Gmin) * vbd);
            if (Mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = _mbp.MosfetType * (cdrain - CondDs * vds - Transconductance * vgs - TransconductanceBs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -_mbp.MosfetType * (cdrain - CondDs * -vds - Transconductance * vgd - TransconductanceBs * vbd);
            }
            BulkPtr.Value -= ceqbs + ceqbd;
            DrainPrimePtr.Value += ceqbd - cdreq;
            SourcePrimePtr.Value += cdreq + ceqbs;

            // Load Y-matrix
            DrainDrainPtr.Value += _temp.DrainConductance;
            SourceSourcePtr.Value += _temp.SourceConductance;
            BulkBulkPtr.Value += CondBd + CondBs;
            DrainPrimeDrainPrimePtr.Value += _temp.DrainConductance + CondDs + CondBd + xrev * (Transconductance + TransconductanceBs);
            SourcePrimeSourcePrimePtr.Value += _temp.SourceConductance + CondDs + CondBs + xnrm * (Transconductance + TransconductanceBs);
            DrainDrainPrimePtr.Value += -_temp.DrainConductance;
            SourceSourcePrimePtr.Value += -_temp.SourceConductance;
            BulkDrainPrimePtr.Value -= CondBd;
            BulkSourcePrimePtr.Value -= CondBs;
            DrainPrimeDrainPtr.Value += -_temp.DrainConductance;
            DrainPrimeGatePtr.Value += (xnrm - xrev) * Transconductance;
            DrainPrimeBulkPtr.Value += -CondBd + (xnrm - xrev) * TransconductanceBs;
            DrainPrimeSourcePrimePtr.Value += -CondDs - xnrm * (Transconductance + TransconductanceBs);
            SourcePrimeGatePtr.Value += -(xnrm - xrev) * Transconductance;
            SourcePrimeSourcePtr.Value += -_temp.SourceConductance;
            SourcePrimeBulkPtr.Value += -CondBs - (xnrm - xrev) * TransconductanceBs;
            SourcePrimeDrainPrimePtr.Value += -CondDs - xrev * (Transconductance + TransconductanceBs);
        }
    }
}
