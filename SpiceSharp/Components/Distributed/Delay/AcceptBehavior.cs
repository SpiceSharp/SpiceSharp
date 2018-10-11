using System;
using System.Collections.Generic;
using System.Text;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DelayBehaviors
{
    public class AcceptBehavior : BaseAcceptBehavior
    {
        private BaseParameters _bp;
        private TransientBehavior _tran;

        public AcceptBehavior(string name) : base(name)
        {
        }

        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            _bp = provider.GetParameterSet<BaseParameters>();
            _tran = provider.GetBehavior<TransientBehavior>();
        }

        public override void Accept(TimeSimulation simulation)
        {
            // Update the input tracker
            _tran.InputTracker.AcceptSolution(simulation);
        }
    }
}
