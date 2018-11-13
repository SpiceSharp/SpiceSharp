using System;
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
            base.Setup(simulation, provider);
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

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
        /// Execute behavior
        /// </summary>
        protected override double Evaluate(double vgs, double vds, double vbs)
        {
            double von;
            double vdsat, cdrain;
            var effectiveLength = _bp.Length - 2 * _mbp.LateralDiffusion;
            var beta = _temp.TempTransconductance * _bp.Width / effectiveLength;
            
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

                if (vbs <= 0)
                {
                    sarg = Math.Sqrt(_temp.TempPhi - vbs);
                }
                else
                {
                    sarg = Math.Sqrt(_temp.TempPhi);
                    sarg = sarg - vbs / (sarg + sarg);
                    sarg = Math.Max(0, sarg);
                }
                von = _temp.TempVoltageBi * _mbp.MosfetType + _mbp.Gamma * sarg;
                var vgst = vgs - von;
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
                    var betap = beta * (1 + _mbp.Lambda * vds);
                    if (vgst <= vds)
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
                        cdrain = betap * vds * (vgst - .5 * vds);
                        Transconductance = betap * vds;
                        CondDs = betap * (vgst - vds) + _mbp.Lambda * beta * vds * (vgst - .5 * vds);
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
            return cdrain;
        }
    }
}
