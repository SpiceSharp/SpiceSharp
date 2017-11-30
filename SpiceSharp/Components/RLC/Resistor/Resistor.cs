using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A resistor
    /// </summary>
    [SpicePins("R+", "R-")]
    public class Resistor : CircuitComponent<Resistor>
    {
        /// <summary>
        /// Register default behaviors of the resistor
        /// </summary>
        static Resistor()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(Resistor), typeof(ComponentBehaviors.ResistorLoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(Resistor), typeof(ComponentBehaviors.ResistorAcBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(Resistor), typeof(ComponentBehaviors.ResistorNoiseBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(Resistor), typeof(ComponentBehaviors.ResistorTemperatureBehavior));
        }

        /// <summary>
        /// Set the model for the resistor
        /// </summary>
        /// <param name="model"></param>
        public void SetModel(ResistorModel model) => Model = model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("temp"), SpiceInfo("Instance operating temperature", Interesting = false)]
        public double RES_TEMP
        {
            get => REStemp - Circuit.CONSTCtoK;
            set => REStemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter REStemp { get; } = new Parameter(300.15);
        [SpiceName("resistance"), SpiceInfo("Resistance", IsPrincipal = true)]
        public Parameter RESresist { get; } = new Parameter();
        [SpiceName("w"), SpiceInfo("Width", Interesting = false)]
        public Parameter RESwidth { get; } = new Parameter();
        [SpiceName("l"), SpiceInfo("Length", Interesting = false)]
        public Parameter RESlength { get; } = new Parameter();
        [SpiceName("i"), SpiceInfo("Current")]
        public double GetCurrent(Circuit ckt) => (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) * RESconduct;
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt) => (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) *
            (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) * RESconduct;

        /// <summary>
        /// Nodes
        /// </summary>
        public int RESposNode { get; private set; }
        public int RESnegNode { get; private set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        internal MatrixElement RESposPosPtr { get; private set; }
        internal MatrixElement RESnegNegPtr { get; private set; }
        internal MatrixElement RESposNegPtr { get; private set; }
        internal MatrixElement RESnegPosPtr { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        internal double RESconduct = 0.0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        public Resistor(CircuitIdentifier name) : base(name) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="res">The resistance</param>
        public Resistor(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, double res) : base(name)
        {
            Connect(pos, neg);
            RESresist.Set(res);
        }

        /// <summary>
        /// Setup the resistor
        /// </summary>
        /// <param name="ckt"></param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            RESposNode = nodes[0].Index;
            RESnegNode = nodes[1].Index;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            RESposPosPtr = matrix.GetElement(RESposNode, RESposNode);
            RESnegNegPtr = matrix.GetElement(RESnegNode, RESnegNode);
            RESposNegPtr = matrix.GetElement(RESposNode, RESnegNode);
            RESnegPosPtr = matrix.GetElement(RESnegNode, RESposNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup(Circuit ckt)
        {
            // Remove references
            RESposPosPtr = null;
            RESnegNegPtr = null;
            RESposNegPtr = null;
            RESnegPosPtr = null;
        }
    }
}
