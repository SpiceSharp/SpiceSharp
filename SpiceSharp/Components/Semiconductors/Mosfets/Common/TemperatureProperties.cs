using System;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// Temperature-dependent properties.
    /// </summary>
    public class TemperatureProperties
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public double TempSurfaceMobility { get; private set; }
        public double TempPhi { get; private set; }
        public double TempVbi { get; private set; }
        public double TempBulkPotential { get; private set; }
        public double TempTransconductance { get; private set; }
        public double TempVt0 { get; private set; }
        public double TempVt { get; private set; }
        public double DrainSatCurrent { get; private set; }
        public double SourceSatCurrent { get; private set; }
        public double DrainVCritical { get; private set; }
        public double SourceVCritical { get; private set; }
        public double DrainConductance { get; private set; }
        public double SourceConductance { get; private set; }
        public double Cbs { get; private set; }
        public double CbsSidewall { get; private set; }
        public double Cbd { get; private set; }
        public double CbdSidewall { get; private set; }
        public double TempCbs { get; private set; }
        public double TempCbd { get; private set; }
        public double TempJctCap { get; private set; }
        public double TempJctCapSidewall { get; private set; }
        public double TempDepCap { get; private set; }
        public double F2d { get; private set; }
        public double F3d { get; private set; }
        public double F4d { get; private set; }
        public double F2s { get; private set; }
        public double F3s { get; private set; }
        public double F4s { get; private set; }
        public double EffectiveLength { get; private set; }
        public double OxideCap { get; private set; }
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
            TempVt = p.Temperature * Constants.KOverQ;
            var ratio = p.Temperature / mp.NominalTemperature;
            var fact2 = p.Temperature / Constants.ReferenceTemperature;
            var kt = p.Temperature * Constants.Boltzmann;
            var egfet = 1.16 - (7.02e-4 * p.Temperature * p.Temperature) /
                    (p.Temperature + 1108);
            var arg = -egfet / (kt + kt) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature + Constants.ReferenceTemperature));
            var pbfact = -2 * TempVt * (1.5 * Math.Log(fact2) + Constants.Charge * arg);

            if (p.Length - 2 * mp.LateralDiffusion <= 0)
                SpiceSharpWarning.Warning(this, "{0}: effective channel length less than zero".FormatString(name));
            var ratio4 = ratio * Math.Sqrt(ratio);
            TempTransconductance = mp.Transconductance / ratio4;
            TempSurfaceMobility = mp.SurfaceMobility / ratio4;
            var phio = (mp.Phi - mprp.PbFactor1) / mprp.Factor1;
            TempPhi = fact2 * phio + pbfact;
            TempVbi = mp.Vt0 - mp.MosfetType * (mp.Gamma * Math.Sqrt(mp.Phi))
                    + .5 * (mprp.EgFet1 - egfet) + mp.MosfetType * .5 * (TempPhi - mp.Phi);
            TempVt0 = TempVbi + mp.MosfetType * mp.Gamma * Math.Sqrt(TempPhi);
            var TempSaturationCurrent = mp.JunctionSatCur * Math.Exp(-egfet / TempVt + mprp.EgFet1 / mprp.Vtnom);
            var TempSaturationCurrentDensity = mp.JunctionSatCurDensity * Math.Exp(-egfet / TempVt + mprp.EgFet1 / mprp.Vtnom);
            var pbo = (mp.BulkJunctionPotential - mprp.PbFactor1) / mprp.Factor1;
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

            var gmaold = (mp.BulkJunctionPotential - pbo) / pbo;
            var capfact = 1 / (1 + (mp.BulkJunctionBotGradingCoefficient *
                    ((4e-4 * (mp.NominalTemperature - Constants.ReferenceTemperature)) - gmaold)));
            TempCbd = mp.CapBd * capfact;
            TempCbs = mp.CapBs * capfact;
            TempJctCap = mp.BulkCapFactor * capfact;
            capfact = 1 / (1 + (mp.BulkJunctionSideGradingCoefficient *
                        ((4e-4 * (mp.NominalTemperature - Constants.ReferenceTemperature)) - gmaold)));
            TempJctCapSidewall = mp.SidewallCapFactor * capfact;
            TempBulkPotential = (fact2 * pbo) + pbfact;
            var gmanew = (TempBulkPotential - pbo) / pbo;
            capfact = 1 + (mp.BulkJunctionBotGradingCoefficient *
                        ((4e-4 * (p.Temperature - Constants.ReferenceTemperature)) - gmanew));
            TempCbd *= capfact;
            TempCbs *= capfact;
            TempJctCap *= capfact;
            capfact = 1 + (mp.BulkJunctionSideGradingCoefficient *
                    ((4e-4 * (p.Temperature - Constants.ReferenceTemperature)) - gmanew));
            TempJctCapSidewall *= capfact;
            TempDepCap = mp.ForwardCapDepletionCoefficient * TempBulkPotential;

            double czbd, czbdsw;
            if (mp.CapBd.Given)
                czbd = TempCbd * p.ParallelMultiplier;
            else
            {
                if (mp.BulkCapFactor.Given)
                    czbd = TempJctCap * p.ParallelMultiplier * p.DrainArea;
                else
                    czbd = 0;
            }
            if (mp.SidewallCapFactor.Given)
                czbdsw = TempJctCapSidewall * p.DrainPerimeter * p.ParallelMultiplier;
            else
                czbdsw = 0;
            arg = 1 - mp.ForwardCapDepletionCoefficient;
            var sarg = Math.Exp((-mp.BulkJunctionBotGradingCoefficient) * Math.Log(arg));
            var sargsw = Math.Exp((-mp.BulkJunctionSideGradingCoefficient) * Math.Log(arg));
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
                    czbs = TempJctCap * p.SourceArea * p.ParallelMultiplier;
                else
                    czbs = 0;
            }
            if (mp.SidewallCapFactor.Given)
                czbssw = TempJctCapSidewall * p.SourcePerimeter * p.ParallelMultiplier;
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
            OxideCap = mp.OxideCapFactor * EffectiveLength * p.Width;
        }
    }
}
