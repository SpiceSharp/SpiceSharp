using System;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// Temperature-dependent properties.
    /// </summary>
    public class TemperatureProperties
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public double TempSurfaceMobility { get; set; }
        public double TempPhi { get; set; }
        public double TempVbi { get; set; }
        public double TempBulkPotential { get; set; }
        public double TempTransconductance { get; set; }
        public double TempVt0 { get; set; }
        public double TempVt { get; set; }
        public double DrainSatCurrent { get; set; }
        public double SourceSatCurrent { get; set; }
        public double DrainVCritical { get; set; }
        public double SourceVCritical { get; set; }
        public double DrainConductance { get; set; }
        public double SourceConductance { get; set; }
        public double Cbs { get; set; }
        public double CbsSidewall { get; set; }
        public double Cbd { get; set; }
        public double CbdSidewall { get; set; }
        public double TempCbs { get; set; }
        public double TempCbd { get; set; }
        public double TempCj { get; set; }
        public double TempCjsw { get; set; }
        public double TempDepCap { get; set; }
        public double F2d { get; set; }
        public double F3d { get; set; }
        public double F4d { get; set; }
        public double F2s { get; set; }
        public double F3s { get; set; }
        public double F4s { get; set; }
        public double EffectiveLength { get; set; }
        public double OxideCap { get; set; }
        public double TempSatCurDensity { get; set; }
        public double TempSatCur { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Updates the specified properties.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="p">The parameters of the transistor.</param>
        /// <param name="mp">The model parameters.</param>
        /// <param name="mprp">The model properties.</param>
        public void Update(string name, Parameters p, ModelParameters mp, ModelProperties mprp)
        {
            if (!mp.Transconductance.Given)
                mp.Transconductance = new GivenParameter<double>(mp.SurfaceMobility * mprp.OxideCapFactor * 1e-4, false);
            TempVt = p.Temperature * Constants.KOverQ;
            double ratio = p.Temperature / mp.NominalTemperature;
            double fact2 = p.Temperature / Constants.ReferenceTemperature;
            double kt = p.Temperature * Constants.Boltzmann;
            double egfet = 1.16 - (7.02e-4 * p.Temperature * p.Temperature) /
                    (p.Temperature + 1108);
            double arg = -egfet / (kt + kt) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature + Constants.ReferenceTemperature));
            double pbfact = -2 * TempVt * (1.5 * Math.Log(fact2) + Constants.Charge * arg);
            OxideCap = mprp.OxideCapFactor * EffectiveLength * p.ParallelMultiplier * p.Width;

            if (p.Length - 2 * mp.LateralDiffusion <= 0)
                SpiceSharpWarning.Warning(this, "{0}: effective channel length less than zero".FormatString(name));
            double ratio4 = ratio * Math.Sqrt(ratio);
            TempTransconductance = mp.Transconductance / ratio4;
            TempSurfaceMobility = mp.SurfaceMobility / ratio4;
            double phio = (mp.Phi - mprp.PbFactor1) / mprp.Factor1;
            TempPhi = fact2 * phio + pbfact;
            TempVbi = mp.Vt0 - mp.MosfetType * (mp.Gamma * Math.Sqrt(mp.Phi)) + .5 * (mprp.EgFet1 - egfet) + mp.MosfetType * .5 * (TempPhi - mp.Phi);
            TempVt0 = TempVbi + mp.MosfetType * mp.Gamma * Math.Sqrt(TempPhi);
            double TempSaturationCurrent = mp.JunctionSatCur * Math.Exp(-egfet / TempVt + mprp.EgFet1 / mprp.Vtnom);
            double TempSaturationCurrentDensity = mp.JunctionSatCurDensity * Math.Exp(-egfet / TempVt + mprp.EgFet1 / mprp.Vtnom);
            double pbo = (mp.BulkJunctionPotential - mprp.PbFactor1) / mprp.Factor1;
            TempBulkPotential = fact2 * pbo + pbfact;

            if ((TempSaturationCurrentDensity == 0) ||
                    (p.DrainArea == 0) ||
                    (p.SourceArea == 0))
            {
                SourceVCritical = DrainVCritical =
                       TempVt * Math.Log(TempVt / (Constants.Root2 * p.ParallelMultiplier * TempSaturationCurrent));
            }
            else
            {
                DrainVCritical =
                        TempVt * Math.Log(TempVt / (Constants.Root2 *
                        p.ParallelMultiplier *
                        TempSaturationCurrentDensity * p.DrainArea));
                SourceVCritical =
                        TempVt * Math.Log(TempVt / (Constants.Root2 *
                        p.ParallelMultiplier *
                        TempSaturationCurrentDensity * p.SourceArea));
            }

            if (mp.DrainResistance.Given)
            {
                if (mp.DrainResistance != 0)
                    DrainConductance = p.ParallelMultiplier / mp.DrainResistance;
                else
                    DrainConductance = 0;
            }
            else if (mp.SheetResistance.Given)
            {
                if (mp.SheetResistance != 0)
                    DrainConductance = p.ParallelMultiplier / (mp.SheetResistance * p.DrainSquares);
                else
                    DrainConductance = 0;
            }
            else
                DrainConductance = 0;

            if (mp.SourceResistance.Given)
            {
                if (mp.SourceResistance != 0)
                    SourceConductance = p.ParallelMultiplier / mp.SourceResistance;
                else
                    SourceConductance = 0;
            }
            else if (mp.SheetResistance.Given)
            {
                if ((mp.SheetResistance != 0) && (p.SourceSquares != 0))
                    SourceConductance = p.ParallelMultiplier / (mp.SheetResistance * p.SourceSquares);
                else
                    SourceConductance = 0;
            }
            else
                SourceConductance = 0;

            EffectiveLength = p.Length - (2 * mp.LateralDiffusion);

            double gmaold = (mp.BulkJunctionPotential - pbo) / pbo;
            double capfact = 1 / (1 + (mp.BulkJunctionBotGradingCoefficient *
                    ((4e-4 * (mp.NominalTemperature - Constants.ReferenceTemperature)) - gmaold)));
            TempCbd = mp.CapBd * capfact;
            TempCbs = mp.CapBs * capfact;
            TempCj = mp.BulkCapFactor * capfact;
            capfact = 1 / (1 + (mp.BulkJunctionSideGradingCoefficient *
                        ((4e-4 * (mp.NominalTemperature - Constants.ReferenceTemperature)) - gmaold)));
            TempCjsw = mp.SidewallCapFactor * capfact;
            TempBulkPotential = (fact2 * pbo) + pbfact;
            double gmanew = (TempBulkPotential - pbo) / pbo;
            capfact = 1 + (mp.BulkJunctionBotGradingCoefficient *
                        ((4e-4 * (p.Temperature - Constants.ReferenceTemperature)) - gmanew));
            TempCbd *= capfact;
            TempCbs *= capfact;
            TempCj *= capfact;
            capfact = 1 + (mp.BulkJunctionSideGradingCoefficient *
                    ((4e-4 * (p.Temperature - Constants.ReferenceTemperature)) - gmanew));
            TempCjsw *= capfact;
            TempDepCap = mp.ForwardCapDepletionCoefficient * TempBulkPotential;

            double czbd, czbdsw;
            if (mp.CapBd.Given)
                czbd = TempCbd * p.ParallelMultiplier;
            else
            {
                if (mp.BulkCapFactor.Given)
                    czbd = TempCj * p.ParallelMultiplier * p.DrainArea;
                else
                    czbd = 0;
            }
            if (mp.SidewallCapFactor.Given)
                czbdsw = TempCjsw * p.DrainPerimeter * p.ParallelMultiplier;
            else
                czbdsw = 0;
            arg = 1 - mp.ForwardCapDepletionCoefficient;
            double sarg = Math.Exp((-mp.BulkJunctionBotGradingCoefficient) * Math.Log(arg));
            double sargsw = Math.Exp((-mp.BulkJunctionSideGradingCoefficient) * Math.Log(arg));
            Cbd = czbd;
            CbdSidewall = czbdsw;
            F2d = (czbd * (1 - (mp.ForwardCapDepletionCoefficient *
                        (1 + mp.BulkJunctionBotGradingCoefficient))) * sarg / arg)
                    + (czbdsw * (1 - (mp.ForwardCapDepletionCoefficient *
                        (1 + mp.BulkJunctionSideGradingCoefficient))) *
                        sargsw / arg);
            F3d = (czbd * mp.BulkJunctionBotGradingCoefficient * sarg / arg /
                        TempBulkPotential)
                    + (czbdsw * mp.BulkJunctionSideGradingCoefficient * sargsw / arg /
                        TempBulkPotential);
            F4d = (czbd * TempBulkPotential * (1 - (arg * sarg)) /
                        (1 - mp.BulkJunctionBotGradingCoefficient))
                    + (czbdsw * TempBulkPotential * (1 - (arg * sargsw)) /
                        (1 - mp.BulkJunctionSideGradingCoefficient))
                    - (F3d / 2 *
                        (TempDepCap * TempDepCap))
                    - (TempDepCap * F2d);

            double czbs, czbssw;
            if (mp.CapBs.Given)
                czbs = TempCbs * p.ParallelMultiplier;
            else
            {
                if (mp.BulkCapFactor.Given)
                    czbs = TempCj * p.SourceArea * p.ParallelMultiplier;
                else
                    czbs = 0;
            }
            if (mp.SidewallCapFactor.Given)
                czbssw = TempCjsw * p.SourcePerimeter * p.ParallelMultiplier;
            else
                czbssw = 0;
            arg = 1 - mp.ForwardCapDepletionCoefficient;
            sarg = Math.Exp((-mp.BulkJunctionBotGradingCoefficient) * Math.Log(arg));
            sargsw = Math.Exp((-mp.BulkJunctionSideGradingCoefficient) * Math.Log(arg));
            Cbs = czbs;
            CbsSidewall = czbssw;
            F2s = (czbs * (1 - (mp.ForwardCapDepletionCoefficient *
                        (1 + mp.BulkJunctionBotGradingCoefficient))) * sarg / arg)
                    + (czbssw * (1 - (mp.ForwardCapDepletionCoefficient *
                        (1 + mp.BulkJunctionSideGradingCoefficient))) *
                        sargsw / arg);
            F3s = (czbs * mp.BulkJunctionBotGradingCoefficient * sarg / arg /
                        TempBulkPotential)
                    + (czbssw * mp.BulkJunctionSideGradingCoefficient * sargsw / arg /
                        TempBulkPotential);
            F4s = (czbs * TempBulkPotential * (1 - (arg * sarg)) /
                        (1 - mp.BulkJunctionBotGradingCoefficient))
                    + (czbssw * TempBulkPotential * (1 - (arg * sargsw)) /
                        (1 - mp.BulkJunctionSideGradingCoefficient))
                    - (F3s / 2 *
                        (TempDepCap * TempDepCap))
                    - (TempDepCap * F2s);
        }
    }
}
