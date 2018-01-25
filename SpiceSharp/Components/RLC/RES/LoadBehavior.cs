using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Components.RES;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Behaviors.RES
{
    /// <summary>
    /// General behavior for <see cref="Components.Resistor"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("v"), PropertyInfo("Voltage")]
        public double GetVoltage(State state) => state.Solution[RESposNode] - state.Solution[RESnegNode];
        [PropertyName("i"), PropertyInfo("Current")]
        public double GetCurrent(State state) => (state.Solution[RESposNode] - state.Solution[RESnegNode]) * RESconduct;
        [PropertyName("p"), PropertyInfo("Power")]
        public double GetPower(State state)
        {
            double v = state.Solution[RESposNode] - state.Solution[RESnegNode];
            return v * v * RESconduct;
        }

        /// <summary>
        /// Nodes
        /// </summary>
        public int RESposNode { get; protected set; }
        public int RESnegNode { get; protected set; }

        /// <summary>
        /// Conductance
        /// </summary>
        public double RESconduct { get; protected set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement RESposPosPtr { get; private set; }
        protected MatrixElement RESnegNegPtr { get; private set; }
        protected MatrixElement RESposNegPtr { get; private set; }
        protected MatrixElement RESnegPosPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create export method
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string property)
        {
            switch (property)
            {
                case "v": return GetVoltage;
                case "c":
                case "i": return GetCurrent;
                case "p": return GetPower;
                default: return null;
            }
        }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            var p = provider.GetParameters<BaseParameters>();

            // Depending on whether or not the resistance is given, get behaviors
            if (!p.RESresist.Given)
            {
                var temp = provider.GetBehavior<TemperatureBehavior>();
                RESconduct = temp.RESconduct;
            }
            else
            {
                if (p.RESresist.Value < 1e-12)
                    RESconduct = 1e12;
                else
                    RESconduct = 1.0 / p.RESresist.Value;
            }
        }
        
        /// <summary>
        /// Connect the behavior to nodes
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            RESposNode = pins[0];
            RESnegNode = pins[1];
        }
        
        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            // Get matrix elements
            RESposPosPtr = matrix.GetElement(RESposNode, RESposNode);
            RESnegNegPtr = matrix.GetElement(RESnegNode, RESnegNode);
            RESposNegPtr = matrix.GetElement(RESposNode, RESnegNode);
            RESnegPosPtr = matrix.GetElement(RESnegNode, RESposNode);
        }
        
        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            RESposPosPtr = null;
            RESnegNegPtr = null;
            RESposNegPtr = null;
            RESnegPosPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            RESposPosPtr.Add(RESconduct);
            RESnegNegPtr.Add(RESconduct);
            RESposNegPtr.Sub(RESconduct);
            RESnegPosPtr.Sub(RESconduct);
        }
    }
}
